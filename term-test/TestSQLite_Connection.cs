using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace MTG_CLI;

[TestClass]
public class TestSQLite_Connection
{
    readonly private static string _sqliteFile = ConfigurationManager.ConnectionStrings["SQLite_InMemory"].ConnectionString;

    // This initializes the DB with Dominaria Unitied data, including some inventory data
    string setupQuery = File.ReadAllText("CreateTestDB.sql");
    ISQL_Connection _sql = new SQLite_Connection(_sqliteFile);

    [TestInitialize]
    public void BeforeAll()
    {
        if (null == _sql) return;

        _sql.Query(setupQuery).Execute();
    }

    [TestMethod]
    public void TestSimpleQuery()
    {
        _sql.Query(DB_Query.GET_ALL_SETS).OpenToRead();

        List<string> setList = new();
        while (_sql.ReadNext())
        {
            string setName = _sql.ReadValue<string>("Name", "");
            setList.Add(setName);
        }
        _sql.Close();

        Assert.AreEqual(187, setList.Count);
    }

    [TestMethod]
    public void TestQueryWithStringParam()
    {
        _sql.Query(DB_Query.GET_SET_CODE)
            .WithParam("@Name", "Dominaria United");
        string? code = _sql.ExecuteScalar<string>();

        Assert.AreEqual("dmu", code);
    }

    [TestMethod]
    public void TestQueryWithIntParam()
    {
        _sql.Query(DB_Query.GET_CARD_DETAILS)
            .WithParam("@CollectorNumber", 31)
            .OpenToRead();
        if (_sql.IsReady())
            _sql.ReadNext();
        string name = _sql.ReadValue<string>("Name", "");
        _sql.Close();

        Assert.AreEqual("Samite Herbalist", name);
    }

    [TestMethod]
    public void TestQueryWithLongParam()
    {
        _sql.Query(DB_Query.GET_CARD_DETAILS)
            .WithParam("@CollectorNumber", 32L)
            .OpenToRead();
        if (_sql.IsReady())
            _sql.ReadNext();
        string name = _sql.ReadValue<string>("Name", "");
        _sql.Close();

        Assert.AreEqual("Serra Paragon", name);
    }

    [TestMethod]
    public void TestFilters()
    {
        FilterSettings filterSettings = new();
        filterSettings.ToggleFilter(RarityFilter.RARE, true);
        filterSettings.ToggleFilter(CountFilter.CNT_ONE_PLUS, true);
        filterSettings.ToggleFilter(ColorFilter.GREEN, true);
        filterSettings.ToggleFilter(ColorFilter.BLACK, true);

        _sql.Query(DB_Query.GET_SET_CARDS)
            .WithFilters(filterSettings)
            .OpenToRead();

        List<NameCnt> list = new();
        while (_sql.ReadNext())
        {
            list.Add(new()
            {
                Name = _sql.ReadValue<string>("Name", ""),
                Cnt = _sql.ReadValue<int>("CntNum", 0)
            });
        }
        _sql.Close();

        Assert.AreEqual(9, list.Count);
        Assert.AreEqual("The Raven Man", list[1].Name);
        Assert.AreEqual(1, list[1].Cnt);
        Assert.AreEqual("Defiler of Vigor", list[2].Name);
        Assert.AreEqual(2, list[2].Cnt);
        Assert.AreEqual("Ertai Resurrected", list[7].Name);
        Assert.AreEqual(1, list[7].Cnt);
    }

    [TestMethod]
    public void TestEmptyFilters()
    {
        _sql.Query(DB_Query.GET_SET_CARDS)
            .WithFilters(new())
            .OpenToRead();

        List<NameCnt> list = new();
        while (_sql.ReadNext())
        {
            list.Add(new()
            {
                Name = _sql.ReadValue<string>("Name", ""),
                Cnt = _sql.ReadValue<int>("CntNum", 0)
            });
        }
        _sql.Close();

        Assert.AreEqual(434, list.Count);
    }

    [TestMethod]
    public void TestNotExistingField()
    {
        _sql.Query(DB_Query.GET_CARD_DETAILS).WithParam("@CollectorNumber", 5).OpenToRead();
        if (_sql.IsReady())
            _sql.ReadNext();
        string val = _sql.ReadValue<string>("DoesntExist", "NotReal");
        string name = _sql.ReadValue<string>("Name", "");
        _sql.Close();

        Assert.AreEqual("NotReal", val);
        Assert.AreEqual("Argivian Phalanx", name);
    }

    [TestMethod]
    public void TestNoQuery()
    {
        // Create a new SQLite_Connection here so that _sql doesn't get screwed up
        ISQL_Connection sql = new SQLite_Connection(_sqliteFile);

        Assert.IsFalse(sql.IsReady());

        // These shouldn't cause a crash
        sql.OpenToRead();
        sql.WithParam("@InvalidStr", "n/a");
        sql.WithParam("@InvalidInt", 1);
        sql.WithParam("@InvalidLong", 123L);
        sql.WithFilters(new());
        sql.Close();

        Assert.AreEqual(0, sql.Execute());
        Assert.AreEqual(0, sql.ExecuteScalar<int>());
        Assert.AreEqual(null, sql.ExecuteScalar<string>());
        Assert.AreEqual("n/a", sql.ReadValue<string>("DoesntExist", "n/a"));
        Assert.AreEqual(-1, sql.ReadValue<int>("DoesntExist", -1));

        Assert.IsFalse(sql.ReadNext());
    }

    class NameCnt
    {
        public string Name { get; set; } = "";
        public int Cnt { get; set; }
    }
}
