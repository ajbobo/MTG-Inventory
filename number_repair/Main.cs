using Google.Cloud.Firestore;

namespace Number_Repair
{
    class RepairObject
    {
        public string Name { get; set; } = "";
        public string DeckBoxNumber { get; set; } = "";
        public string CollectorNumber { get; set; } = "";

        public override string ToString()
        {
            return $"{Name} : {DeckBoxNumber} -> {CollectorNumber}";
        }
    }

    class Number_Repair
    {
        private static Dictionary<string, RepairObject> _repairList = new();
        private static Dictionary<string, object>[] _setData = new Dictionary<string, object>[] { };
        private static Dictionary<string, object>[] _newData = new Dictionary<string, object>[] { };
        private static FirestoreDb _db = FirestoreDb.Create("mtg-inventory-9d4ca");

        private static void ImportFileData(string setCode)
        {
            string filename = $"Magic Cards - {setCode}.csv";
            Console.WriteLine($"Getting CSV file: {filename}");

            using (var reader = new StreamReader($"Sets/{filename}"))
            {
                int rowNum = 1;
                while (!(reader?.EndOfStream ?? true))
                {
                    string line = reader?.ReadLine() ?? "";
                    if (rowNum >= 3)
                    {
                        string[] arr = line?.Split(",") ?? new string[] { };

                        RepairObject obj = new RepairObject() { Name = arr[3], DeckBoxNumber = arr[0], CollectorNumber = arr[2] };
                        Console.WriteLine($"Adding {obj}");
                        _repairList.Add(obj.DeckBoxNumber, obj);
                    }
                    rowNum++;
                }
            }
        }

        private static async Task ReadFromFirebase(string setCode)
        {
            Console.WriteLine("Firebase data");

            DocumentSnapshot setDoc = await _db.Collection("User_Inv").Document(setCode).GetSnapshotAsync();
            setDoc.TryGetValue<Dictionary<string, object>[]>("Cards", out _setData);
        }

        private static void FixData()
        {
            _newData = new Dictionary<string, object>[_setData.Length];
            int row = 0;
            foreach (Dictionary<string, object> curCard in _setData)
            {
                string curNumber = curCard?["CollectorNumber"].ToString() ?? "";
                string name = curCard?["Name"].ToString() ?? "";

                try
                {
                    RepairObject obj = _repairList?[curNumber] ?? new();
                    if (!obj.Name.Equals(name))
                    {
                        Console.WriteLine($"Warning -> {curNumber} doesn't match name: {name} != {obj.Name}");
                    }

                    Console.WriteLine($"Changing {name} ({curNumber}) to {obj.CollectorNumber}");
                    _newData[row] = new Dictionary<string, object>();
                    _newData[row].Add("Name", name);
                    _newData[row].Add("CollectorNumber", obj.CollectorNumber);
                    _newData[row].Add("Counts", curCard?["Counts"] ?? "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                row++;
            }
        }

        private static async Task WriteFixedData(string setCode)
        {
            CollectionReference collection = _db.Collection("User_Inv");
            Dictionary<string, object> cards = new Dictionary<string, object>
            {
                { "Cards", _newData }
            };
            await collection.Document(setCode).SetAsync(cards);
        }

        public static async Task Main()
        {
            string targetSet = "ice";

            ImportFileData(targetSet);
            await ReadFromFirebase(targetSet);
            FixData();
            await WriteFixedData(targetSet);
        }
    }
}