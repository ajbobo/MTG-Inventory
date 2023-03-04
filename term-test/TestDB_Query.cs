using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MTG_CLI;

[TestClass]
public class TestDB_Query
{
    [TestMethod]
    public void Test_QueryCreation()
    {
        DB_Query[] queries = {
            DB_Query.CREATE_SET_TABLE,
            DB_Query.INSERT_SET,
            DB_Query.GET_ALL_SETS,
            DB_Query.GET_SET_NAME,
            DB_Query.GET_SET_CODE,
            DB_Query.CREATE_CARD_TABLE,
            DB_Query.INSERT_CARD,
            DB_Query.GET_SET_CARDS,
            DB_Query.GET_SINGLE_CARD_COUNT,
            DB_Query.GET_CARD_DETAILS,
            DB_Query.GET_CARD_NAMES,
            DB_Query.GET_CARD_NUMBER,
            DB_Query.CREATE_USER_INVENTORY,
            DB_Query.ADD_TO_USER_INVENTORY,
            DB_Query.GET_USER_INVENTORY,
            DB_Query.GET_CARD_CTCS,
            DB_Query.UPDATE_CARD_CTC
        };

        // Make sure that none of the queries are the same
        for (int x = 0; x < queries.Length - 1; x++)
        {
            for (int y = x + 1; y < queries.Length; y++)
                Assert.AreNotEqual(queries[x], queries[y]);
        }

        // Make sure that every call to get a single query always returns the same instance
        DB_Query q = DB_Query.CREATE_SET_TABLE;
        Assert.AreEqual(q, queries[0]);
        Assert.IsTrue(q == queries[0]);
    }

    [TestMethod]
    public void TestNullComparison()
    {
        DB_Query q = DB_Query.GET_CARD_CTCS;

        Assert.IsFalse(q.Equals(null));
    }

    [TestMethod]
    public void TestHashCodes()
    {
        DB_Query q1 = DB_Query.INSERT_SET;
        DB_Query q2 = DB_Query.INSERT_SET;
        DB_Query q3 = DB_Query.GET_CARD_DETAILS;

        Assert.AreEqual(q1.GetHashCode(), q2.GetHashCode());
        Assert.AreNotEqual(q1.GetHashCode(), q3.GetHashCode());
    }
}