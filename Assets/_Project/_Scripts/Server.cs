using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
namespace Networking.Core {
    class Server : MonoBehaviour {
        [SerializeField] Transform player;
        static bool dirtyFlag;
        const string IP = "127.0.0.1";
        static Socket server = new(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        static byte[] buffer = new byte[1024], outBuffer = new byte[1024];
        static float[] positions;
        void Init() {
            Debug.Log("Spinning up server!");
            dirtyFlag = false;
            var ip = IPAddress.Parse(IP);
            server.Bind(new IPEndPoint(ip, 6969));
            server.Listen(1);
            Debug.Log("Listening...");
            server.BeginAccept(Accept, null);
        }

        void Accept(IAsyncResult arg) {
            var client = server.EndAccept(arg);
            Debug.Log("Client connected!");
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, client);
        }

        void Receive(IAsyncResult arg) {
            var socket = arg.AsyncState as Socket;
            var bytesReceived = socket.EndReceive(arg);
            positions = new float[bytesReceived / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, positions, 0, bytesReceived);
            Debug.Log($"RECEIVED: X: {positions[0]}, Y: {positions[1]}, Z: {positions[2]}");
            dirtyFlag = true;
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, socket);
            socket.BeginSend(outBuffer, 0, outBuffer.Length, SocketFlags.None, Send, socket);
        }

        void Send(IAsyncResult arg) {
            var socket = arg.AsyncState as Socket;
            Buffer.BlockCopy(positions, 0, outBuffer, 0, positions.Length * sizeof(float));
            socket.EndSend(arg);
            socket.BeginSend(outBuffer, 0, outBuffer.Length, SocketFlags.None, Send, socket);
        }

        void Update() {
            if (!dirtyFlag) return;
            dirtyFlag = false;
            player.position = new(positions[0], positions[1], positions[2]);
        }

        public void Start() {
            Init();
        }
    }

}
