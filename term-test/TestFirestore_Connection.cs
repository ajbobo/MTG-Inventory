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
using System;

namespace MTG_CLI;


[TestClass]
public class TestFirestore_Connection
{
    readonly private static string _sqlConnection = ConfigurationManager.ConnectionStrings["SQLite_InMemory"].ConnectionString;

    [TestMethod]
    public async Task TestReadData()
    {
        ISQL_Connection conn = new SQLite_Connection(_sqlConnection);
        IDB_Inventory inv = new DB_Inventory(conn);

        Mock<IFirestoreDB_Wrapper> mockFirestore = new();
        mockFirestore
            .Setup(r => r.GetDocumentField(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(GetCards()));

        Firestore_Connection firestoreConn = new(mockFirestore.Object, inv);
        await firestoreConn.ReadData("setName");

        string code;
        List<XCardData> list = inv.GetTableData(out code);

        Assert.AreEqual("setName", code);
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual("Card5", list[2]["Name"]);
    }

    [TestMethod]
    public async Task TestWriteData()
    {
        ISQL_Connection conn = new SQLite_Connection(_sqlConnection);
        IDB_Inventory inv = new DB_Inventory(conn);
        inv.CreateDBTable();
        inv.PopulateDBTable("setName", GetCards());

        Mock<IFirestoreDB_Wrapper> mockFirestore = new();
        mockFirestore
            .Setup(r => r.WriteDocumentField(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<XCardData[]>()))
            .Returns(Task.CompletedTask);

        Firestore_Connection firestoreConn = new(mockFirestore.Object, inv);
        await firestoreConn.WriteData();
    }

    private XCardData[] GetCards()
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

        XCardData card4 = new();
        card4.Add("CollectorNumber", 5);
        card4.Add("Name", "Card5");
        counts = new();
        counts.Add("standard", 4L);
        card4.Add("Counts", counts);

        return new[] {
            card1,
            card2,
            card3,
            card4
        };
    }
}