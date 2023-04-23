using System;
using System.Data.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzureCosmosDb
{
    public class ApiController : ApiControllerBase
    {
        private readonly CosmosClient _client;

        public ApiController(CosmosClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IEnumerable<Card> GetCards()
        {
            // Create a CosmosQuery object.
            CosmosQuery query = new CosmosQuery("SELECT * FROM Cards");

            // Execute the query.
            CosmosResults results = _client.GetDatabase("YourDatabaseName")
                                              .GetContainer("YourCollectionName")
                                              .ExecuteQuery(query);

            // Iterate through the results.
            foreach (var item in results)
            {
                // Get the values from the current row.
                int id = item["Id"] as int;
                string name = item["Name"] as string;
                int manaCost = item["ManaCost"] as int;
                string type = item["Type"] as string;

                // Create a new Card object.
                Card card = new Card
                {
                    Id = id,
                    Name = name,
                    ManaCost = manaCost,
                    Type = type
                };

                // Add the card to the results.
                yield return card;
            }
        }

        [HttpPost]
        public void AddCard([FromBody] Card card)
        {
            // Create a CosmosItem object.
            CosmosItem item = new CosmosItem("Cards");

            // Set the item's properties.
            item["Id"] = card.Id;
            item["Name"] = card.Name;
            item["ManaCost"] = card.ManaCost;
            item["Type"] = card.Type;

            // Add the item to the database.
            _client.GetDatabase("YourDatabaseName")
                   .GetContainer("YourCollectionName")
                   .UpsertItem(item);
        }

        [HttpPut]
        public void UpdateCard([FromBody] Card card)
        {
            // Create a CosmosItem object.
            CosmosItem item = new CosmosItem("Cards", card.Id);

            // Set the item's properties.
            item["Name"] = card.Name;
            item["ManaCost"] = card.ManaCost;
            item["Type"] = card.Type;

            // Update the item in the database.
            _client.GetDatabase("YourDatabaseName")
                   .GetContainer("YourCollectionName")
                   .ReplaceItem(item);
        }

        [HttpDelete]
        public void DeleteCard(int id)
        {
            // Delete the item from the database.
            _client.GetDatabase("YourDatabaseName")
                   .GetContainer("YourCollectionName")
                   .DeleteItem("Cards", id);
        }

        [HttpGet]
        public IEnumerable<Set> GetSets()
        {
            // Create a CosmosQuery object.
            CosmosQuery query = new CosmosQuery("SELECT * FROM Sets");

            // Execute the query.
            CosmosResults results = _client.GetDatabase("YourDatabaseName")
                                              .GetContainer("YourCollectionName")
                                              .ExecuteQuery(query);

            // Iterate through the results.
            foreach (var item in results)
            {
                // Get the values from the current row.
                string name = item["Name"] as string;
                string setCode = item["SetCode"] as string;

                // Create a new Set object.
                Set set = new Set
                {
                    Name = name,
                    SetCode = setCode
                };

                // Add the set to the results.
                yield return set;
            }
        }

        [HttpGet]
        public IEnumerable<Card> GetCardsBySetCode(string setCode)
        {
            // Create a CosmosQuery object.
            CosmosQuery query = new CosmosQuery($"SELECT * FROM Cards WHERE SetCode = '{setCode}'");

            // Execute the query.
            CosmosResults results = _client.GetDatabase("YourDatabaseName")
                                            .GetContainer("YourCollectionName")
                                            .ExecuteQuery(query);

            // Iterate through the results.
            foreach (var item in results)
            {
                // Get the values from the current row.
                int id = item["Id"] as int;
                string name = item["Name"] as string;
                int manaCost = item["ManaCost"] as int;
                string type = item["Type"] as string;

                // Create a new Card object.
                Card card = new Card
                {
                    Id = id,
                    Name = name,
                    ManaCost = manaCost,
                    Type = type
                };

                // Add the card to the results.
                yield return card;
            }
        }
    }
}

// Card data class
public class Card
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ManaCost { get; set; }
    public string Type { get; set; }
}

// Set data class
public class Set
{
    public string Name { get; set; }
    public string SetCode { get; set; }
}

public class Collection
{
    private readonly List<Card> _cards;

    public Collection()
    {
        _cards = new List<Card>();
    }

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    public void RemoveCard(Card card)
    {
        _cards.Remove(card);
    }

    public IEnumerable<Card> GetCards()
    {
        return _cards;
    }

    public int Count
    {
        get { return _cards.Count; }
    }
}