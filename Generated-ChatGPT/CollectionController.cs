using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MyNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly ILogger<CollectionController> _logger;
        private readonly CosmosClient _cosmosClient;

        public CollectionController(ILogger<CollectionController> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        // GET: /collection/{set}
        [HttpGet("{set}")]
        public async Task<IEnumerable<Collection>> GetCollectionBySet(string set, [FromQuery] string filter = null)
        {
            try
            {
                var collection = await _cosmosClient.GetCollectionBySetAsync(set);
                if (filter != null)
                {
                    // Apply filter if provided
                    var filterJObject = JObject.Parse(filter);
                    collection = collection.Where(c =>
                        filterJObject.Properties().All(prop => c.Attrs.ContainsKey(prop.Name) && c.Attrs[prop.Name].ToString() == prop.Value.ToString()));
                }
                return collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get collection by set {set}", set);
                throw;
            }
        }

        // GET: /collection/{set}/{card}
        [HttpGet("{set}/{card}")]
        public async Task<ActionResult<Collection>> GetCollectionBySetAndCard(string set, string card)
        {
            try
            {
                var collection = await _cosmosClient.GetCollectionBySetAndCardAsync(set, card);
                if (collection == null)
                {
                    return NotFound();
                }
                return collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get collection by set {set} and card {card}", set, card);
                throw;
            }
        }

        // POST: /collection/{set}/{card}
        [HttpPost("{set}/{card}")]
        public async Task<ActionResult> AddCardToCollectionAsync(string set, string card, [FromBody] Collection collection)
        {
            try
            {
                if (collection.SetCode != set || collection.CollectorNumber != card)
                {
                    return BadRequest("Mismatched SetCode or CollectorNumber in body");
                }
                await _cosmosClient.AddCardToCollectionAsync(collection);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add card to collection for set {set} and card {card}", set, card);
                throw;
            }
        }
    }
}
