using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Redbox_Mobile_Command_Center_Server {
    public class TCPClient {
        private TcpClient _client;
        private NetworkStream _stream;

        public async Task ConnectAsync(string ipAddress, int port) {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            _stream = _client.GetStream();
            Console.WriteLine($"Connected to server at {ipAddress}:{port}");
        }

        public async Task SendMessageAsync(string message) {

            message = EncryptionHelper.Encrypt(message);

            if (_stream == null)
                throw new InvalidOperationException("Not connected to a server.");

            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
            //Console.WriteLine($"Sent: {message}");
        }

        public async Task<string> ReceiveMessageAsync() {
            if (_stream == null)
                throw new InvalidOperationException("Not connected to a server.");

            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead > 0) {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                message = EncryptionHelper.Decrypt(message);

                //Console.WriteLine($"Received: {message}");
                return message;
            }

            return string.Empty; // Return an empty string if no data is received.
        }

        public void Disconnect() {
            _stream?.Close();
            _client?.Close();
            Console.WriteLine("Disconnected from server.");
        }
    }
}
