using ProtoBuf;
using System;
using System.IO;
using UnityEngine;
    /// <summary>
    /// 最基础的协议类
    /// 对信息进行系列化和反序列化
    /// 每一个协议就是一个类
    /// 这个是所有协议的基类
    /// 协议分为三部分
    /// 1.协议头(该协议的内容+名称的总长度)
    /// 2.协议名称
    /// 3.协议内容
    /// </summary>
    public class MsgBase
    {
        //协议类型
        public virtual ProtocolEnum ProtoType { get; set; }

        /// <summary>
        /// 编码协议名
        /// </summary>
        /// <param name="msgBase"></param>
        /// <returns></returns>
        public static byte[] EncodName(MsgBase msgBase)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
            Int16 len = (Int16)nameBytes.Length;
            byte[] bytes = new byte[2 + len];
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }
        /// <summary>
        /// 解码协议名
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            if (offset + 2 > bytes.Length) return ProtocolEnum.None;
            //算出名字的的长度
            Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
            if (offset + 2 + len > bytes.Length) return ProtocolEnum.None;
            count = 2 + len;
            try
            {
                string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
                return (ProtocolEnum)System.Enum.Parse(typeof(ProtocolEnum), name);
            }
            catch (Exception e)
            {
                Debug.LogError("不存在的协议" + e.Message);
                return ProtocolEnum.None;
            }
        }
        /// <summary>
        /// 协议的系列化和加密
        /// </summary>
        /// <param name="mesBase"></param>
        /// <returns></returns>
        public static byte[] Encond(MsgBase mesBase)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                //将我们的协议类进行序列化转化为byte数组
                Serializer.Serialize(memory, mesBase);
                byte[] bytes = memory.ToArray();
                //string sercet = ServerSocket.SecretKey;
                ////对数组进行加密
                //if (mesBase is MsgSecret)
                //{
                //    sercet = ServerSocket.PublicKey;
                //}
                //bytes = AES.AESEncrypt(bytes, sercet);
                return bytes;
            }
        }
        /// <summary>
        /// 协议的解密
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int offset, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("协议解密出错,长度为0");
                return null;
            }
            try
            {
                byte[] newBytes = new byte[count];
                Array.Copy(bytes, offset, newBytes, 0, count);
                //string secret = ServerSocket.SecretKey;
                //if (protocol == ProtocolEnum.MsgSecret)
                //{
                //    secret = ServerSocket.PublicKey;
                //}
                //newBytes = AES.AESDecrypt(newBytes, secret);
                using (MemoryStream memory = new MemoryStream(newBytes, 0, count))
                {
                    //根据枚举的名称转化为协议类,所以我们的枚举名称要保持一致
                    Type t = System.Type.GetType(protocol.ToString());
                    //反序列化
                    MsgBase msg = (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
                    return msg;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("协议解密出错" + e.Message);
                return null;
            }
        }
    }

