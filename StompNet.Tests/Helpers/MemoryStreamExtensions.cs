using System.IO;
using System.Text;

namespace StompNet.Tests.Helpers
{
    internal static class MemoryStreamExtensions
    {
        public static void Write(this MemoryStream stream, string str)
        {
            foreach (byte b in Encoding.UTF8.GetBytes(str))
            {
                stream.WriteByte(b);
            }
        }
    }
}
