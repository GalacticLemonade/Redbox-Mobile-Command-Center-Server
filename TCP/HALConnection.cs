﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public class HALConnection {
        private readonly string _server;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private Task _listenerTask;

        public event Action<string> MessageReceived;
        public event Action Disconnected;

        public HALConnection(string server, int port) {
            _server = server;
            _port = port;
        }

        public void Connect() {
            try {
                _client = new TcpClient();
                _client.Connect(_server, _port);
                _stream = _client.GetStream();

                //Console.WriteLine("Connected to HAL.");

                // Start a task to listen for incoming messages
                _listenerTask = Task.Run(() => ListenForMessages());
            }
            catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void SendMessage(string message) {
            if (_stream == null || !_stream.CanWrite) {
                Console.WriteLine("Cannot send message, stream is unavailable.");
                return;
            }

            try {
                byte[] data = Encoding.UTF8.GetBytes(message);
                //Console.WriteLine(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex) {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
        }

        private void ListenForMessages() {
            try {
                byte[] buffer = new byte[1024];
                while (_client != null && _client.Connected) {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0) {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        MessageReceived?.Invoke(message);
                    }
                    else {
                        // HAL disconnected or closed the connection
                        break;
                    }
                }
            }
            catch (IOException) {
                // Connection was closed
            }
            finally {
                Disconnect(false);
            }
        }

        public void Disconnect(bool sendQuit = true) {
            if (sendQuit && _stream != null && _stream.CanWrite) {
                SendMessage("quit");
            }

            Disconnected?.Invoke();
            //Console.WriteLine("Disconnected from HAL.");
        }

        public static async Task<string> SendHALCommandAsync(string command) {
            command = command + "\r\n";

            // Create the connection instance
            HALConnection halConnection = new HALConnection("127.0.0.1", 7001);

            string commandResponse = null;
            int messageCount = 0;

            var tcs = new TaskCompletionSource<string>();

            // Subscribe to events
            halConnection.MessageReceived += (halMessage) => {
                //Console.WriteLine(halMessage);
                messageCount++;

                if (messageCount == 2) {
                    Console.WriteLine(halMessage);
                    Console.WriteLine(messageCount);
                    // Capture the second message (command response)
                    commandResponse = halMessage;
                    tcs.SetResult(commandResponse); // Set the result once we have the second response
                }
            };

            halConnection.Disconnected += () => {
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
                Console.WriteLine(await tcs.Task);
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
    }
}
