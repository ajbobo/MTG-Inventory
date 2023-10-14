using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Firestore;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage] // I don't have a way to mock a FirestoreDb, so I can't unit test this one
    public class FirestoreDB_Wrapper : IFirestoreDB_Wrapper
    {
        private FirestoreDb? _db;

        public void Connect(string dbName)
        {
            _db = FirestoreDb.Create(dbName);
        }

        public async Task<XCardData[]> GetDocumentField(string collection, string document, string field)
        {
            CollectionReference? colRef = _db?.Collection(collection);
            DocumentReference? docRef = colRef?.Document(document);

            XCardData[] res = {};
            if (docRef != null)
            {
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
                docSnap.TryGetValue<XCardData[]>(field, out res);
            }
            
            return (res != null ? res : new XCardData[]{} );
        }

        public async Task WriteDocumentField(string collection, string document, string field, XCardData[] data)
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