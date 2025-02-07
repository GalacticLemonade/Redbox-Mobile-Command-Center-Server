﻿using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public class Program {
        static TCPClient client;

        public static HardwareService HardwareService;
        static ClientHelper ClientHelper;
        static MoveHelper MoveHelper;

        public static List<string> SplitByCRLF(string input) {
            // Split the string by \r\n
            return input.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        static void Main(string[] args) {
            TCPServer server = new TCPServer("0.0.0.0", 11600);
            server.Start();

            SetKioskIP();

            HardwareService = new HardwareService(IPCProtocol.Parse("rcp://127.0.0.1:7001"));
            ClientHelper = new ClientHelper(HardwareService);
            MoveHelper = new MoveHelper(HardwareService);
            ServiceLocator.Instance.AddService<IControlSystem>((object)new ClientControlSystem(HardwareService));

            if (!ClientHelper.TestCommunication()) {
                Console.WriteLine("Failed to communicate with HAL.");
                return;
            }



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
            string halIDResponse = await HALConnection.SendHALCommandAsync("SERVICE get-kiosk-id");

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
            string[] commandArguments = arguments.Skip(1).ToArray();

            if (CommandRegistry.Commands.TryGetValue(commandName, out ICommand command)) {
                return await command.Run(commandArguments);
            }
            else {
                return "Unknown command: " + commandName;
            }
        }
    }
}
