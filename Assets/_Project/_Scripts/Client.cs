using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
namespace Networking.Core {
    class Client : MonoBehaviour {
        const string IP = "127.0.0.1";
        static bool dirtyFlag;
        static Socket client = new(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        static byte[] buffer = new byte[1024], positionBytes;
        static float[] positions;
        [SerializeField] Transform player;
        [SerializeField] float moveSpeed = 5;

        void Init() {
            Debug.Log("Spinning up client!");
            var ip = IPAddress.Parse(IP);
            client.Connect(new IPEndPoint(ip, 6969));
            Debug.Log("Connected!");
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, client);
            positionBytes = Array.Empty<byte>();
            client.BeginSend(positionBytes, 0, positionBytes.Length, SocketFlags.None, Send, client);
            dirtyFlag = false;
        }

        void Receive(IAsyncResult arg) {
            var socket = arg.AsyncState as Socket;
            var bytesReceived = socket.EndReceive(arg);
            var tempPositions = new float[bytesReceived / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, tempPositions, 0, bytesReceived);
            Debug.Log($"RECEIVED: X: {tempPositions[0]}, Y: {tempPositions[1]}, Z: {tempPositions[2]}");
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, socket);
        }

        void Send(IAsyncResult arg) {
            var socket = arg.AsyncState as Socket;
            socket.EndSend(arg);
            if (dirtyFlag) {
                positions ??= new float[3];
                positionBytes = new byte[positions.Length * sizeof(float)];
                Buffer.BlockCopy(positions, 0, positionBytes, 0, positionBytes.Length);
                dirtyFlag = false;
            } else {
                positionBytes = Array.Empty<byte>();
            }
            Thread.Sleep(250);
            socket.BeginSend(positionBytes, 0, positionBytes.Length, SocketFlags.None, Send, socket);
        }

        public void Start() {
            Init();
        }

        void Update() {
            var x = Input.GetAxis("Horizontal");
            var z = Input.GetAxis("Vertical");

            if (x != 0 || z != 0) {
                dirtyFlag = true;
                player.position += new Vector3(x, 0, z) * (Time.deltaTime * moveSpeed);
                positions = new float[] { player.position.x, player.position.y, player.position.z };
            }
        }
    }
}
