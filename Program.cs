using System;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    class Program {
        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            // prevent closing of app
            while (true) { }
        }

        public async static Task<string> OnServerIncomingData(string message) {
            string[] arguments = message.Split(' ');

            switch(arguments[0]) {
                case "ping-kiosk":
                    return "200";
            }

            Console.WriteLine(message);
            return "Completed";
        }
    }
}
