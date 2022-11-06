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

        public async Task<Dictionary<string, object>[]> GetDocumentField(string collection, string document, string field)
        {
            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);

            Dictionary<string, object>[] res = {};
            if (docRef != null)
            {
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
                docSnap.TryGetValue<Dictionary<string, object>[]>(field, out res);
            }
            
            return res;
        }

        public async Task WriteDocumentField(string collection, string document, string field, Dictionary<string, object>[] data)
        {
            Dictionary<string, object> fieldData = new Dictionary<string, object>
            {
                { field, data }
            };

            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);
            if (docRef != null)
                await docRef.SetAsync(fieldData);
        }
    }
}