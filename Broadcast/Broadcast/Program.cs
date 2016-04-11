using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broadcast
{
    class Program
    {
        static void Main(string[] args)
        {
            new AsynchronousSocketListener();
            AsynchronousSocketListener.StartListening();
        }
    }
}
