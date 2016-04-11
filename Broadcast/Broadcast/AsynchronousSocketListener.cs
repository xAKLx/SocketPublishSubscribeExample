using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Broadcast
{

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        //private static HashSet<Socket> subscribers;

        private static Dictionary<Socket, String> subscribers;
        

        public AsynchronousSocketListener()
        {
            //subscribers = new HashSet<Socket>();
            subscribers = new Dictionary<Socket, string>();
        }


        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                    // Read data from the client socket. 
                    int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the 
                        // client. Display it on the console.
                        content = content.Remove(content.Length - 5);
                        
                        // Add client handler to set
                        if (handler != null && !subscribers.ContainsKey(handler))
                        {
                            subscribers.Add(handler, content);
                            Console.WriteLine("New Subscriber: " + content);
                        }
                        else
                        {
                            string value = "";
                            //Echo the data back to all subscribers.
                            foreach (var client in subscribers)
                                try
                                {
                                    value = "";
                                    subscribers.TryGetValue(handler, out value);
                                    if (client.Key != handler)
                                        Send(client.Key, value + ": " + content + "<EOF>");
                                }
                                catch (Exception) { }

                            Console.WriteLine("Read {0} bytes from socket. \n{1}", content.Length, value + ": " + content);
                            
                        }
                        state.sb = new StringBuilder();

                        // All data received. Get next message. 
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

                    }
                    else {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            
            }
            catch (Exception) { }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}