using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MTG_CLI;

[TestClass]
public class TestCardData
{
    [TestMethod]
    public void EmptyCardData()
    {
        CardData card = new();
        Assert.AreEqual(0, card.Keys.Length);
    }

    [TestMethod]
    public void AssignFields()
    {
        CardData card = new();
        card["one"] = 1;
        card["two"] = "2";

        Assert.AreEqual(2, card.Keys.Length);
        Assert.AreEqual(1, card["one"]);
        Assert.AreEqual("2", card["two"]);
    }

    [TestMethod]
    public void AddFields()
    {
        CardData card = new();
        card.Add("one", 1);
        card.Add("two", "2");

        Assert.AreEqual(2, card.Keys.Length);
        Assert.AreEqual(1, card["one"]);
        Assert.AreEqual("2", card["two"]);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void InvalidField()
    {
        CardData card = new();
        int val = (int)card["Invalid"];
    }
}

[TestClass]
public class TestCardDataConverter
{
    [TestMethod]
    public void ToFirestore()
    {
        CardData card = new();
        card["one"] = 1;
        card["two"] = "2";

        CardDataConverter convert = new();
        object res = convert.ToFirestore(card);

        Assert.IsInstanceOfType(res,typeof(Dictionary<string, object>));

        var dict = (Dictionary<string, object>)res;
        Assert.AreEqual(2, dict.Keys.Count);
        Assert.AreEqual(1, dict["one"]);
        Assert.AreEqual("2", dict["two"]);
    }

    [TestMethod]
    public void FromFirestore()
    {
        Dictionary<string, object> dict = new()
        {
            {"one", 1},
            {"two", "2"}
        };

        CardDataConverter convert = new();
        CardData res = convert.FromFirestore(dict);

        Assert.AreEqual(2, res.Keys.Length);
        Assert.AreEqual(1, res["one"]);
        Assert.AreEqual("2", res["two"]);
    }
}