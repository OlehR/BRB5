using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BRB5.Model
{
    public class NativeBase
    {
        public virtual byte[] ResizeImage(byte[] imageData, float max, int compress = 90) { throw new NotImplementedException(); }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
