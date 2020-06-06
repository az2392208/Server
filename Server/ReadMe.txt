1.不懂的为地方:MsgBase中解码名字的部分是如何得出名字的长度的
            //算出名字的的长度
            Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
