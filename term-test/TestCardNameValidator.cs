using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using NStack;
using System.Configuration;

namespace MTG_CLI;

[TestClass]
public class TestCardNameValidator
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
    public void TestStartWithWord()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "Karn";
        CheckState(validator, "Karn", "Karn, Living Legacy");
    }

    [TestMethod]
    public void TestOneCharacterAtATime()
    {
        CardNameValidator validator = new(_sql);

        validator.InsertAt('a', 0);
        CheckState(validator, "a", "Anointed Peacekeeper");

        validator.InsertAt('r', 1);
        CheckState(validator, "ar", "Archangel of Wrath");

        validator.InsertAt('g', 2);
        CheckState(validator, "arg", "Argivian Cavalier");
    }

    [TestMethod]
    public void TestInvalidCharacter()
    {
        CardNameValidator validator = new(_sql);

        validator.InsertAt('a', 0);
        CheckState(validator, "a", "Anointed Peacekeeper");

        validator.InsertAt('a', 1); // This doesn't match any card names, so Typed and Display shouldn't change
        CheckState(validator, "a", "Anointed Peacekeeper");

        validator.InsertAt('r', 1); // Now we have a valid character being typed
        CheckState(validator, "ar", "Archangel of Wrath");
    }

    [TestMethod]
    public void TestClearText()
    {
        CardNameValidator validator = new(_sql);

        validator.InsertAt('a', 0);
        CheckState(validator, "a", "Anointed Peacekeeper");

        validator.Text = "";
        CheckState(validator, "", "");
    }

    [TestMethod]
    public void TestUnknownFirstCharacter()
    {
        CardNameValidator validator = new(_sql);

        validator.InsertAt('x', 0);
        CheckState(validator, "", "");
    }

    [TestMethod]
    public void TestWordWithExtraCharacter()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "DefilerX";
        CheckState(validator, "Defiler", "Defiler of Faith");
    }

    [TestMethod]
    public void TestWordWithMultipleExtraCharacters()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "DefilerXYZ";
        CheckState(validator, "Defiler", "Defiler of Faith");
    }

    [TestMethod]
    public void TestWordWithExtraCharactersAndCorrection()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "Defiler of XYZ";
        CheckState(validator, "Defiler of ", "Defiler of Faith");

        validator.InsertAt('d', 11);
        CheckState(validator, "Defiler of d", "Defiler of Dreams");
    }

    [TestMethod]
    public void TestWordWithDelete()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "Defiler of D";
        CheckState(validator, "Defiler of D", "Defiler of Dreams");

        validator.Delete(11);
        CheckState(validator, "Defiler of ", "Defiler of Faith");
    }

    [TestMethod]
    public void TestInvalidDelete()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "";
        CheckState(validator, "", "");

        validator.Delete(-1);
        CheckState(validator, "", "");

        validator.Delete(1);
        CheckState(validator, "", "");
    }

    [TestMethod]
    public void TestCursor()
    {
        CardNameValidator validator = new(_sql);
        validator.Text = "Karn,";

        Assert.AreEqual(0, validator.Cursor(0));
        Assert.AreEqual(0, validator.Cursor(-1));
        Assert.AreEqual(5, validator.Cursor(5));
        Assert.AreEqual(5, validator.Cursor(18));

        Assert.AreEqual(5, validator.CursorEnd());

        Assert.AreEqual(2, validator.CursorLeft(3));
        Assert.AreEqual(0, validator.CursorLeft(0));
        Assert.AreEqual(0, validator.CursorLeft(-1));

        Assert.AreEqual(4, validator.CursorRight(3));
        Assert.AreEqual(1, validator.CursorRight(0));
        Assert.AreEqual(1, validator.CursorRight(-1));
        Assert.AreEqual(5, validator.CursorRight(15));

        Assert.AreEqual(0, validator.CursorStart());
    }

    [TestMethod]
    public void TestDefaults()
    {
        CardNameValidator validator = new(_sql);
        Assert.IsTrue(validator.IsValid);
        Assert.IsFalse(validator.Fixed);
    }

    private static void CheckState(CardNameValidator validator, string expectedTyped, string expectedDisplay)
    {
        ustring displayText = validator.DisplayText;
        ustring typed = validator.Text;

        Assert.AreEqual(expectedTyped, typed);
        Assert.AreEqual(expectedDisplay, displayText);
    }
}