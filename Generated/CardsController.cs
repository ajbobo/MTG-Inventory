using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CardsController(IConfiguration configuration, CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], _configuration["CosmosDb:CardContainerName"]);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Card>>> GetCardsAsync(string set = null)
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM c");

            if (set != null)
            {
                query = new QueryDefinition("SELECT * FROM c WHERE c.Set = @set")
                    .WithParameter("@set", set);
            }

            List<Card> cards = new List<Card>();

            await foreach (Card card in _container.GetItemQueryIterator<Card>(query))
            {
                cards.Add(card);
            }

            return Ok(cards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCardAsync(string id)
        {
            try
            {
                ItemResponse<Card> response = await _container.ReadItemAsync<Card>(id, new PartitionKey(id));
                return Ok(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Card>> AddCardAsync([FromBody] Card card)
        {
            ItemResponse<Card> response = await _container.CreateItemAsync(card, new PartitionKey(card.Id));
            return Ok(response.Resource);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Card>> UpdateCardAsync(string id, [FromBody] Card card)
        {
            try
            {
                ItemResponse<Card> response = await _container.ReplaceItemAsync(card, id, new PartitionKey(id));
                return Ok(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCardAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<Card>(id, new PartitionKey(id));
                return NoContent();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }
    }
}
