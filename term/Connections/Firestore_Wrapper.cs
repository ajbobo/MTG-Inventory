using Google.Cloud.Firestore;

namespace MTG_CLI
{
    public class Firestore_Wrapper : IFirestore_Wrapper
    {
        private FirestoreDb? _db;

        public void Connect(string dbName)
        {
            _db = FirestoreDb.Create(dbName);
        }

        public async Task<DocumentSnapshot?> GetDocument(string collection, string document)
        {
            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);
            if (docRef != null)
                return await docRef.GetSnapshotAsync();
            else
                return null;
        }

        public async Task<bool> WriteDocument(string collection, string document, Dictionary<string, object> data)
        {
            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);
            if (docRef != null)
            {
                await docRef.SetAsync(data);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}