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
        // 上一次连接时间(用于判断心跳包)
        public long LastPingTime { get; set; } = 0;
        //存储数据的类
        public ByteArray ReadBuff = new ByteArray();
    }
}
