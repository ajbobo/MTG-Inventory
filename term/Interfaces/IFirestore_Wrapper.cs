using Google.Cloud.Firestore;

namespace MTG_CLI
{
    public interface IFirestore_Wrapper
    {
        public void Connect(string dbName);
        public Task<DocumentSnapshot?> GetDocument(string collection, string document);
        public Task<bool> WriteDocument(string collection, string document, Dictionary<string, object> data);
    }
}