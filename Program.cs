using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public class Program {
        public static List<string> SplitByCRLF(string input) {
            // Split the string by \r\n
            return input.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        private static async Task<string> SendHALCommandAsync(string command) {
            // Create the connection instance
            HALConnection halConnection = new HALConnection("127.0.0.1", 7001);

            string commandResponse = null;
            int messageCount = 0;

            var tcs = new TaskCompletionSource<string>();

            // Subscribe to events
            halConnection.MessageReceived += (halMessage) =>
            {
                Console.WriteLine(halMessage);
                messageCount++;

                if (messageCount == 2) {
                    // Capture the second message (command response)
                    commandResponse = halMessage;
                    tcs.SetResult(commandResponse); // Set the result once we have the second response
                }
            };

            halConnection.Disconnected += () =>
            {
                Console.WriteLine("The client has disconnected from HAL.");
                tcs.TrySetCanceled(); // Cancel the TaskCompletionSource if disconnected
            };

            // Connect to HAL
            halConnection.Connect();

            // Send the command to HAL
            halConnection.SendMessage(command);

            // Wait for the second response (non-blocking async)
            try {
                return await tcs.Task; // Will return when the second response is received
            }
            catch (OperationCanceledException) {
                Console.WriteLine("Connection was closed or cancelled.");
                return null;
            }
            finally {
                halConnection.Disconnect();
            }
        }

        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            // prevent closing of app
            while (true) { }
        }

        public static async Task<string> OnServerIncomingData(string message) {
            string[] arguments = message.Split(' ');

            switch (arguments[0]) {
                case "ping-kiosk":
                    return "200";
                case "execute-command":
                    string command = arguments[1];

                    Console.WriteLine("Execute-Command");
                    Console.WriteLine(command);

                    switch (command) {
                        case "hal-startup":
                            string halResponse = await SendHALCommandAsync("SERVICE test-comm\r\n");

                            //split the response by CRLF (carriage return and line feed)
                            List<string> responseLines = SplitByCRLF(halResponse);
                            
                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halResponse = await SendHALCommandAsync("SERVICE get-kiosk-id\r\n");

                            //split the response by CRLF (carriage return and line feed)
                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            break;
                    }
                    break;
            }

            Console.WriteLine(message);
            return "Completed";
        }
    }
}
