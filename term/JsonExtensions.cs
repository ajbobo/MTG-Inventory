using Newtonsoft.Json.Linq;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static string AsString(this JToken? token)
        {
           return (token != null ? token.ToString() : "");
        }
    }
}