

using System.Text.RegularExpressions;

namespace mtg_api;

public class Filters
{
        internal static IEnumerable<CardData> FilterByNumber(string collectorNumber, IEnumerable<CardData> list)
    {
        return from card in list
               where collectorNumber.Length == 0 || card.Card!.CollectorNumber.Equals(collectorNumber)
               select card;
    }

    internal static IEnumerable<CardData> FilterByColor(string colorFilter, IEnumerable<CardData> list)
    {
        return from card in list
               where colorFilter.Length == 0 || InsideString(card.Card!.ColorIdentity, colorFilter.ToUpper())
               select card;
    }

    internal static IEnumerable<CardData> FilterByPrice(string priceFilter, IEnumerable<CardData> list)
    {
        GetComparison(priceFilter, out string priceOp, out decimal priceNum);
        var priceList =
            from card in list
            where (priceOp.Equals(">=") && (card.Card!.Price >= priceNum || card.Card!.PriceFoil >= priceNum)) ||
                (priceOp.Equals("<=") && (card.Card!.Price <= priceNum || card.Card!.PriceFoil <= priceNum)) ||
                (priceOp.Equals(">") && (card.Card!.Price > priceNum || card.Card!.PriceFoil > priceNum)) ||
                (priceOp.Equals("<") && (card.Card!.Price < priceNum || card.Card!.PriceFoil < priceNum)) ||
                (priceOp.Equals("=") && (card.Card!.Price == priceNum || card.Card!.PriceFoil == priceNum))
            select card;
        return priceList;
    }

    internal static IEnumerable<CardData> FilterByCount(string countFilter, IEnumerable<CardData> list)
    {
        string op;
        decimal num;
        GetComparison(countFilter, out op, out num);
        var countList =
            from card in list
            where (op.Equals(">=") && card.TotalCount >= num) ||
                (op.Equals("<=") && card.TotalCount <= num) ||
                (op.Equals(">") && card.TotalCount > num) ||
                (op.Equals("<") && card.TotalCount < num) ||
                (op.Equals("=") && card.TotalCount == num)
            select card;
        return countList;
    }

    internal static IEnumerable<CardData> FilterByRarity(string rarityFilter, IEnumerable<CardData> list)
    {
        return from card in list
               where rarityFilter.Length == 0 || rarityFilter.ToUpper().Contains(card.Card!.Rarity[..1].ToUpper())
               select card;
    }

    internal static bool InsideString(string search, string target)
    {
        foreach (char curChar in search)
        {
            if (target.Contains(curChar))
                return true;
        }

        return false;
    }

    internal static void GetComparison(string countFilter, out string op, out decimal num)
    {
        op = ">=";
        num = 0;
        if (countFilter.Length == 0)
            return;

        Regex regex = new Regex(@"(?<op><=|>=|=|<|>)(\s*)(?<num>\d+)");
        Match match = regex.Match(countFilter); // If there is more than one match, only use the first one
        if (match.Success)
        {
            GroupCollection groups = match.Groups;
            op = groups["op"].Value;
            num = decimal.Parse(groups["num"].Value);
        }
    }
}