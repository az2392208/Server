using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public class ClientSocket
    {
        //客户端的类
        public Socket socket { get; set; }
        public long LastPingTime { get; set; } = 0;
        public ByteArray ReadBuff = new ByteArray();
    }
}
