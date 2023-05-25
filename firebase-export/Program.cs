using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace MTG_Export
{
    public class Firebase_Export
    {
        public const string DB_NAME = "mtg-inventory-9d4ca";
        public const string COLLECTION_NAME = "User_Inv";
        public const string CARDS_FIELD = "Cards";

        public static async Task Main()
        {
            var results = new Dictionary<string, CardData[]>();

            var firestore = FirestoreDb.Create(DB_NAME);
            Console.WriteLine($"Connected to database {firestore.ProjectId}");

            var collection = firestore.Collection(COLLECTION_NAME);
            Console.WriteLine($"Connected to collection {collection.Id}");

            var documentList = collection.ListDocumentsAsync();
            // var enumerator = documentList.GetAsyncEnumerator();
            // await enumerator.MoveNextAsync();
            // var curDoc = enumerator.Current;
            await foreach (DocumentReference curDoc in documentList)
            {
                Console.WriteLine($"Found document: {curDoc.Id}");

                var snap = await curDoc.GetSnapshotAsync();
                if (snap.ContainsField(CARDS_FIELD))
                {
                    CardData[] cards = snap.GetValue<CardData[]>(CARDS_FIELD);
                    Console.WriteLine($"{snap.Id} has {cards.Length} cards");
                    results.Add(snap.Id, cards);
                }
            }

            using (StreamWriter writer = new StreamWriter("output.json"))
            {
                var options = new JsonSerializerSettings();
                options.Formatting = Formatting.Indented;

                var serializer = JsonSerializer.Create(options);
                serializer.Serialize(writer, results);
            }
        }
    }
}