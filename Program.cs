using System;

namespace Redbox_Mobile_Command_Center_Server {
    class Program {
        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            // prevent closing of app
            while (true) { }
        }

        public static string OnServerIncomingData(string message) {
            return "a";
        }
    }
}
