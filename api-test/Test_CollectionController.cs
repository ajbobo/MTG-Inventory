using mtg_api;
using Moq;
using System.Runtime.Caching;

namespace api_test;

[TestClass]
public class Test_CollectionController
{
    private static List<CardData> CreateCards()
    {
        return new List<CardData>
        {
            new() { Card = new() { CollectorNumber = "1", Name = "One", Rarity = "Common", Price = 0.20M, PriceFoil = 0.30M, ColorIdentity = "W" }, TotalCount = 1 },
            new() { Card = new() { CollectorNumber = "2", Name = "Two", Rarity = "Uncommon", Price = 0.30M, PriceFoil = 0.30M, ColorIdentity = "U" }, TotalCount = 2 },
            new() { Card = new() { CollectorNumber = "3", Name = "Three", Rarity = "Rare", Price = 0.40M, PriceFoil = 1.30M, ColorIdentity = "B" }, TotalCount = 3 },
            new() { Card = new() { CollectorNumber = "4", Name = "Four", Rarity = "Mythic", Price = 1.20M, PriceFoil = 1.30M, ColorIdentity = "R" }, TotalCount = 4 },
            new() { Card = new() { CollectorNumber = "5", Name = "Five", Rarity = "Common", Price = 1.30M, PriceFoil = 1.30M, ColorIdentity = "G" }, TotalCount = 0 },
            new() { Card = new() { CollectorNumber = "6", Name = "Six", Rarity = "Uncommon", Price = 10.40M, PriceFoil = 11.30M, ColorIdentity = "" }, TotalCount = 1 },
            new() { Card = new() { CollectorNumber = "7", Name = "Seven", Rarity = "Rare", Price = 10.20M, PriceFoil = 10.30M, ColorIdentity = "WU" }, TotalCount = 2 },
            new() { Card = new() { CollectorNumber = "8", Name = "Eight", Rarity = "Mythic", Price = 20.20M, PriceFoil = 20.30M, ColorIdentity = "UB" }, TotalCount = 3 },
            new() { Card = new() { CollectorNumber = "9", Name = "Nine", Rarity = "Common", Price = 20.20M, PriceFoil = 20.30M, ColorIdentity = "BR" }, TotalCount = 4 },
            new() { Card = new() { CollectorNumber = "10", Name = "Ten", Rarity = "Uncommon", Price = 30.20M, PriceFoil = 20.30M, ColorIdentity = "RG" }, TotalCount = 0 },
            new() { Card = new() { CollectorNumber = "11", Name = "Eleven", Rarity = "Rare", Price = 100.20M, PriceFoil = 101.30M, ColorIdentity = "GW" }, TotalCount = 1 },
        };
    }

    private static CollectionController SetupController()
    {
        var cache = MemoryCache.Default;

        Mock<IScryfall_Connection> conn = new();
        Mock<MtgInvContext> context = new();

        var controller = new CollectionController(context.Object, conn.Object, cache);
        return controller;
    }

    [TestMethod]
    public void TestFilterByCollectorNumber_AllCards()
    {
        var res = CollectionController.FilterByNumber("", CreateCards());

        Assert.AreEqual(11, res.Count());
        for (int x = 1; x <= 11; x++)
            Assert.AreEqual(x.ToString(), res.ToArray()[x - 1].Card!.CollectorNumber);
    }

    [TestMethod]
    public void TestFilterByCollectorNumber_SingleCard()
    {
        var res = CollectionController.FilterByNumber("5", CreateCards());

        Assert.AreEqual(1, res.Count());
        Assert.AreEqual("Five", res.ToArray()[0].Card!.Name);
    }

    [TestMethod]
    public void TestFilterByColor_AllCards()
    {
        var res = CollectionController.FilterByColor("", CreateCards());

        Assert.AreEqual(11, res.Count());
        for (int x = 1; x <= 11; x++)
            Assert.AreEqual(x.ToString(), res.ToArray()[x - 1].Card!.CollectorNumber);
    }

    [TestMethod]
    public void TestFilterByColor_SingleColor()
    {
        var res = CollectionController.FilterByColor("W", CreateCards());

        Assert.AreEqual(3, res.Count());
        Assert.AreEqual("One", res.ToArray()[0].Card!.Name);
        Assert.AreEqual("Seven", res.ToArray()[1].Card!.Name);
        Assert.AreEqual("Eleven", res.ToArray()[2].Card!.Name);
    }

    [TestMethod]
    public void TestFilterByColor_TwoColors()
    {
        var res = CollectionController.FilterByColor("WB", CreateCards());

        Assert.AreEqual(6, res.Count());
        Assert.AreEqual("One", res.ToArray()[0].Card!.Name);
        Assert.AreEqual("Three", res.ToArray()[1].Card!.Name);
        Assert.AreEqual("Seven", res.ToArray()[2].Card!.Name);
        Assert.AreEqual("Eight", res.ToArray()[3].Card!.Name);
        Assert.AreEqual("Nine", res.ToArray()[4].Card!.Name);
        Assert.AreEqual("Eleven", res.ToArray()[5].Card!.Name);
    }

    [TestMethod]
    public void TestInsideStringTrue()
    {
        Assert.IsTrue(CollectionController.InsideString("W","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("U","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("B","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("R","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("G","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("UW","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("BU","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("RB","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("GR","WUBRG"));
        Assert.IsTrue(CollectionController.InsideString("WG","WUBRG"));
        Assert.IsFalse(CollectionController.InsideString("Q","WUBRG"));
    }

    [TestMethod]
    public void TestInsideStringFalse()
    {
        Assert.IsFalse(CollectionController.InsideString("Q","WUBRG"));
        Assert.IsFalse(CollectionController.InsideString("w","WUBRG"));
    }
}