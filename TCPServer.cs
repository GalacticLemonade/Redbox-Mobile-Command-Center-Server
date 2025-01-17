using Redbox_Mobile_Command_Center_Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCPServer {
    private TcpListener _listener;

    public TCPServer(string ipAddress, int port) {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start() {
        _listener.Start();
        Console.WriteLine($"Server started on {_listener.LocalEndpoint}. Waiting for clients...");
        AcceptClientsAsync();
    }

    public void Stop() {
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }

    private async void AcceptClientsAsync() {
        while (true) {
            try {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client); // Handle each client in a separate task
            }
            catch (Exception ex) {
                Console.WriteLine($"Error accepting client: {ex.Message}");
                break;
            }
        }
    }

    private async Task<string> HandleClientAsync(TcpClient client) {
        using (client) {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0) {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    message = EncryptionHelper.Decrypt(message);
                    Console.WriteLine($"Received from client: {message}");

                    String response = Program.OnServerIncomingData(message);

                    response = EncryptionHelper.Encrypt(response);
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);

                    return message; // Return the message received from the client
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally {
                Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
            }
        }

        return string.Empty; // Return an empty string if no message is received
    }
}
