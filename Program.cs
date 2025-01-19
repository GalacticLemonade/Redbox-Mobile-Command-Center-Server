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

            switch (arguments[0]) {
                case "ping-kiosk":
                    return "200";
                case "execute-command":
                    string command = arguments[1];

                    //Console.WriteLine("Execute-Command");
                    //Console.WriteLine(command);

                    switch (command) {
                        case "hal-startup":
                            // Test Comm
                            string halResponse = await SendHALCommandAsync("SERVICE test-comm");

                            List<string> responseLines = SplitByCRLF(halResponse);
                            
                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Get Kiosk I
                            halResponse = await SendHALCommandAsync("SERVICE get-kiosk-id");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Check init job status
                            halResponse = await SendHALCommandAsync("JOB init-status");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // List all jobs (this is where parsing gets interesting!)
                            // appears to be jobid|label|type(?)|priority|unknown|status(?)|status pt2(?)|status pt3(?)|unknown|unknown
                            halResponse = await SendHALCommandAsync("JOB list");

                            //im guessing it's \r\n between jobs?

                            //responseLines = SplitByCRLF(halResponse);

                            //if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                            //Console.WriteLine("Response code 203 received.");
                            //}
                            //else {
                            //Console.WriteLine("Invalid response.");
                            //return "402";
                            //}

                            Console.WriteLine("Recieved job list.");

                            // Set diagnostic mode to true(?)
                            halResponse = await SendHALCommandAsync("SERVICE diagnostic-mode status: true");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Compile lua script 1
                            halResponse = await SendHALCommandAsync("PROGRAM compile path: @'C:\\Program Files\\Redbox\\MS HAL Tester\\bin\\Scripts\\ms-pull-in-dvd.hs' name: 'ms-pull-in-dvd' requires-client-connection: False");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Compile lua script 2
                            halResponse = await SendHALCommandAsync("PROGRAM compile path: @'C:\\Program Files\\Redbox\\MS HAL Tester\\bin\\Scripts\\ms-vend-disk-in-picker.hs' name: 'ms-vend-disk-in-picker' requires-client-connection: False");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received. Line 179.");
                            }
                            else {
                                Console.WriteLine("Invalid response. Line 179.");
                                return "402";
                            }

                            // Create a new configuration job
                            // also fun parsing!!
                            // appears to return as jobid|label|type|priority|unknown|status(?)|state(?)|idfk(?)|unknown|unknown
                            // 203 Command completed successfully. (Execution Time = 00:00:00.0000000)
                            halResponse = await SendHALCommandAsync("JOB schedule name: 'kiosk-configuration-job' priority: Highest label: ''");

                            responseLines = SplitByCRLF(halResponse);

                            string kioskConfigJobID = responseLines[0].Split('|')[0];

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received. With JobID " + kioskConfigJobID);
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // connect to the job we just made
                            halResponse = await SendHALCommandAsync("JOB connect job: '" + kioskConfigJobID + "'");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // check job status (why here though?)
                            halResponse = await SendHALCommandAsync("JOB scheduler-status'");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // start job
                            halResponse = await SendHALCommandAsync("JOB pend job: '" + kioskConfigJobID + "'");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received. FINAL LINE! YAY!");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            break;
                        case "test-comm":
                            SendHALCommandAsync("SERVICE test-comm");
                            return "200";
                        case "move-to-slot":
                            //JOB schedule name: 'tester-move-to-slot' priority: Highest label: ''
                            //STACK push value: 'slot' job: 'id'
                            //STACK push value: 'deck' job: 'id'
                            //JOB connect job: 'id'
                            //JOB scheduler-status
                            //JOB pend job: 'id'

                            string slot = arguments[2];
                            string deck = arguments[3];

                            Console.WriteLine("mts");

                            string halMoveToSlotResp = await SendHALCommandAsync("JOB schedule name: 'tester-move-to-slot' priority: Highest label: ''");

                            List<string> responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            string kioskConfigJobID2 = responseLinesMTS[0].Split('|')[0];

                            if (responseLinesMTS.Count > 1 && responseLinesMTS[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halMoveToSlotResp = await SendHALCommandAsync("STACK push value: '" + slot + "' job: '" + kioskConfigJobID2 + "'");

                            responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            if (responseLinesMTS.Count > 0 && responseLinesMTS[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halMoveToSlotResp = await SendHALCommandAsync("STACK push value: '" + deck + "' job: '" + kioskConfigJobID2 + "'");

                            responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            if (responseLinesMTS.Count > 0 && responseLinesMTS[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halMoveToSlotResp = await SendHALCommandAsync("JOB connect job: '" + kioskConfigJobID2 + "'");

                            responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            if (responseLinesMTS.Count > 0 && responseLinesMTS[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halMoveToSlotResp = await SendHALCommandAsync("JOB scheduler-status");

                            responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            if (responseLinesMTS.Count > 1 && responseLinesMTS[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            halMoveToSlotResp = await SendHALCommandAsync("JOB pend job: '" + kioskConfigJobID2 + "'");

                            responseLinesMTS = SplitByCRLF(halMoveToSlotResp);

                            if (responseLinesMTS.Count > 0 && responseLinesMTS[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            break;
                        case "exit-tester":
                            string halExitResp = await SendHALCommandAsync("JOB execute-immediate-base64 statement: 'IEFJUlhDSEdSIEZBTk9ODQogVkVORERPT1IgQ0xPU0UNCiBHUklQUEVSIFJFTlQNCiBHUklQUEVSIFJFVFJBQ1QNCiBTRU5TT1IgUElDS0VSLU9GRg0KIFJPTExFUiBTVE9QDQogUklOR0xJR0hUIE9GRg0KIENMRUFSDQo='");

                            List<string> responseLines232 = SplitByCRLF(halExitResp);

                            if (responseLines232.Count > 1 && responseLines232[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }
                            return "200";
                    }
                    break;
            }

            //Console.WriteLine(message);
            return "Completed";
        }
    }
}
