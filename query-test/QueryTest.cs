using System;
using Xunit;
using Google.Cloud.Firestore;

namespace Migrator
{
    public class QueryTest
    {
        [Fact]
        public async Task ListSets()
        {
            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            // int cnt = 0;
            CollectionReference user = db.Collection("User_Inv");
            await foreach (DocumentReference doc in user.ListDocumentsAsync())
            {
                Console.WriteLine(doc.Id);
            }
        }

        [Theory]
        [InlineData("SNC")]
        [InlineData("VOW")]
        public async Task GetCardsForSet(string setCode)
        {
            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            CollectionReference user = db.Collection("User_Inv");
            DocumentReference setDoc = user.Document(setCode.ToUpper());
            DocumentSnapshot snap = await setDoc.GetSnapshotAsync();
            string setName = snap.GetValue<string>("Name");
            string dbCode = snap.GetValue<string>("Code");
            Console.WriteLine($"{setName} ({dbCode})");

            CollectionReference cards = setDoc.Collection("Cards");
            await foreach (DocumentReference card in cards.ListDocumentsAsync())
            {
                DocumentSnapshot cardSnap = await card.GetSnapshotAsync();
                string cardName = cardSnap.GetValue<string>("Name");
                string cardNum = cardSnap.GetValue<string>("CollectorNumber");
                Console.WriteLine($"{cardNum} -- {cardName}");
            }
        }

        protected void PrintSnapshotCards(QuerySnapshot snap)
        {
            foreach (DocumentSnapshot card in snap.Documents)
            {
                string cardName = card.GetValue<string>("Name");
                string cardNum = card.GetValue<string>("CollectorNumber");
                int count = card.GetValue<int>("Counts.Total");
                Console.WriteLine($"{cardNum} -- {cardName} = {count}");
            }
        }

        [Theory]
        [InlineData("VOW")]
        [InlineData("WAR")]
        public async Task GetCardCounts(string setCode)
        {
            Console.WriteLine($"Getting card counts for {setCode}");

            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            Query query = db.Collection($"User_Inv/{setCode}/Cards").OrderBy("SortNumber");
            QuerySnapshot snap = await query.GetSnapshotAsync();
            PrintSnapshotCards(snap);
        }

        [Theory]
        [InlineData("SNC")]
        public async Task GetCollectionCounts(string setCode)
        {
            Console.WriteLine($"Getting card counts for {setCode}");

            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            Query query = db.Collection($"User_Inv/{setCode}/Cards")
                            .WhereGreaterThan("Counts.Total", 0);
            QuerySnapshot snap = await query.GetSnapshotAsync();
            PrintSnapshotCards(snap);
        }

        [Theory]
        [InlineData("SNC")]
        public async Task GetFoils(string setCode)
        {
            Console.WriteLine($"Getting foil counts for {setCode}");

            FirestoreDb db = FirestoreDb.Create("testdb-8448b");

            Console.WriteLine("Getting just foil");
            Query query = db.Collection($"User_Inv/{setCode}/Cards")
                            .WhereGreaterThan("Counts.foil", 0);
            QuerySnapshot snap = await query.GetSnapshotAsync();
            PrintSnapshotCards(snap);

            Console.WriteLine("Getting foil|prerelease");
            query = db.Collection($"User_Inv/{setCode}/Cards")
                            .WhereGreaterThan("Counts.foil | prerelease", 0);
            snap = await query.GetSnapshotAsync();
            PrintSnapshotCards(snap);
        }
    }
}