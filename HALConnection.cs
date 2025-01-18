using System;
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
        private Thread _listenerThread;

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

                Console.WriteLine("Connected to HAL");

                //start a thread to listen for incoming messages
                _listenerThread = new Thread(ListenForMessages) {
                    IsBackground = true
                };
                _listenerThread.Start();
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
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex) {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
        }

        private void ListenForMessages() {
            try {
                while (_client != null && _client.Connected) {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0) {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        MessageReceived?.Invoke(message);
                    }
                    else {
                        //hal disconnected
                        break;
                    }
                }
            }
            catch (IOException) {
                //connection was closed
            }
            finally {
                Disconnect();
            }
        }

        public void Disconnect() {
            if (_listenerThread != null && _listenerThread.IsAlive) {
                _listenerThread.Join();
            }

            if (_stream != null) {
                _stream.Close();
                _stream = null;
            }

            if (_client != null) {
                _client.Close();
                _client = null;
            }

            Disconnected?.Invoke();
            Console.WriteLine("Disconnected from the server.");
        }
    }
}
