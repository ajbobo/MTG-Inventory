using System.Text;
using Newtonsoft.Json.Linq;

namespace ExtensionMethods
{
    public static class JsonExtensions
    {
        public static string AsString(this JToken? token)
        {
           return token != null ? token.ToString() : "";
        }

        public static int AsInt(this JToken? token)
        {
            if (token == null)
                return 0;
            _ = int.TryParse(token.ToString(), out int res);
            return res;
        }

        public static bool HasValue(this JToken? token)
        {
            return token != null && token.Type != JTokenType.Null;
        }

        public static string CompressArray(this JToken? token)
        {
            if (token?.Type != JTokenType.Array)
                return "";
            
            StringBuilder builder = new();
            foreach (JToken entry in token.Children())
            {
                builder.Append(entry.ToString());
            }
            return builder.ToString();
        }
    }
}