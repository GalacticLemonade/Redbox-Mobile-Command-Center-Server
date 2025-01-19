using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public class Program {
        static TCPClient client;

        public static List<string> SplitByCRLF(string input) {
            // Split the string by \r\n
            return input.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        private static async Task<string> SendHALCommandAsync(string command) {
            command = command + "\r\n";

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

            Thread.Sleep(500);

            Console.WriteLine("yo?? wake up??");

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
                Console.WriteLine("Response recieved. Disconnect from HAL.");
                halConnection.Disconnect();
            }
        }

        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            SetKioskIP();

            // prevent closing of app
            while (true) { }
        }

        private static async void SetKioskIP() {
            // get local ip
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostEntry(host);
            IPAddress ipv4Address = ip.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            if (ipv4Address != null) {
                Console.WriteLine(ipv4Address.ToString());
            }
            else {
                Console.WriteLine("No IPv4 address found.");
                return;
            }

            client = new TCPClient();
            await client.ConnectAsync("216.169.82.236", 11500);

            // Get Kiosk ID
            string halIDResponse = await SendHALCommandAsync("SERVICE get-kiosk-id");

            List<string> responseIDLines = SplitByCRLF(halIDResponse);

            if (responseIDLines.Count > 1 && responseIDLines[1].StartsWith("203")) {
                Console.WriteLine("Response code 203 received.");
            }
            else {
                Console.WriteLine("Invalid response.");
                return;
            }

            string kioskid = responseIDLines[0].Trim();

            if (kioskid.ToLower() == "unknown") {
                kioskid = "35618";
            }

            await client.SendMessageAsync("set-kiosk-addr " + kioskid + " " + ipv4Address.ToString() + ":11600");

            client.Disconnect();
        }

        public static async Task<string> OnServerIncomingData(string message) {
            string[] arguments = message.Split(' ');

            if (arguments.Length == 0)
                return "Invalid command.";

            string commandName = arguments[0];

            if (CommandRegistry.Commands.TryGetValue(commandName, out ICommand command)) {
                return await command.Run(arguments.Skip(1).ToArray());
            }
            else {
                return "Unknown command: " + commandName;
            }
        }
    }
}
