using Google.Cloud.Firestore;

namespace MTG_CLI
{
    [FirestoreData(ConverterType = typeof(CardDataConverter))]
    public class XCardData
    {
        private Dictionary<string, object> _data { set; get; } = new();

        public object this[string index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public void Add(string index, object data)
        {
            _data[index] = data;
        }

        public string[] Keys { get { return _data.Keys.ToArray<string>(); } }
    }

    public class CardDataConverter : IFirestoreConverter<XCardData>
    {
        public object ToFirestore(XCardData value)
        {
            Dictionary<string, object> res = new();
            foreach (string key in value.Keys)
                res[key] = value[key];

            return res;
        }

        public XCardData FromFirestore(object value)
        {
            XCardData res = new();

            Dictionary<string, object> dict = (Dictionary<string, object>)value;
            foreach (string key in dict.Keys)
                res[key] = dict[key];

            return res;
        }
    }
}