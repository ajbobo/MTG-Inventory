
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
            FirestoreDb db = FirestoreDb.Create("testdb-8448b");
            CollectionReference inventory = db.Collection("User_Inv_2");
            IAsyncEnumerable<DocumentReference> docs = inventory.ListDocumentsAsync();
            await foreach (DocumentReference setDoc in docs)
            {
                DocumentSnapshot setSnap = await setDoc.GetSnapshotAsync();
                List<Inv_Card> cardList = setSnap.GetValue<List<Inv_Card>>("Cards");

                foreach (Inv_Card curCard in cardList)
                {
                    foreach (Inv_CardTypeCount ctc in curCard.Counts)
                    {
                        try
                        {
                            Console.WriteLine($"{setSnap.Id} Card# {curCard.CollectorNumber} - Attrs: {ctc.Attrs}");

                            SQLManager sql = new SQLManager(connection);
                            int val = sql.Query(ADD_TO_USER_INVENTORY)
                                         .WithParam("@SetCode", setSnap.Id)
                                         .WithParam("@CollectorNumber", curCard.CollectorNumber)
                                         .WithParam("@Name", curCard.Name)
                                         .WithParam("@Attrs", ctc.Attrs)
                                         .WithParam("@Count", ctc.Count)
                                         .Go();
                            Console.WriteLine($"Inserted {val} row(s)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception: {ex.Message}");
                        }
                    }
                }
            }

        }

        public static async Task Main()
        {
            using (var connection = new SqliteConnection("Data source=temp.db"))
            {
                connection.Open();

                InitializeTables(connection);
                await PopulateTables(connection);
            }
        }
    }
}