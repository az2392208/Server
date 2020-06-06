using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string address = "127.0.0.1";
            int port = 8899;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(endPoint);
                Console.WriteLine("连接成功啦"+client.LocalEndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (true)
            {

            }

        }
    }
}
