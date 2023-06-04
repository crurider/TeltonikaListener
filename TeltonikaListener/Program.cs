using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeltonikaListener {
    internal class Program {
        static async Task Main(string[] args) {
            Receiver receiver = new Receiver();
            List<Task<bool>> taskList = await receiver.StartReceiverAsync(6666);

            foreach (Task<bool> task in taskList) {
                bool result = await task;
                Console.WriteLine("Task completed with result: " + result);
            }
        }

    public class Receiver {
            public bool IsStopRequested { get; set; }
            private int _portToRun;

            public async Task<List<Task<bool>>> StartReceiverAsync(int port) {
                return await Task.Factory.StartNew(() => {
                    _portToRun = port;
                    List<Task<bool>> taskList = new List<Task<bool>>();

                    TcpListener listener = new TcpListener(IPAddress.Any, _portToRun);

                    listener.Start();
                    while (!IsStopRequested) {
                        Socket client = listener.AcceptSocket();
                        taskList.Add(ProcessAsync(client));
                    }
                    return taskList;
                });
            }

            internal async Task<bool> ProcessAsync(Socket clientSocket) {
                StreamWriter sr = new StreamWriter("D:\\Projects\\TeltonikaListener\\TeltonikaListener\\bin\\Debug\\AVL.txt");

                return await Task.Factory.StartNew(() => {
                    var ns = new NetworkStream(clientSocket);

                    byte[] buffer = new byte[1024];
                    int length;
                    while ((length = ns.Read(buffer, 0, buffer.Length)) > 0) {
                        string msgFromClient = Encoding.Default.GetString(buffer.Take(length).ToArray()).Trim();
                        sr.WriteLineAsync(msgFromClient);

                        byte[] msg = new byte[] { 0x01 };
                        // Send back a response.
                        ns.Write(msg, 0, msg.Length);
                    }
                    sr.Close();
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Disconnect(true);
                    return true;
                });
            }
        }
    }
}
