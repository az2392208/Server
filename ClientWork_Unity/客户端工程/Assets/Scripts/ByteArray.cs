using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ByteArray
{
    //默认数据长度
    public const int DEFFAULT_SIZE = 1024;
    //初始大小
    private int m_InitSize = 0;
    //缓冲区(真正用于读取的数组)
    public byte[] Bytes;
    //读取位置=开始读的索引
    public int ReadIdx = 0;
    //写入位置=写入完成后的索引
    public int WriteIndex = 0;
    //容量
    private int Capacity = 0;

    /// <summary>
    ///  剩余空间(容量减去写入位置)
    /// </summary>
    /// <param name=""></param>
    public int Remain { get { return Capacity - WriteIndex; } }

    /// <summary>
    /// 数据长度(写完后的位置-开始写的时候的位置=数据长度)
    /// </summary>
    public int Length { get { return WriteIndex - ReadIdx; } }

    public ByteArray()
    {
        Bytes = new byte[DEFFAULT_SIZE];
        Capacity = DEFFAULT_SIZE;
        m_InitSize = DEFFAULT_SIZE;
        ReadIdx = 0;
        WriteIndex = 0;
    }

    public ByteArray(byte[] dafalutBytes)
    {
        Bytes = dafalutBytes;
        Capacity = dafalutBytes.Length;
        m_InitSize = dafalutBytes.Length;
        ReadIdx = 0;
        WriteIndex = dafalutBytes.Length;
    }

    /// <summary>
    /// 检测并移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (Length < 8)
        {
            MoveBytes();
        }
    }

    public void MoveBytes()
    {
        if (ReadIdx < 0)
            return;
        //将数组从Readindex开始copy到该数组的0位置copy的长度为length
        Array.Copy(Bytes, ReadIdx, Bytes, 0, Length);
        WriteIndex = Length;
        ReadIdx = 0;
    }
    /// <summary>
    /// 设置数据的长度
    /// 用于数组长度不够存储数据时扩充数组的长度
    /// </summary>
    /// <param name="size">扩充后的长度</param>
    public void ReSize(int size)
    {
        if (ReadIdx < 0) return;
        if (size < Length) return;
        if (size < DEFFAULT_SIZE) return;
        int n = 1024;
        while (n < size)
        {
            n *= 2;
        }
        Capacity = n;
        byte[] newByte = new byte[Capacity];
        Array.Copy(Bytes, ReadIdx, newByte, 0, Length);
        WriteIndex = Length;
        ReadIdx = 0;
    }
}
