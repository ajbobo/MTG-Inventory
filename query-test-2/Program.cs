
using Google.Cloud.Firestore;
using Microsoft.Data.Sqlite;
using static Migrator2.SQLManager.InternalQuery;

namespace Migrator2
{
    public class QueryTester
    {
        public static void InitializeTables(SqliteConnection connection)
        {
            SQLManager sql = new(connection);
            sql.Query(CREATE_USER_INVENTORY).Go();
        }

        private static async Task PopulateTables(SqliteConnection connection)
        {
            Console.WriteLine("Reading inventory from Firebase");

            FirestoreDb db = FirestoreDb.Create("testdb-8448b");
            CollectionReference inventory = db.Collection("User_Inv_2");
            IAsyncEnumerable<DocumentReference> docs = inventory.ListDocumentsAsync();
            int numSets = await docs.CountAsync();
            int cnt = 1;
            await foreach (DocumentReference setDoc in docs)
            {
                Console.WriteLine($"Getting set {setDoc.Id} ({cnt}/{numSets})");

                DocumentSnapshot setSnap = await setDoc.GetSnapshotAsync();
                List<Inv_Card> cardList = setSnap.GetValue<List<Inv_Card>>("Cards");

                foreach (Inv_Card curCard in cardList)
                {
                    foreach (Inv_CardTypeCount ctc in curCard.Counts)
                    {
                        try
                        {
                            // Console.WriteLine($"{setSnap.Id} Card# {curCard.CollectorNumber} - Attrs: {ctc.Attrs}");

                            SQLManager sql = new SQLManager(connection);
                            int val = sql.Query(ADD_TO_USER_INVENTORY)
                                         .WithParam("@SetCode", setSnap.Id)
                                         .WithParam("@CollectorNumber", curCard.CollectorNumber)
                                         .WithParam("@Name", curCard.Name)
                                         .WithParam("@Attrs", ctc.Attrs)
                                         .WithParam("@Count", ctc.Count)
                                         .Go();
                            // Console.WriteLine($"Inserted {val} row(s)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception: {ex.Message}");
                        }
                    }
                }

                cnt++;
            }
        }

        private static void GetCardsFromSet(string setCode, SqliteConnection connection)
        {
            SQLManager sql = new(connection);
            using (SqliteDataReader? reader = sql.Query(GET_SET_UNIQUE_CARDS).WithParam("@SetCode", setCode.ToLower()).Read())
            {
                PrintReaderTable(reader);
            }
        }

        private static void GetPlaySetsFromSet(string setCode, SqliteConnection connection)
        {
            SQLManager sql = new(connection);
            using (SqliteDataReader? reader = sql.Query(GET_SET_PLAYSETS).WithParam("@SetCode", setCode.ToLower()).Read())
            {
                PrintReaderTable(reader);
            }
        }

        private static void PrintReaderTable(SqliteDataReader? reader)
        {
            if (reader == null)
                return;

            for (int col = 0; col < reader.FieldCount; col++)
            {
                Console.Write($"{reader.GetName(col)}  ");
            }
            Console.WriteLine();

            while (reader.Read())
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    Console.Write($"{reader.GetString(col)}  ");
                }
                Console.WriteLine();
            }
        }

        public static async Task Main()
        {
            using (var connection = new SqliteConnection("Data source=:memory:"))
            {
                connection.Open();

                InitializeTables(connection);
                await PopulateTables(connection);

                // GetCardsFromSet("dom", connection);
                GetPlaySetsFromSet("dom", connection);
                GetPlaySetsFromSet("snc", connection);
            }
        }
    }
}