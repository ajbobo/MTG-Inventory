using webapp.Models;

namespace webapp.Utils;

public class SymbolHelper
{
    private static List<MTG_Symbol> _symbolList = new();


    public static void SetSymbolList(List<MTG_Symbol> list)
    {
        _symbolList = list;
    }

    public static string SymbolToUrl(string symbol)
    {
        foreach (MTG_Symbol curSymbol in _symbolList)
        {
            if (curSymbol.Text.Equals(symbol))
                return curSymbol.URL;
        }
        return "";
    }

    public static List<string> CastingCostToUrls(string cost)
    {
        List<string> res = new();
        string[] splitList = cost.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries); // Symbols are "{x}"
        foreach (string curSymbol in splitList)
        {
            res.Add(SymbolToUrl($"{{{curSymbol}}}"));
        }
        return res;
    }

    public static List<string> ColorIdentityToUrls(string color)
    {
        List<string> res = new();
        foreach (char curChar in color)
        {
            res.Add(SymbolToUrl($"{{{curChar}}}"));
        }
        return res;
    }

    public static List<string> CTCsToUrls(List<CardTypeCount> CTCs)
    {
        List<string> res = new();
        if (CTCs == null)
            return res;

        foreach (CardTypeCount ctc in CTCs)
        {
            string type = ctc.CardType.ToLower();
            if (type.Equals("standard"))
                continue;
            if (type.Contains("foil"))
                res.Add("foil");
            if (!ctc.CardType.ToLower().Equals("foil"))
                res.Add("other");
        }
        return res;
    }
}