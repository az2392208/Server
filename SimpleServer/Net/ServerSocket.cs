using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleServer.Net
{
    public class ServerSocket : Singleton<ServerSocket>
    {
        //公钥:服务器喝所用客户端共用的,由服务器下发
        public static string PublicKey = "OceanServer";
        //密钥:单独的加密,可随时间变化
        public static string SecretKey = "Oecan_Up&&NB!!Aive";
        //过程是 客户端第一次通过公钥连接服务器,服务器收到之后会将密钥发给客户端 然后以后的通讯都是通过密钥进行加密的
#if DEBUG //测试的时候使用debug宏定义
        //测试的时候应该使用本机地址
        private string m_IpStr = "127.0.0.1";
#else
        //对应的阿里云或者腾讯云对应的本机地址(不是公共IP地址)
        private string m_IpStr = "127.45.0.1";
#endif
        //端口号
        int m_port = 8099;
        //服务器的监听socket
        private static Socket m_ListenSocket;

        //客户端的socket集合(已经连接上的所有socket)
        private static List<Socket> m_CheckReadList = new List<Socket>();
        //初始化
        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint iPEndPoint = new IPEndPoint(ip, m_port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
