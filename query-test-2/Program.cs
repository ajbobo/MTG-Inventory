
using Google.Cloud.Firestore;
using Microsoft.Data.Sqlite;

namespace Migrator2
{
    public class QueryTester
    {
        public static void InitializeTables(SqliteConnection connection)
        {
            SqliteCommand com = new() { Connection = connection };

            com.CommandText = "DROP TABLE IF EXISTS user_inventory";
            com.ExecuteNonQuery();

            com.CommandText =
            @"
                CREATE TABLE user_inventory (
                    SetCode         varchar(4),
                    CollectorNumber varchar(3),
                    Name            varchar(128),
                    Attrs           varchar(25),
                    Count           int
                );
            ";
            com.ExecuteNonQuery();
        }

        private static async Task PopulateTables(SqliteConnection connection)
        {
            SqliteCommand com = new() { Connection = connection };
            com.CommandText =
            @"
                INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count)
                VALUES (
                    @SetCode,
                    @CollectorNumber,
                    @Name,
                    @Attrs,
                    @Count
                );
            ";
            com.Parameters.AddWithValue("@SetCode", "");
            com.Parameters.AddWithValue("@CollectorNumber", "");
            com.Parameters.AddWithValue("@Name", "");
            com.Parameters.AddWithValue("@Attrs", "");
            com.Parameters.AddWithValue("@Count", "");

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

                            com.Parameters["@SetCode"].Value = setSnap.Id;
                            com.Parameters["@CollectorNumber"].Value = curCard.CollectorNumber;
                            com.Parameters["@Name"].Value = curCard.Name;
                            com.Parameters["@Attrs"].Value = ctc.Attrs;
                            com.Parameters["@Count"].Value = ctc.Count;
                            int val = com.ExecuteNonQuery();
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