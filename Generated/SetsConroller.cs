using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestApiDemo.Models;

namespace RestApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SetsController : ControllerBase
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public SetsController(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(configuration["DatabaseName"], configuration["SetsContainerName"]);
        }

        [HttpGet]
        public async Task<IEnumerable<Set>> Get()
        {
            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Set> queryResultSetIterator = _container.GetItemQueryIterator<Set>(queryDefinition);

            List<Set> sets = new List<Set>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Set> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Set set in currentResultSet)
                {
                    sets.Add(set);
                }
            }

            return sets;
        }

        [HttpGet("{code}")]
        public async Task<Set> Get(string code)
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.SetCode = @code";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@code", code);

            FeedIterator<Set> queryResultSetIterator = _container.GetItemQueryIterator<Set>(queryDefinition);

            Set set = null;
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Set> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                set = currentResultSet.FirstOrDefault();
            }

            return set;
        }
    }
}
