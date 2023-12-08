using mtg_api;
using Moq;
using Moq.Protected;

namespace api_test;

[TestClass]
public class Test_Scryfall_Connection
{
    [DataTestMethod]
    [DataRow("core", "", "")]
    [DataRow("expansion", "", "")]
    [DataRow("masterpiece", "", "")]
    [DataRow("masters", "", "")]
    [DataRow("commander", "", "")]
    [DataRow("draft_innovation", "", "")]
    [DataRow("funny", "", "")]
    public void TestCollectableSetTypes(string type, string block, string parent)
    {
        var con = new Scryfall_Connection(new HttpClient());

        Assert.IsTrue(con.IsCollectableSetType(type, block, parent));
    }

    [DataTestMethod]
    [DataRow("alchemy", "", "")]
    [DataRow("archenemy", "", "")]
    [DataRow("arsenal", "", "")]
    [DataRow("box", "", "")]
    [DataRow("duel_deck", "", "")]
    [DataRow("from_the_vault", "", "")]
    [DataRow("memorabilia", "", "")]
    [DataRow("minigame", "", "")]
    [DataRow("planechase", "", "")]
    [DataRow("premium_deck", "", "")]
    [DataRow("promo", "", "")]
    [DataRow("spellbook", "", "")]
    [DataRow("starter", "", "")]
    [DataRow("token", "", "")]
    [DataRow("treasure_chest", "", "")]
    [DataRow("vanguard", "", "")]
    [DataRow("funny", "", "plist")]
    [DataRow("funny", "htr", "")]
    public void TestNonCollectableSetTypes(string type, string block, string parent)
    {
        var con = new Scryfall_Connection(new HttpClient());

        Assert.IsFalse(con.IsCollectableSetType(type, block, parent));
    }

    [TestMethod]
    public async Task TestGetSets()
    {
        HttpResponseMessage resp = new();
        string respText = File.ReadAllText("MockResults/Scryfall-Sets.json");
        resp.Content = new StringContent(respText);

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SetListUri = "htto://anyURIisFine" };
        var setList = await conn.GetCollectableSets();

        Assert.AreEqual(202, setList.Count);
        Assert.AreEqual("Fourth Edition Foreign Black Border", setList[189].Name);
        Assert.AreEqual("Dominaria Remastered", setList[0].Name);
    }

    [TestMethod]
    public async Task TestGetCards()
    {
        HttpResponseMessage resp1 = new();
        string respText = File.ReadAllText("MockResults/Scryfall-Cards-1.json");
        resp1.Content = new StringContent(respText);

        HttpResponseMessage resp2 = new();
        respText = File.ReadAllText("MockResults/Scryfall-Cards-2.json");
        resp2.Content = new StringContent(respText);

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.Contains("page=1")), 
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp1);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Query.Contains("page=2")), 
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp2);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SetSearchUri = "htto://anyURIisFine?page={1}" };
        var cardList = await conn.GetCardsInSet("mid");

        Assert.AreEqual(350, cardList.Count);
        Assert.AreEqual("Diregraf Horde", cardList[100].Name);
        Assert.AreEqual("Primal Adversary", cardList[200].Name);
    }

    [TestMethod]
    public async Task TestGetSymbols()
    {
        HttpResponseMessage resp = new();
        string respText = File.ReadAllText("MockResults/Scryfall-Symbols.json");
        resp.Content = new StringContent(respText);

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SymbolSearchUri = "htto://anyURIisFine" };
        var symbolList = await conn.GetSymbols();

        Assert.AreEqual(51, symbolList.Count);
        Assert.AreEqual("{7}", symbolList[10].Text);
        Assert.AreEqual("{U}", symbolList[45].Text);
    }

    [TestMethod]
    public async Task TestGetCardsConnectionFailed()
    {
        HttpResponseMessage resp = new() { StatusCode = System.Net.HttpStatusCode.NotFound };

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SetSearchUri = "htto://anyURIisFine" };
        List<MTG_Card> list = await conn.GetCardsInSet("dom");

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public async Task TestGetSetsConnectionFailed()
    {
        HttpResponseMessage resp = new() { StatusCode = System.Net.HttpStatusCode.NotFound };

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SetListUri = "htto://anyURIisFine" };
        List<MTG_Set> list = await conn.GetCollectableSets();

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public async Task TestGetSymbolConnectionFailed()
    {
        HttpResponseMessage resp = new() { StatusCode = System.Net.HttpStatusCode.NotFound };

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        var conn = new Scryfall_Connection(new HttpClient(mockHandler.Object)) { SymbolSearchUri = "htto://anyURIisFine" };
        List<MTG_Symbol> list = await conn.GetSymbols();

        Assert.AreEqual(0, list.Count);
    }
}