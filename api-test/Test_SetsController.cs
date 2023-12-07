using mtg_api;
using Moq;
using System.Runtime.Caching;

namespace api_test;

[TestClass]
public class Test_SetsController
{
    private List<MTG_Set> CreateSets()
    {
        return new List<MTG_Set>
        {
            new MTG_Set() { Code = "dom", Name = "Dominaria" },
            new MTG_Set() { Code = "dmu", Name = "Dominaria United" },
            new MTG_Set() { Code = "mid", Name = "Midnight Hunt" }
        };
    }

    [TestMethod]
    public async Task TestGetSets()
    {
        var cache = MemoryCache.Default;
        cache.Remove("sets");

        Mock<IScryfall_Connection> conn = new();
        conn.Setup(o => o.GetCollectableSets())
            .ReturnsAsync(CreateSets());

        var controller = new SetsController(conn.Object, cache);
        var res = await controller.GetMTG_Sets();

        Assert.IsNotNull(res.Value);
        Assert.AreEqual(3, res.Value.Count);
        Assert.AreEqual("Dominaria", res.Value[0].Name);
        Assert.AreEqual("dmu", res.Value[1].Code);
        Assert.AreEqual("Midnight Hunt", res.Value[2].Name);

        Assert.IsNotNull(cache.Get("sets"));

        Assert.AreEqual(1, conn.Invocations.Count);
    }
}