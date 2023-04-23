using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace YourNamespace
{
    public class CosmosClient : IDisposable
    {
        private readonly Container _cardsContainer;
        private readonly Container _setsContainer;
        private readonly Container _collectionsContainer;

        public CosmosClient(IConfiguration configuration)
        {
            var cosmosEndpointUri = configuration["CosmosDb:EndpointUri"];
            var cosmosPrimaryKey = configuration["CosmosDb:PrimaryKey"];
            var cosmosDatabaseName = configuration["CosmosDb:DatabaseName"];
            var cosmosCardsContainerName = configuration["CosmosDb:CardsContainerName"];
            var cosmosSetsContainerName = configuration["CosmosDb:SetsContainerName"];
            var cosmosCollectionsContainerName = configuration["CosmosDb:CollectionsContainerName"];

            var cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosPrimaryKey, new CosmosClientOptions());

            _cardsContainer = cosmosClient.GetContainer(cosmosDatabaseName, cosmosCardsContainerName);
            _setsContainer = cosmosClient.GetContainer(cosmosDatabaseName, cosmosSetsContainerName);
            _collectionsContainer = cosmosClient.GetContainer(cosmosDatabaseName, cosmosCollectionsContainerName);
        }

        public async Task<IEnumerable<Card>> GetCardsAsync(string setCode)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.set_code = @setCode")
                .WithParameter("@setCode", setCode);

            var iterator = _cardsContainer.GetItemQueryIterator<Card>(query);

            var cards = new List<Card>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                cards.AddRange(response.ToList());
            }

            return cards;
        }

        public async Task<IEnumerable<Set>> GetSetsAsync()
        {
            var iterator = _setsContainer.GetItemQueryIterator<Set>();

            var sets = new List<Set>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                sets.AddRange(response.ToList());
            }

            return sets;
        }

        public async Task<Set> GetSetAsync(string setCode)
        {
            var response = await _setsContainer.ReadItemAsync<Set>(setCode, new PartitionKey(setCode));

            return response.Resource;
        }

        public async Task<IEnumerable<Collection>> GetCollectionAsync(string setCode, string filter = null)
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.set_code = @setCode {(filter != null ? "AND " + filter : "")}")
                .WithParameter("@setCode", setCode);

            var iterator = _collectionsContainer.GetItemQueryIterator<Collection>(query);

            var collections = new List<Collection>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                collections.AddRange(response.ToList());
            }

            return collections;
        }

        public async Task<Collection> GetCollectionAsync(string setCode, string collectorNumber)
        {
            try
            {
                var response = await _collectionsContainer.ReadItemAsync<Collection>(collectorNumber, new PartitionKey(setCode));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task AddCardToCollectionAsync(string setCode, string collectorNumber)
        {
            // Get the card from the Cards table
            var cardResponse = await _containerCards.ReadItemAsync<Card>(collectorNumber, new PartitionKey(setCode));
            var card = cardResponse.Resource;

            // Check if the card already exists in the Collection table
            var collectionResponse = await _containerCollection.ReadItemAsync<Collection>(collectorNumber, new PartitionKey(setCode));
            if (collectionResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the card doesn't exist in the Collection table, create a new item
                var newCollectionItem = new Collection
                {
                    SetCode = setCode,
                    CollectorNumber = collectorNumber,
                    Name = card.Name,
                    Attrs = card.Attrs,
                    Count = 1
                };
                await _containerCollection.CreateItemAsync(newCollectionItem);
            }
            else
            {
                // If the card already exists in the Collection table, increment the count
                var collectionItem = collectionResponse.Resource;
                collectionItem.Count++;
                await _containerCollection.ReplaceItemAsync(collectionItem, collectionItem.Id, new PartitionKey(setCode));
            }
        }

        public async Task<ActionResult> AddItemAsync(string set, string card, [FromBody] Collection newItem)
        {
            newItem.SetCode = set;
            newItem.Name = card;
            newItem.Count = 1;

            try
            {
                ItemResponse<Collection> response = await _container.CreateItemAsync(newItem);
                return Ok(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Conflict($"Item with id {newItem.Id} already exists");
            }
        }
    }
}
