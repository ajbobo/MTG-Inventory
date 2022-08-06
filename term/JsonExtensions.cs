using System.Text;
using Newtonsoft.Json.Linq;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static string AsString(this JToken? token)
        {
           return (token != null ? token.ToString() : "");
        }

        public static int AsInt(this JToken? token)
        {
           return (token != null ? (int)token : 0);
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