using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetManager : Singleton<NetManager>
{
    public enum NetEvent
    {
        ConnectSuss = 1,
        ConnectFail = 2,
        ConnectClose = 3
    }

    //密钥
    public string SecretKey { get; private set; }

    public string PublicKey = "OceanServer";
    //客户端socket
    private Socket m_Socket;
    //读取数组
    private ByteArray m_ReadBuff;
    //ip地址
    private string m_Ip;
    //端口号
    private int m_port;
    //连接中
    private bool m_Connecting = false;
    private bool m_Closing = false;

    private Thread m_MsgThread;
    private Thread m_HeartThread;
    //最后一次接收到信息的时间
    static long lastPongTime = 0;
    //最后一次的发送时间
    static long lastPingTime = 0;

    //发送消息处理队列
    private Queue<ByteArray> m_WriteQueue;

    //消息处理集合
    private List<MsgBase> m_MsgList;
    //Unity消息处理集合
    private List<MsgBase> m_UnityMsgList;
    //未处理的消息列表长度
    private int m_MsgCount = 0;

    #region 连接监听事件的委托
    public delegate void EventListener(string str);
    private Dictionary<NetEvent, EventListener> m_ListenerDic = new Dictionary<NetEvent, EventListener>();
    public void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent] += listener;
        }
        else
        {
            m_ListenerDic[netEvent] = listener;
        }
    }

    public void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent] -= listener;
            if (m_ListenerDic == null)
            {
                m_ListenerDic.Remove(netEvent);
            }
        }
    }
    private void FirstEvent(NetEvent netEvent, string str)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent](str);
        }
    }
    #endregion
    #region 协议监听事件的委托
    public delegate void ProtoListener(MsgBase msg);
    private Dictionary<ProtocolEnum, ProtoListener> m_ProtoDic = new Dictionary<ProtocolEnum, ProtoListener>();
    /// <summary>
    /// 一个协议希望只有一个监听
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="listener"></param>
    public void AddProtoListener(ProtocolEnum protocolEnum, ProtoListener listener)
    {
        m_ProtoDic[protocolEnum] = listener;
    }

    public void FirstProto(ProtocolEnum protocolEnum, MsgBase msgBase)
    {
        if (m_ProtoDic.ContainsKey(protocolEnum))
        {
            m_ProtoDic[protocolEnum](msgBase);
        }
    }
    #endregion

    public void Connect(string ip, int port)
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            Debug.LogError("连接失败.已经连接中了");
            return;
        }

        if (m_Connecting)
        {
            Debug.LogError("连接失败.正在连接中");
            return;
        }
        InitState();
        m_Socket.NoDelay = true;
        m_Connecting = true;
        m_Socket.BeginConnect(ip, port, ConnecrCallback, m_Socket);
        m_Ip = ip;
        m_port = port;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    private void InitState()
    {
        //初始化变量
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ReadBuff = new ByteArray();
        m_WriteQueue = new Queue<ByteArray>();
        m_Connecting = false;
        m_Closing = false;
        m_MsgList = new List<MsgBase>();
        m_UnityMsgList = new List<MsgBase>();
        m_MsgCount = 0;
        lastPongTime = GetTimeStamp();
        lastPingTime = GetTimeStamp();

    }

    private void ConnecrCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            m_Connecting = false;
            m_MsgThread = new Thread(MsgThread);
            m_MsgThread.IsBackground = true;
            m_MsgThread.Start();
            Debug.Log("Socket Connect Success");
            FirstEvent(NetEvent.ConnectSuss, "连接成功");
            m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, socket);
        }
        catch (Exception e)
        {
            Debug.LogError("Socket Connect fail" + e.Message);
            FirstEvent(NetEvent.ConnectFail, "连接失败");
            m_Connecting = false;
            return;
        }
    }

    public void MsgUpdate()
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgCount == 0)
            {
                return;
            }
            MsgBase msgBase = null;
            lock (m_UnityMsgList)
            {
                if (m_UnityMsgList.Count > 0)
                {
                    msgBase = m_UnityMsgList[0];
                    m_UnityMsgList.RemoveAt(0);
                    m_MsgCount--;
                }
            }
            if (msgBase != null)
            {
                FirstProto(msgBase.ProtoType, msgBase);
            }
        }
    }

    /// <summary>
    /// 处理接收到的信息
    /// 如果是后台的当场处理
    /// 如果是Unity的就发送给Unity来处理
    /// </summary>
    void MsgThread()
    {
        while (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgList.Count <= 0)
            {
                continue;
            }
            MsgBase msgBase = null;
            lock (m_MsgList)
            {
                if (m_MsgList.Count > 0)
                {
                    msgBase = m_MsgList[0];
                    m_MsgList.RemoveAt(0);
                }
            }

            if (msgBase != null)
            {
                //心跳包的处理
                if (msgBase is MsPing)
                {
                    lastPongTime = GetTimeStamp();
                    m_MsgCount--;
                }
                else
                {
                    lock (m_UnityMsgList)
                    {
                        m_UnityMsgList.Add(msgBase);
                    }
                }
            }
            else
            {
                break;
            }
        }
    }
    /// <summary>
    /// 接收数据回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count <= 0)
            {
                //关闭连接TODO
                Close();
                return;
            }
            m_ReadBuff.WriteIndex += count;
            OnReceiveData();
            if (m_ReadBuff.Remain < 8)
            {
                m_ReadBuff.MoveBytes();
                m_ReadBuff.ReSize(m_ReadBuff.Length * 2);
            }
            socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, socket);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Close();
            return;
        }
    }
    /// <summary>
    /// 对数据的处理
    /// </summary>
    private void OnReceiveData()
    {
        if (m_ReadBuff.Length <= 4 || m_ReadBuff.ReadIdx < 0)
        {
            return;
        }
        int readindex = m_ReadBuff.ReadIdx;
        byte[] bytes = m_ReadBuff.Bytes;
        int bodyLength = BitConverter.ToInt32(bytes, readindex);
        //读取协议之后进行判断,如果消息长度小于读出来的长度,证明没有一条完整的数据
        if (m_ReadBuff.Length < bodyLength + 4)
        {
            return;
        }
        m_ReadBuff.ReadIdx += 4;
        int nameCount = 0;
        ProtocolEnum protocol = MsgBase.DecodeName(m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, out nameCount);
        if (protocol == ProtocolEnum.None)
        {
            Debug.LogError("OnReceiveData MsgBase.Decodename fail");
            return;
        }
        m_ReadBuff.ReadIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        try
        {
            MsgBase msgBase = MsgBase.Decode(protocol, m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, bodyCount);
            if (msgBase == null)
            {
                Debug.LogError("接收数据协议内容解析出错");
                Close();
                return;
            }
            m_ReadBuff.ReadIdx += bodyCount;
            m_ReadBuff.CheckAndMoveBytes();
            //协议具体内容的操作
            lock (m_MsgList)
            {
                m_MsgList.Add(msgBase);
                m_MsgCount++;
            }
            //处理粘包
            if (m_ReadBuff.Length > 4)
            {
                OnReceiveData();
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("OnReceiveData MsgBase.DecodeName Fail" + ex.ToString());
            Close();
            return;
        }
    }
    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="msgBase"></param>
    public void SendMessage(MsgBase msgBase)
    {
        if (m_Socket == null || !m_Socket.Connected)
        {
            return;
        }
        if (m_Connecting)
        {
            Debug.LogError("正在连接服务器中,无法发送消息");
            return;
        }
        if (m_Closing)
        {
            Debug.LogError("正在断开连接中,无法发送消息");
            return;
        }

        try
        {
            byte[] nameBytes = MsgBase.EncodName(msgBase);
            byte[] bodyBytes = MsgBase.Encond(msgBase);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] headBytes = BitConverter.GetBytes(len);
            byte[] sendBytes = new byte[headBytes.Length + len];
            Array.Copy(headBytes, 0, sendBytes, 0, headBytes.Length);
            Array.Copy(nameBytes, 0, sendBytes, headBytes.Length, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, headBytes.Length + nameBytes.Length, bodyBytes.Length);
            ByteArray ba = new ByteArray(sendBytes);
            int count = 0;
            lock (m_WriteQueue)
            {
                m_WriteQueue.Enqueue(ba);
                count = m_WriteQueue.Count;
            }
            if (count == 1)
            {
                m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, m_Socket);
            }

        }
        catch (Exception e)
        {
            Debug.LogError("SendMessage error" + e.Message);
            Close();
            return;
        }
    }
    /// <summary>
    /// 发送消息的回调
    /// </summary>
    /// <param name="ar"></param>
    void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                return;
            }
            int count = socket.EndSend(ar);
            //判断是否发送完整
            ByteArray ba;
            lock (m_WriteQueue)
            {
                ba = m_WriteQueue.First();
            }
            ba.ReadIdx += count;
            //代表发送完整
            if (ba.Length == 0)
            {
                lock (m_WriteQueue)
                {
                    m_WriteQueue.Dequeue();
                    if (m_WriteQueue.Count > 0)
                    {
                        ba = m_WriteQueue.First();
                    }
                    else
                    {
                        ba = null;
                    }
                }
            }

            //发送不完整,或发送完整且存在第二条消息
            if (ba != null)
            {
                //TODO
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SendCallback fail" + e.Message);
            Close();
            return;
        }
    }

    public void Close(bool normal = true)
    {
        if (m_Socket == null || m_Connecting)
        {
            return;
        }

        SecretKey = "";
        m_Socket.Close();
        FirstEvent(NetEvent.ConnectClose, normal.ToString());
        Debug.Log("Close Socket");
    }
    public void SetKey(string key)
    {
        SecretKey = key;
    }
    /// <summary>
    /// 获取时间
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}
