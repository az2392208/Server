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
        //发送时间
        public static long m_pingInterval = 30;
        //服务器的监听socket
        private static Socket m_ListenSocket;

        //临时保存所有socket集合
        private static List<Socket> m_CheckReadList = new List<Socket>();
        //定义一个字典存放所有的socket
        public static Dictionary<Socket, ClientSocket> m_clientDic = new Dictionary<Socket, ClientSocket>();
        //一个临时的变量存放需要释放的Socket
        public static List<ClientSocket> m_TempList = new List<ClientSocket>();
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint iPEndPoint = new IPEndPoint(ip, m_port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(iPEndPoint);
            m_ListenSocket.Listen(10);

            Debug.LogInfo("服务器启动监听成功", m_ListenSocket.LocalEndPoint.ToString());

            while (true)
            {
                //检查是否有读取的Socket

                //加载所有的Socket
                ResetCheckRead();
                try
                {
                    //检测数哪些是可用的Socket
                    Socket.Select(m_CheckReadList, null, null, 1000);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                for (int i = m_CheckReadList.Count - 1; i >= 0; i--)
                {
                    Socket s = m_CheckReadList[i];
                    if (s == m_ListenSocket)
                    {
                        //说明有客户端连接到服务器了,所以服务器可读
                        ReadListen(s);
                    }
                    else
                    {
                        //说明连接的客户端可读,有消息传上来了
                        ReadClient(s);
                    }
                }
                //检测心跳包超时的计算
                m_TempList.Clear();
                long timeNoe = GetTimeStamp();
                foreach (ClientSocket clientSocket in m_clientDic.Values)
                {
                    //当前时间减去上一次心跳包的时间大于四倍心跳包连接的时间
                    if (timeNoe - clientSocket.LastPingTime > m_pingInterval * 4)
                    {
                        Debug.Log("Ping Close" + clientSocket.socket.RemoteEndPoint.ToString());
                        m_TempList.Add(clientSocket);
                    }
                }
                foreach (ClientSocket clientSocket in m_TempList)
                {
                    CloseClient(clientSocket);
                }
            }
        }



        /// <summary>
        /// 读取所有的socket
        /// 将他们从Socket字典中提取到Socket集合中来
        /// </summary>
        public void ResetCheckRead()
        {
            m_CheckReadList.Clear();
            m_CheckReadList.Add(m_ListenSocket);
            foreach (Socket s in m_clientDic.Keys)
            {
                m_CheckReadList.Add(s);
            }
        }
        /// <summary>
        /// 与客户端进行消息的通讯
        /// </summary>
        /// <param name="client"></param>
        void ReadClient(Socket client)
        {
            ClientSocket clientSocket = m_clientDic[client];
            //接收信息根据信息解析协议,更具协议内容
            ByteArray readBuff = clientSocket.ReadBuff;
            int count = 0;
            //如果上一次刚好填充满整个长度
            if (readBuff.Remain <= 0)
            {
                //移动到index=0的位置
                OnReceiveData(clientSocket);
                readBuff.CheckAndMoveBytes();
                //如果数据长度大与默认长度,保证信息正常接受
                while (readBuff.Remain <= 0)
                {
                    //扩充数组长度
                    int expandsize = readBuff.Length < ByteArray.DEFFAULT_SIZE ? ByteArray.DEFFAULT_SIZE : readBuff.Length;
                    readBuff.ReSize(expandsize);
                }
            }
            try
            {
                count = client.Receive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0);
            }
            catch (SocketException e)
            {
                Debug.LogError("Receive Error:+" + e);
                CloseClient(clientSocket);
                return;
            }
            if (count <= 0)
            {
                CloseClient(clientSocket);
                return;
            }
            readBuff.WriteIndex += count;
            //解析信息
            OnReceiveData(clientSocket);
            readBuff.CheckAndMoveBytes();
        }

        /// <summary>
        /// 解析接收到的数据
        /// </summary>
        /// <param name="clientSocket"></param>
        private void OnReceiveData(ClientSocket clientSocket)
        {
            //如果信息长度不够,我们要再次读取信息
        }
        /// <summary>
        /// 关闭同客户端之间的通讯
        /// </summary>
        /// <param name="client"></param>
        public void CloseClient(ClientSocket client)
        {
            client.socket.Close();
            m_clientDic.Remove(client.socket);
        }
        /// <summary>
        /// 这个是等待服务器进行连接的监听端口
        /// </summary>
        /// <param name="listen"></param>
        private void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.socket = client;
                //获取当前的时间戳
                clientSocket.LastPingTime = GetTimeStamp();
                m_clientDic.Add(client, clientSocket);
                Debug.Log($"一个客户端连接:{client.LocalEndPoint.ToString()},当前{m_clientDic.Count}客户端在线");
                Debug.Log("一个客户端连接");
            }
            catch (SocketException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts);
        }
    }
}
