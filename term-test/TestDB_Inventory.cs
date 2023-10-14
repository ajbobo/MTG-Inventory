using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace MTG_CLI;

[TestClass]
public class TestDB_Inventory
{
    readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_InMemory"].ConnectionString;
    ISQL_Connection _sql = new SQLite_Connection(_sqliteFile);

    [TestMethod]
    public void TestCreatePopulateRead()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        DB_Inventory inv = new(_sql);
        inv.CreateDBTable();
        inv.PopulateDBTable("ajb", CreateData());

        string code;
        List<XCardData> data = inv.GetTableData(out code);

        Assert.AreEqual("ajb", code);
        Assert.AreEqual(2, data.Count);
        
        Assert.AreEqual("Card1", data[0]["Name"]);
        Dictionary<string, int> attrs = (Dictionary<string, int>)data[0]["Counts"];
        Assert.AreEqual(2, attrs["foil"]);
        Assert.AreEqual(1, attrs["standard"]);

        Assert.AreEqual("Card3", data[1]["Name"]);
        attrs = (Dictionary<string, int>)data[1]["Counts"];
        Assert.AreEqual(3, attrs["foil"]);
    }

    private XCardData[] CreateData()
    {
        XCardData card1 = new();
        card1.Add("CollectorNumber", 1);
        card1.Add("Name", "Card1");
        Dictionary<string, object> counts = new();
        counts.Add("standard", 1L);
        card1.Add("Counts", counts);

        XCardData card2 = new();
        card2.Add("CollectorNumber", 1);
        card2.Add("Name", "Card1");
        counts = new();
        counts.Add("foil", 2L);
        card2.Add("Counts", counts);

        XCardData card3 = new();
        card3.Add("CollectorNumber", 3);
        card3.Add("Name", "Card3");
        counts = new();
        counts.Add("foil", 3L);
        card3.Add("Counts", counts);

        return new[] {
            card1,
            card2,
            card3
        };
    }
}