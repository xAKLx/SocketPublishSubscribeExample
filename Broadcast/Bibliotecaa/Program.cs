using SocketClientExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biblioteca
{
    class Program
    {
        static void Main(string[] args)
        {
            string msg = Console.ReadLine();
            AsynchronousClient client = new AsynchronousClient(Dns.GetHostName(), 11000, msg);
            new Thread(new ThreadStart(AsynchronousClient.StartClient)).Start();
            
            while (true)
            {
                msg = Console.ReadLine();
                AsynchronousClient.SendData(msg);
            }
        }
    }
}
