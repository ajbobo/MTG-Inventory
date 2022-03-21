using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace MTG_CLI
{
    public class Inventory
    {
        private FirestoreDb _db;

        // Inventory structure: _inventory[SetCode][CardNum] = Card
        private Dictionary<string, Dictionary<string, MTG_Card>> _inventory = new();

        public Inventory()
        {
            _db = FirestoreDb.Create("mtg-inventory-9d4ca");
        }

        public async Task ReadFromFirebase()
        {
            Console.WriteLine("Firebase data");
            Query userInventoryQuery = _db.Collection("user_inventory");
            QuerySnapshot snapshot = await userInventoryQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in snapshot)
            {
                MTG_Card curCard = doc.ConvertTo<MTG_Card>();
                AddCardToInventory(curCard);
            }
        }

        public void ReadFromJson()
        {
            Console.WriteLine("Local Json data");
            using (StreamReader reader = new StreamReader("C:\\Dev\\Misc\\MTG-Inventory\\web-ui\\src\\data\\hard_coded.js"))
            {
                string json = reader.ReadToEnd();
                List<MTG_Card> theList = JsonConvert.DeserializeObject<List<MTG_Card>>(json) ?? new();
                foreach (MTG_Card curCard in theList)
                    AddCardToInventory(curCard);
            }
        }

        private void AddCardToInventory(MTG_Card curCard)
        {
            // Console.WriteLine($"{curCard.SetCode} {curCard.CollectorNumber} - {curCard.Name}");

            if (!_inventory.ContainsKey(curCard.SetCode))
                _inventory.Add(curCard.SetCode, new());

            _inventory[curCard.SetCode].Add(curCard.CollectorNumber, curCard);
        }

        public int GetTotalCardCount(string setCode, string collectorNumber)
        {
            if (!_inventory.ContainsKey(setCode) || !_inventory[setCode].ContainsKey(collectorNumber))
                return 0;

            MTG_Card curCard = _inventory[setCode][collectorNumber];
            // Calcuate the total from CTCs - FINISH ME
            return 1;
        }
    }
}