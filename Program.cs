using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    class Program {
        private string SendHALCommand(string command) {
            //create the connection instance
            HALConnection halConnection = new HALConnection("127.0.0.1", 7001);

            string commandResponse = null;
            int messageCount = 0;

            //subscribe to events
            halConnection.MessageReceived += (halMessage) =>
            {
                messageCount++;

                if (messageCount == 2) {
                    //capture the second message (command response)
                    commandResponse = halMessage;
                }
            };

            halConnection.Disconnected += () =>
            {
                Console.WriteLine("The client has disconnected from HAL.");
            };

            //connect to HAL
            halConnection.Connect();

            //send the command to HAL
            halConnection.SendMessage(command);

            //wait for the second response (blocking until the response is received)
            while (commandResponse == null) {
                Thread.Sleep(10); //small delay to prevent busy-waiting
            }

            //disconnect and send the quit command
            halConnection.Disconnect();

            return commandResponse;
        }

        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            

            // prevent closing of app
            //while (true) { }
        }

        public async static Task<string> OnServerIncomingData(string message) {
            string[] arguments = message.Split(' ');

            switch(arguments[0]) {
                case "ping-kiosk":
                    return "200";
                case "execute-command":
                    string command = arguments[1];

                    switch (command) {
                        case "hal-startup":


                            break;
                    }
                    break;
            }

            Console.WriteLine(message);
            return "Completed";
        }
    }
}
