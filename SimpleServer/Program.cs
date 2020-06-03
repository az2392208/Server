using System;
using System.IO;
using System.Text;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] arg)
        {
            string path = @"C:\Users\Aive\Desktop\hellowworld.txt";
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (fs != null)
                    {
                        Console.WriteLine(fs.Length);
                        fs.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[fs.Length];

                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        byte[] headBuffer = Encoding.UTF8.GetBytes("AESEncrypt");
                        fs.Write(headBuffer, 0, 10);
                        fs.Write(buffer, 0, buffer.Length);

                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
