using System.Text;
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

        public MTG_Card? GetCard(string setCode, string collectorNumber)
        {
            if (!_inventory.ContainsKey(setCode) || !_inventory[setCode].ContainsKey(collectorNumber))
                return null;

            return _inventory[setCode][collectorNumber];
        }

        public int GetTotalCardCount(string setCode, string collectorNumber)
        {
            MTG_Card? curCard = GetCard(setCode, collectorNumber);
            return curCard?.GetTotalCount() ?? 0;
        }

        public string GetCardCountDisplay(string setCode, string collectorNumber)
        {
            MTG_Card? curCard = GetCard(setCode, collectorNumber);
            return GetCardCountDisplay(curCard);
        }

        public string GetCardCountDisplay(MTG_Card? curCard)
        {
            return String.Format("{0}{1}{2}", curCard?.GetTotalCount() ?? 0, (curCard?.HasAttr("foil") ?? false ? "✶" : ""), (curCard?.HasOtherAttr("foil") ?? false ? "Ω" : ""));
        }
    }
}