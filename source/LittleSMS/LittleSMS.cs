using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace kasthack.Tools.LittleSMS {
    public enum SMSStatus {
        Unknown,
        Rejected,
        Undeliverable,
        Deleted,
        Expired,
        Delivered,
        Accepted,
        Enroute,
        Enqueued,
        Error
    }
    public class Littlesms {

        private String _loginVar;
        private String _passVar;

        private const string LittleSMSRoot = @"https://littlesms.ru/";


        static readonly Dictionary<string, SMSStatus> _statuses = new Dictionary<string, SMSStatus>
        #region Statuses
        {
            {"enqueued", SMSStatus.Enqueued},
            {"enroute",SMSStatus.Enroute},
            {"accepted",SMSStatus.Accepted},
            {"delivered", SMSStatus.Delivered},
            {"expired",SMSStatus.Expired},
            {"deleted",SMSStatus.Deleted},
            {"undeliverable",SMSStatus.Undeliverable},
            {"rejected",SMSStatus.Rejected},
            {"unknown",SMSStatus.Unknown},
        };
        #endregion
        /// <summary>
        /// Test connection
        /// </summary>
        /// <returns></returns>
        public bool ConnectionTest() {
            try {
                WebRequest.Create( LittleSMSRoot );
                return true;
            }
            catch ( WebException ) {
                return false;
            }
        }
        /// <summary>
        /// Auth
        /// </summary>
        /// <param name="login">login</param>
        /// <param name="apikey">pass</param>
        /// <returns></returns>
        public bool Login( String login, String apikey ) {
            if ( JObject.Parse(  RequestString(
                    string.Format(
                        @"{0}api/user/balance?user={1}&apikey={2}",
                        LittleSMSRoot,
                        login,
                        apikey)
                ))["status"].Value<string>()!="success") return false;
            this._loginVar = login;
            this._passVar = apikey;
            return true;
        }
        /// <summary>
        /// Get balance
        /// </summary>
        /// <returns></returns>
        public double GetBalance() {
            // ReSharper disable once AssignNullToNotNullAttribute
            var s = RequestString(
                string.Format(
                    @"{0}api/user/balance?user={1}&apikey={2}",
                    LittleSMSRoot,
                    this._loginVar,
                    this._passVar
                )
            );
            var json = JObject.Parse( s );
            return json[ "status" ].Value<string>() == "success" ? json[ "balance" ].Value<double>() : 0;
        }
        /// <summary>
        /// Get SMS status by ID
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public SMSStatus GetStatus( int msgId ) {
            var jsonText = RequestString(
                string.Format(
                    @"{0}api/message/status?user={1}&apikey={2}&&messages_id={3}",
                    LittleSMSRoot,
                    this._loginVar,
                    this._passVar,
                    msgId
            ) );
            var json = JObject.Parse( jsonText );
            SMSStatus stat;
            return Enum.TryParse(
                json[ "messages" ][ msgId.ToString(CultureInfo.InvariantCulture)].Value<string>().Trim(), true,out stat )?
                stat:
                SMSStatus.Unknown;
        }
        /// <summary>
        /// Send SMS
        /// </summary>
        /// <param name="phone">target number</param>
        /// <param name="sender">send from</param>
        /// <param name="message">message</param>
        /// <returns>Message ID</returns>
        public int Send( String phone, String sender, String message ) {
            var s = RequestString(
                string.Format(
                    @"{0}api/message/send?user={1}&recipients={2}&message={3}&sender={4}&apikey={5}",
                    LittleSMSRoot,
                    this._loginVar,
                    phone,
                    message,
                    sender,
                    this._passVar
                )
            );
            var json = JObject.Parse( s );
            return json[ "status" ].Value<string>() == "success" ?
                json[ "messages_id" ].First.First.Value<int>() :
                0;
        }
        private static string RequestString( string url ) {
            var reqGET = WebRequest.Create( url );
            reqGET.Proxy = null;
            reqGET.Credentials = CredentialCache.DefaultCredentials;
            ServicePointManager.ServerCertificateValidationCallback = ( ( sender, certificate, chain, sslPolicyErrors ) => true );
            using ( var rs = reqGET.GetResponse().GetResponseStream() ) {
                using ( var s = new StreamReader( rs ) ) {
                    return s.ReadToEnd();
                }
            }
        }
    }
}