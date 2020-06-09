using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
    #region 监听事件的委托
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
    void FirstEvent(NetEvent netEvent, string str)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent](str);
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
    void InitState()
    {
        //初始化变量
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ReadBuff = new ByteArray();
        m_Connecting = false;
        m_Closing = false;
    }

    void ConnecrCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            m_Connecting = false;
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

    void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {

        }
        catch (Exception e)
        {
        }
    }
}
