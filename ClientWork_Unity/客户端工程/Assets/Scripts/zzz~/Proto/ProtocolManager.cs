using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有的协议收发的一个单独类
/// </summary>
public class ProtocolManager
{
    /// <summary>
    /// 连接服务器的第一个请求
    /// </summary>
    public static void SecretRequest()
    {
        MsgSecret msg = new MsgSecret();
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resmsg) =>
         {
             NetManager.Instance.SetKey(((MsgSecret)resmsg).Secret);
         });
    }
}
