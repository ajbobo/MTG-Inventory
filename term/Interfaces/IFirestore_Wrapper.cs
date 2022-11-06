using Google.Cloud.Firestore;

namespace MTG_CLI
{
    public interface IFirestore_Wrapper
    {
        public void Connect(string dbName);
        public Task<Dictionary<string, object>[]> GetDocumentField(string collection, string document, string field);
        public Task WriteDocumentField(string collection, string document, string field, Dictionary<string, object>[] data);
    }
}