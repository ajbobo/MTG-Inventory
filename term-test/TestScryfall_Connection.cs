using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Configuration;
using System.IO;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MTG_CLI;

[TestClass]
public class TestScryfall_Connection
{
    readonly private static string _sqlConnection = ConfigurationManager.ConnectionStrings["SQLite_InMemory"].ConnectionString;

    // I think these are all the set types that Scryfall returns, with a couple of variations for "funny"
    [DataTestMethod]
    [DataRow("alchemy", "Alchemy 222", "neo", false)]
    [DataRow("archenemy", "", "arc", false)]
    [DataRow("arsenal", "Commander", "", false)]
    [DataRow("box", "Ice Age", "csp", false)]
    [DataRow("commander", "Commander", "mid", true)]
    [DataRow("core", "Core Set", "", true)]
    [DataRow("draft_innovation", "Conspiracy", "", false)]
    [DataRow("duel_deck", "", "", false)]
    [DataRow("expansion", "Alara", "", true)]
    [DataRow("from_the_vault", "", "", false)]
    [DataRow("funny", "Heroes of the Realm", "", false)]
    [DataRow("funny", "", "unf", false)]
    [DataRow("funny", "", "", true)]
    [DataRow("masterpiece", "Amonkhet", "akh", true)]
    [DataRow("masters", "", "mb1", true)]
    [DataRow("memorabilia", "Commander", "c13", false)]
    [DataRow("planechase", "", "hop", false)]
    [DataRow("premium_deck", "", "", false)]
    [DataRow("promo", "Alara", "ala", false)]
    [DataRow("spellbook", "", "", false)]
    [DataRow("starter", "Khans of Tarkir", "frf", false)]
    [DataRow("token", "Alara", "ala", false)]
    [DataRow("treasure_chest", "", "", false)]
    [DataRow("vanguard", "", "", false)]
    public void TestCollectableSetType(string setType, string block, string parent, bool expected)
    {
        Mock<ISQL_Connection> mockSql = new();
        Mock<HttpClient> mockClient = new();
        Scryfall_Connection conn = new(mockSql.Object, mockClient.Object);

        Assert.AreEqual(expected, conn.IsCollectableSetType(setType, block, parent));
    }

    [TestMethod]
    public async Task TestGetSets()
    {
        ISQL_Connection sql = new SQLite_Connection(_sqlConnection);

        HttpResponseMessage resp = new();
        string respText = File.ReadAllText("MockResults/Scryfall-Sets.json");
        resp.Content = new StringContent(respText);

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        Scryfall_Connection conn = new(sql, new HttpClient(mockHandler.Object));
        bool res = await conn.GetCollectableSets();

        List<string> setList = new();
        sql.Query(DB_Query.GET_ALL_SETS).OpenToRead();
        while (sql.ReadNext())
        {
            setList.Add(sql.ReadValue<string>("Name", ""));
        }
        sql.Close();

        Assert.AreEqual(190, setList.Count);
        Assert.AreEqual("Limited Edition Alpha", setList[189]);
        Assert.AreEqual("Dominaria Remastered", setList[0]);
    }

    [TestMethod]
    public async Task TestEmptySets()
    {
        ISQL_Connection sql = new SQLite_Connection(_sqlConnection);

        HttpResponseMessage resp = new();
        string respText = File.ReadAllText("MockResults/Empty-Sets.json");
        resp.Content = new StringContent(respText);

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        Scryfall_Connection conn = new(sql, new HttpClient(mockHandler.Object));
        bool res = await conn.GetCollectableSets();

        List<string> setList = new();
        sql.Query(DB_Query.GET_ALL_SETS).OpenToRead();
        while (sql.ReadNext())
        {
            setList.Add(sql.ReadValue<string>("Name", ""));
        }
        sql.Close();

        Assert.AreEqual(0, setList.Count);
    }

    [TestMethod]
    public async Task TestGetSetFailure()
    {
        ISQL_Connection sql = new SQLite_Connection(_sqlConnection);

        HttpResponseMessage resp = new();
        resp.StatusCode = System.Net.HttpStatusCode.Forbidden;

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        Scryfall_Connection conn = new(sql, new HttpClient(mockHandler.Object));
        bool res = await conn.GetCollectableSets();

        Assert.IsFalse(res);
    }

    [TestMethod]
    public async Task TestGetCardsInSet()
    {
        ISQL_Connection sql = new SQLite_Connection(_sqlConnection);

        HttpResponseMessage resp1 = new();
        string respText1 = File.ReadAllText("MockResults/Scryfall-Cards-1.json");
        resp1.Content = new StringContent(respText1);

        HttpResponseMessage resp2 = new();
        string respText2 = File.ReadAllText("MockResults/Scryfall-Cards-2.json");
        resp2.Content = new StringContent(respText2);

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

        Scryfall_Connection conn = new(sql, new HttpClient(mockHandler.Object));
        bool res = await conn.GetCardsInSet("dmu");

        List<string> cardList = new();
        sql.Query(DB_Query.GET_CARD_NAMES).OpenToRead();
        while (sql.ReadNext())
        {
            cardList.Add(sql.ReadValue<string>("Name", ""));
        }
        sql.Close();

        Assert.IsTrue(res);
        Assert.AreEqual(272, cardList.Count);
        Assert.AreEqual("Karn, Living Legacy", cardList[0]);
        Assert.AreEqual("Phyrexian Vivisector", cardList[99]);
        Assert.AreEqual("Ivy, Gleeful Spellthief", cardList[200]);
        Assert.AreEqual("Briar Hydra", cardList[270]);
        Assert.AreEqual("Ambitious Farmhand // Seasoned Cathar", cardList[271]);
    }

    [TestMethod]
    public async Task TestGetCardsFailure()
    {
        ISQL_Connection sql = new SQLite_Connection(_sqlConnection);

        HttpResponseMessage resp = new();
        resp.StatusCode = System.Net.HttpStatusCode.Forbidden;

        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(resp);

        Scryfall_Connection conn = new(sql, new HttpClient(mockHandler.Object));
        bool res = await conn.GetCardsInSet("dmu");

        Assert.IsFalse(res);
    }
}