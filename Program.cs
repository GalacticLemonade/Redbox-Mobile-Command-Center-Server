using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuicNet;
using QuicNet.Streams;
using QuicNet.Connections;

namespace Redbox_Mobile_Command_Center_Server {
    internal class Program {
        // fired when a client is connected
        static void ClientConnected(QuicConnection connection) {
            connection. += StreamOpened;
        }

        // fired when a new stream has been opened (It does not carry data with it)
        static void StreamOpened(QuicStream stream) {
            stream.On += StreamDataReceived;
        }

        // fired when a stream received full batch of data
        static void StreamDataReceived(QuicStream stream, byte[] data) {
            string decoded = Encoding.UTF8.GetString(data);

            // Send back data to the client on the same stream
            stream.Send(Encoding.UTF8.GetBytes("Ping back from server."));
        }

        static void Main(string[] args) {
            QuicListener listener = new QuicListener(11000);
            listener.OnClientConnected += ClientConnected;
        }
    }
}
