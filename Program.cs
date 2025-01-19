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

                //Console.WriteLine(halMessage);
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

                    //Console.WriteLine("Execute-Command");
                    //Console.WriteLine(command);

                    switch (command) {
                        case "hal-startup":
                            // Test Comm
                            string halResponse = await SendHALCommandAsync("SERVICE test-comm\r\n");

                            List<string> responseLines = SplitByCRLF(halResponse);
                            
                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Get Kiosk ID
                            halResponse = await SendHALCommandAsync("SERVICE get-kiosk-id\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Check init job status
                            halResponse = await SendHALCommandAsync("JOB init-status\r\n");

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
                            halResponse = await SendHALCommandAsync("JOB list\r\n");

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
                            halResponse = await SendHALCommandAsync("SERVICE diagnostic-mode status: true\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[0].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Compile lua script 1
                            halResponse = await SendHALCommandAsync("PROGRAM compile path: @'C:\\Program Files\\Redbox\\MS HAL Tester\\bin\\Scripts\\ms-pull-in-dvd.hs' name: 'ms-pull-in-dvd' requires-client-connection: False\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // Compile lua script 2
                            halResponse = await SendHALCommandAsync("PROGRAM compile path: @'C:\\Program Files\\Redbox\\MS HAL Tester\\bin\\Scripts\\ms-vend-disk-in-picker.hs' name: 'ms-vend-disk-in-picker' requires-client-connection: False\r\n");

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
                            halResponse = await SendHALCommandAsync("JOB schedule name: 'kiosk-configuration-job' priority: Highest label: ''\r\n");

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
                            halResponse = await SendHALCommandAsync("JOB connect job: '" + kioskConfigJobID + "'\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // check job status (why here though?)
                            halResponse = await SendHALCommandAsync("JOB scheduler-status'\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received.");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            // start job
                            halResponse = await SendHALCommandAsync("JOB pend job: '" + kioskConfigJobID + "'\r\n");

                            responseLines = SplitByCRLF(halResponse);

                            if (responseLines.Count > 1 && responseLines[1].StartsWith("203")) {
                                Console.WriteLine("Response code 203 received. FINAL LINE! YAY!");
                            }
                            else {
                                Console.WriteLine("Invalid response.");
                                return "402";
                            }

                            break;
                        case "exit-tester":
                            string halExitResp = await SendHALCommandAsync("JOB execute-immediate-base64 statement: 'IEFJUlhDSEdSIEZBTk9ODQogVkVORERPT1IgQ0xPU0UNCiBHUklQUEVSIFJFTlQNCiBHUklQUEVSIFJFVFJBQ1QNCiBTRU5TT1IgUElDS0VSLU9GRg0KIFJPTExFUiBTVE9QDQogUklOR0xJR0hUIE9GRg0KIENMRUFSDQo='\r\n");

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
