using SimpleServer.Net;
using System;
using System.IO;
using System.Text;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] arg)
        {
            ServerSocket.Instance.Init();
        }
    }
}
