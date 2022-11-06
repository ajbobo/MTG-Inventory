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

        public async Task<CardData[]> GetDocumentField(string collection, string document, string field)
        {
            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);

            CardData[] res = {};
            if (docRef != null)
            {
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
                docSnap.TryGetValue<CardData[]>(field, out res);
            }
            
            return res;
        }

        public async Task WriteDocumentField(string collection, string document, string field, CardData[] data)
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