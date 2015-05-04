using System.Text;
using Newtonsoft.Json;

namespace RabbitDemo.Utilities
{

    public static class GetBytesExtensions
    {
        public static byte[] GetBytes(this string stringValue, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bytes = encoding.GetBytes(stringValue);
            return bytes;
        }

        public static string GetString(this byte[] body, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var value = encoding.GetString(body);
            
            return value;
        }

    }
}