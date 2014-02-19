using System;
using kasthack.Tools.LittleSMS;
namespace Test {
    public static class Program {
        static void Main( string[] args ) {
            var api = new Littlesms();
            if ( api.Login( R("Username"), R("Pass") ) ) {
                Console.WriteLine( "Login OK." );
                Console.WriteLine( "Getting balance..." );
                var balance = api.GetBalance();
                Console.WriteLine( "Balance is {0} RUR", balance );
                Console.WriteLine( "Sending SMS to coder..." );
                var msgId = api.Send( R("Phone"), R("Sender"), R("Message") );
                Console.WriteLine( "Message sent getting status..." );
                
                var status = api.GetStatus( msgId );
                Console.WriteLine( "Status is {0}", status );
            }
            else {
                Console.WriteLine( "Login fail" );
            }
            Console.WriteLine( "Exiting" );
            Console.WriteLine( "Press enter to exit" );
            Console.ReadLine();
        }

        private static string R(string msg) {
            switch ( msg ) {
                case "Phone":
                    return "<phone>";
                case "Sender":
                    return "<sender>";
                case "Message":
                    return "<msg>";
                case "Pass":
                    return "<pass>";
                case "Username":
                    return "<username>";
            }
            Console.Write(msg);
            Console.Write(": ");
            return Console.ReadLine();
        }
    }
}
