using mtg_api;
using Moq;
using System.Runtime.Caching;

namespace api_test;

[TestClass]
public class Test_SymbolsController
{
    private List<MTG_Symbol> CreateSymbols()
    {
        return new List<MTG_Symbol>
        {
            new MTG_Symbol() { Text = "{0}", URL = "http://zero" },
            new MTG_Symbol() { Text = "{B}", URL = "http://black" }
        };
    }

    [TestMethod]
    public async Task TestGetSets()
    {
        var cache = MemoryCache.Default;
        cache.Remove("symbols");

        Mock<IScryfall_Connection> conn = new();
        conn.Setup(o => o.GetSymbols())
            .ReturnsAsync(CreateSymbols());

        var controller = new SymbolsController(conn.Object, cache);
        var res = await controller.GetMTG_Symbols();

        Assert.IsNotNull(res.Value);
        Assert.AreEqual(2, res.Value.Count);
        Assert.AreEqual("{0}", res.Value[0].Text);
        Assert.AreEqual("http://black", res.Value[1].URL);

        Assert.IsNotNull(cache.Get("symbols"));

        Assert.AreEqual(1, conn.Invocations.Count);
    }
}