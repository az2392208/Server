using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public partial class MsgHandler
    {
        /// <summary>
        /// 所有的协议处理函数都是这个标准,函数名=协议枚举名=类名
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgbase"></param>
        //密钥的获取
        public static void MsgSecret(ClientSocket c, MsgBase msgbase)
        {
            MsgSecret msgSecret = (MsgSecret)msgbase;
            msgSecret.Secret = ServerSocket.SecretKey;
            ServerSocket.OnSendData(c, msgbase);
        }
    }
}
