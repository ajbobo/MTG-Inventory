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
                curCard.UUID = doc.Id;
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

        public void AddCard(Scryfall.Card curCard, CardTypeCount ctc)
        {
            string setCode = curCard.SetCode;

            if (!_inventory.ContainsKey(setCode))
                _inventory.Add(setCode, new());
            else if (_inventory[setCode].ContainsKey(curCard.CollectorNumber)) // The card is in inventory already - no need to add it
                return;

            MTG_Card newCard = new()
            {
                Name = curCard.Name,
                CollectorNumber = curCard.CollectorNumber,
                SetCode = setCode,
                Set = curCard.SetName
            };
            newCard.Counts.Add(ctc);

            _inventory[setCode].Add(curCard.CollectorNumber, newCard);
        }

        public MTG_Card? GetCard(Scryfall.Card card)
        {
            string setCode = card.SetCode;
            string collectorNumber = card.CollectorNumber;

            if (!_inventory.ContainsKey(setCode) || !_inventory[setCode].ContainsKey(collectorNumber))
                return null;

            return _inventory[setCode][collectorNumber];
        }

        public string GetCardCountDisplay(Scryfall.Card card)
        {
            MTG_Card? curCard = GetCard(card);
            return GetCardCountDisplay(curCard);
        }

        public string GetCardCountDisplay(MTG_Card? curCard)
        {
            return String.Format("{0}{1}{2}", curCard?.GetTotalCount() ?? 0, (curCard?.HasAttr("foil") ?? false ? "✶" : ""), (curCard?.HasOtherAttr("foil") ?? false ? "Ω" : ""));
        }

        async public Task WriteToFirebase(MTG_Card card)
        {
            CollectionReference collection = _db.Collection("user_inventory");
            if (card.UUID.Length > 0)
            {
                await collection.Document(card.UUID).SetAsync(card);
            }
            else
            {
                DocumentReference doc = await collection.AddAsync(card);
                card.UUID = doc.Id;
            }
        }
    }
}