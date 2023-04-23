Sure, here's an example of how you could build a Windows C# app to accomplish this:

1. Create a new Windows Forms Application project in Visual Studio.

2. Add the necessary NuGet packages for the Cosmos DB SDK:

   ```
   Microsoft.Azure.Cosmos
   ```

3. Create a new class called `Card` to represent a card in the collection. This should match the definition in the API.

   ```csharp
   public class Card
   {
       public string SetCode { get; set; }
       public int CollectorNumber { get; set; }
       public string Name { get; set; }
       public string Attrs { get; set; }
       public int Count { get; set; }
   }
   ```

4. Create a new class called `Set` to represent a set. This should match the definition in the API.

   ```csharp
   public class Set
   {
       public string SetName { get; set; }
       public string SetCode { get; set; }
   }
   ```

5. Create a new class called `CosmosDbClient` to handle interactions with the Cosmos DB.

   ```csharp
   using Microsoft.Azure.Cosmos;
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Threading.Tasks;
   
   public class CosmosDbClient
   {
       private readonly CosmosClient _client;
       private readonly Container _container;
   
       public CosmosDbClient(string endpointUri, string primaryKey, string databaseName, string containerName)
       {
           _client = new CosmosClient(endpointUri, primaryKey);
           _container = _client.GetContainer(databaseName, containerName);
       }
   
       public async Task<List<Card>> GetCardsInCollectionAsync(string setCode)
       {
           var queryDefinition = new QueryDefinition($"SELECT * FROM c WHERE c.SetCode = '{setCode}'");
           var queryResultSetIterator = _container.GetItemQueryIterator<Card>(queryDefinition);
   
           var cards = new List<Card>();
           while (queryResultSetIterator.HasMoreResults)
           {
               var response = await queryResultSetIterator.ReadNextAsync();
               cards.AddRange(response.ToList());
           }
   
           return cards;
       }
   
       public async Task<List<Set>> GetSetsAsync()
       {
           var queryDefinition = new QueryDefinition("SELECT * FROM c");
           var queryResultSetIterator = _container.GetItemQueryIterator<Set>(queryDefinition);
   
           var sets = new List<Set>();
           while (queryResultSetIterator.HasMoreResults)
           {
               var response = await queryResultSetIterator.ReadNextAsync();
               sets.AddRange(response.ToList());
           }
   
           return sets;
       }
   }
   ```

   This class uses the Cosmos DB SDK to query the database for cards in a specific set and to get the list of sets available in the database.

6. In the Windows Forms designer, add a ComboBox to the form to display the list of sets.

7. In the form's code-behind file, add the following code to populate the ComboBox with the available sets:

   ```csharp
   private readonly CosmosDbClient _cosmosDbClient = new CosmosDbClient("<cosmos db endpoint uri>", "<cosmos db primary key>", "<cosmos db database name>", "<cosmos db container name>");
   
   private async void Form1_Load(object sender, EventArgs e)
   {
       var sets = await _cosmosDbClient.GetSetsAsync();
       var setNames = sets.Select(s => s.SetName).ToArray();
       comboBox1.Items.AddRange(setNames);
   }
   ```

   This code initializes a new instance of `CosmosDbClient` and calls the `GetSetsAsync` method to retrieve the available sets from the database