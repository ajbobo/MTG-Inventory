using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Scryfall;

namespace MTG_Inventory
{
    class PopulateDB
    {
        static readonly HttpClient client = new HttpClient();

        static async Task<SetResponse> GetAllSets()
        {
            try
            {
                string data = await client.GetStringAsync("https://api.scryfall.com/sets");

                SetResponse response = JsonConvert.DeserializeObject<SetResponse>(data);

                Console.WriteLine("Got sets");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }

            return null;
        }


        static async Task WriteSet(FirestoreDb db, SetData set)
        {
            Console.Write("Writing set: {0}  ", set.Name);

            DocumentReference docRef = db.Collection("sets").Document(set.Code);
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                { "Name", set.Name },
                { "Code", set.Code.ToUpper() },
                { "Date", set.Released_At },
                { "Icon_Uri", set.Icon_Svg_Uri },
                { "Update_Time",  DateTime.UtcNow }
            };

            await docRef.SetAsync(entry);

            Console.WriteLine("Done");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Getting sets from Scryfall");

            SetResponse resp = await GetAllSets();

            System.Console.WriteLine("Connecting to database");
            FirestoreDb db = FirestoreDb.Create("mtg-inventory-9d4ca");

            Console.WriteLine("Writing sets to DB");
            foreach (SetData curSet in resp.data)
            {
                if (!curSet.Digital &&
                    (curSet.Set_Type == SetData.Type.EXPANSION || curSet.Set_Type == SetData.Type.CORE))
                {
                    WriteSet(db, curSet).Wait();
                }
            }

            System.Console.WriteLine("Done");
        }
    }
}
