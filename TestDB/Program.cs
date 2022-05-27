﻿using System;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace TestDB
{
    class Tester
    {
        private static async Task MigrateData(FirestoreDb db)
        {
            // Get data
            Console.WriteLine("Local Json data");
            const string CACHE_FILE = "C:\\Dev\\Misc\\MTG-Inventory\\TestDB\\inventory_cache.json";

            Dictionary<string, Dictionary<string, MTG_Card_Legacy>> oldData;
            using (StreamReader reader = new StreamReader(CACHE_FILE))
            {
                string json = reader.ReadToEnd();
                oldData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MTG_Card_Legacy>>>(json) ?? new();
            }

            Console.WriteLine("Read the old data - converting to new format");
            UserInventory inventory = new();
            foreach (string setCode in oldData.Keys)
            {
                Dictionary<string, MTG_Card_Legacy> oldSet = oldData[setCode];

                MTG_Set curSet = new();
                curSet.Code = setCode;

                foreach (string cardNum in oldSet.Keys)
                {
                    MTG_Card_Legacy oldCard = oldSet[cardNum];

                    MTG_Card curCard = new(oldCard);
                    if (curSet.Code.Equals(oldCard.SetCode))
                    {
                        curSet.Name = oldCard.Set;
                    }
                    else
                    {
                        Console.WriteLine("Mismatched SetCode on a card: " + oldCard);
                    }

                    curSet.Cards.Add(curCard);
                }

                inventory.Sets.Add(curSet);
            }

            // Console.WriteLine(inventory);

            // Approach 1 - A single Document with everything -- This reads and writes quickly, with minimal quota usage, but I think it may be hard to query directly. 
            //              I'd have to do all filtering locally. That might be better since I have to filter based on 2 data sources (Firebase + Scryfall)
            // DocumentReference userDoc = db.Collection("User").Document("Inventory");
            // await userDoc.SetAsync(inventory);

            // Approach 2 - User_Inventory(Collection) -> User(Document) -> Set(Collection) -> Card(Document)
            //              Writing all this data the first time is slow, uses lots of quota (1 per document/card)
            //              This might be easier to query, though. Filters could be done through queries instead of custom code. Can't filter Scryfall data this way, though.
            // DocumentReference userDoc = db.Collection("User_Inventory").Document("User");
            // foreach (MTG_Set curSet in inventory.Sets)
            // {
            //     foreach (MTG_Card curCard in curSet.Cards)
            //     {
            //         DocumentReference cardDoc = userDoc.Collection(curSet.Code.ToUpper()).Document(curCard.CollectorNumber);
            //         await cardDoc.SetAsync(curCard);
            //     }
            // }

            // Approach 3 - User(Collection) -> Set(Document) -> [Name, Code](Fields), Cards(Collection) -> Card(Document) -> [Name, CollectorNumber](Fields), Counts(Collection) -> CTC(Document)
            //              Better than approach 2, I think. More filterable on CTC values. 
            //              To be fully filterable, I'd need to have all Scryfall data in Firestore, too, so that I can search for cards that aren't in my inventory
            int cnt = 0;
            CollectionReference user = db.Collection("UserId_X");
            foreach (MTG_Set curSet in inventory.Sets)
            {
                DocumentReference set = user.Document(curSet.Code.ToUpper());
                await set.SetAsync(curSet);

                CollectionReference cardList = set.Collection("Cards");
                foreach (MTG_Card curCard in curSet.Cards)
                {
                    cnt++;
                    if (cnt >= 500)
                        continue;

                    if (cnt % 50 == 0)
                        Console.WriteLine("{0} written", cnt);

                    await cardList.Document(curCard.CollectorNumber).SetAsync(curCard);

                    foreach (CardTypeCount ctc in curCard.Counts)
                        await cardList.Document(curCard.CollectorNumber).Collection("Counts").Document().SetAsync(ctc);
                }
            }

        }

        private static async Task GetSets(FirestoreDb db)
        {
            Query query = db.Collection("UserId_X")
                .OrderBy("Code");
            QuerySnapshot snap = await query.GetSnapshotAsync();
            foreach (DocumentSnapshot curSet in snap.Documents)
            {
                MTG_Set set = curSet.ConvertTo<MTG_Set>();
                Console.WriteLine(set);

                Query q2 = curSet.Reference.Collection("Cards");
                QuerySnapshot s2 = await q2.GetSnapshotAsync();

                foreach (DocumentSnapshot doc in s2.Documents)
                {
                    set.Cards.Add(doc.ConvertTo<MTG_Card>());
                }
                Console.WriteLine(set.Cards.Count());
            }
        }

        private static async Task GetSetCards(FirestoreDb db, string setCode)
        {
            Console.WriteLine("Cards in {0}", setCode);

            Query query = db.Collection("UserId_X").Document(setCode.ToUpper()).Collection("Cards");
            QuerySnapshot snap = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot curCard in snap.Documents)
            {
                MTG_Card card = curCard.ConvertTo<MTG_Card>();

                CollectionReference countsRef = curCard.Reference.Collection("Counts");
                QuerySnapshot ctcQ = await countsRef.GetSnapshotAsync();
                foreach (DocumentSnapshot ctcSnap in ctcQ)
                {
                    card.Counts.Add(ctcSnap.ConvertTo<CardTypeCount>());
                }

                Console.WriteLine(card);
            }
        }

        private static async Task GetFoils(FirestoreDb db, string setCode)
        {
            Console.WriteLine("Foils in {0}", setCode);

            DocumentSnapshot setSnap = await db.Collection("UserId_X").Document(setCode.ToUpper()).GetSnapshotAsync();

            // How do I search only collections inside the Set's Document?
            Query query = db.CollectionGroup("Counts")
                .WhereArrayContains("Attrs", "foil");
                
            QuerySnapshot snap = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot ctcSnap in snap.Documents)
            {
                DocumentSnapshot cardSnap = await ctcSnap.Reference.Parent.Parent.GetSnapshotAsync();
                                
                MTG_Card curCard = cardSnap.ConvertTo<MTG_Card>();
                curCard.Counts.Add(ctcSnap.ConvertTo<CardTypeCount>());

                Console.WriteLine(curCard);
            }
        }

        public static async Task Main()
        {

            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            // await MigrateData(db);

            // await GetSets(db);

            await GetSetCards(db, "vow");

            await GetFoils(db, "vow");

        }
    }
}