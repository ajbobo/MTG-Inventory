using System.Text;

namespace MTG_CLI
{

    public abstract class Filter
    {
        public string DisplayName { protected set; get; } = "";

        override public string ToString()
        {
            return DisplayName;
        }
    }

    public class RarityFilter : Filter
    {
        public static RarityFilter COMMON { get; } = new() { DisplayName = "Common" };
        public static RarityFilter UNCOMMON { get; } = new() { DisplayName = "Uncommon" };
        public static RarityFilter RARE { get; } = new() { DisplayName = "Rare" };
        public static RarityFilter MYTHIC { get; } = new() { DisplayName = "Mythic" };

        public static Filter[] GetAllValues()
        {
            return new[] { COMMON, UNCOMMON, RARE, MYTHIC };
        }
    }

    public class CountFilter : Filter
    {
        public int Min { private set; get; }
        public int Max { private set; get; }

        public static CountFilter CNT_ZERO { get; } = new() { DisplayName = "0", Min = 0, Max = 0 };
        public static CountFilter CNT_ONE_PLUS { get; } = new() { DisplayName = "1+", Min = 1, Max = 1000 };
        public static CountFilter CNT_FOUR_PLUS { get; } = new() { DisplayName = "4+", Min = 4, Max = 1000 };
        public static CountFilter CNT_LESS_THAN_FOUR { get; } = new() { DisplayName = "<4", Min = 0, Max = 3 };
        public static CountFilter CNT_ALL { get; } = new() { DisplayName = "<all cards>" };

        public static Filter[] GetAllValues()
        {
            return new[] { CNT_ALL, CNT_ZERO, CNT_ONE_PLUS, CNT_LESS_THAN_FOUR, CNT_FOUR_PLUS };
        }
    }

    public class ColorFilter : Filter
    {
        public char Color { private set; get; }

        public static ColorFilter WHITE { get; } = new() { DisplayName = "White (W)", Color = 'W' };
        public static ColorFilter BLUE { get; } = new() { DisplayName = "Blue (U)", Color = 'U' };
        public static ColorFilter BLACK { get; } = new() { DisplayName = "Black (B)", Color = 'B' };
        public static ColorFilter RED { get; } = new() { DisplayName = "Red (R)", Color = 'R' };
        public static ColorFilter GREEEN { get; } = new() { DisplayName = "Green (G)", Color = 'G' };
        public static ColorFilter COLORLESS { get; } = new() { DisplayName = "Colorless", Color = 'X' };

        public static Filter[] GetAllValues()
        {
            return new[] { WHITE, BLUE, BLACK, RED, GREEEN, COLORLESS };
        }
    }


    public class FilterSettings
    {
        private List<Filter> _rarityList = new();
        private List<Filter> _countList = new();
        private List<Filter> _colorList = new();

        public void ToggleFilter(Filter filter, bool enable)
        {
            if (filter == CountFilter.CNT_ALL) // Disable all Count filters
            {
                ToggleFilter(CountFilter.CNT_ZERO, false);
                ToggleFilter(CountFilter.CNT_ONE_PLUS, false);
                ToggleFilter(CountFilter.CNT_LESS_THAN_FOUR, false);
                ToggleFilter(CountFilter.CNT_FOUR_PLUS, false);
                return;
            }

            List<Filter> filterList;
            if (filter.GetType() == typeof(RarityFilter))
                filterList = _rarityList;
            else if (filter.GetType() == typeof(CountFilter))
                filterList = _countList;
            else if (filter.GetType() == typeof(ColorFilter))
                filterList = _colorList;
            else
                return; // Weird state - do nothing

            if (!enable && filterList.Contains(filter))
                filterList.Remove(filter);

            if (enable && !filterList.Contains(filter))
                filterList.Add(filter);
        }

        public bool HasFilter(Filter filter)
        {
            return _rarityList.Contains(filter) || _countList.Contains(filter) || _colorList.Contains(filter);
        }

        public int GetMinCount()
        {
            return (_countList.Count > 0 ? ((CountFilter)_countList[0]).Min : 0);
        }

        public int GetMaxCount()
        {
            return (_countList.Count > 0 ? ((CountFilter)_countList[0]).Max : 1000);
        }

        public string[] GetRarities()
        {
            Filter[] allRarities = RarityFilter.GetAllValues();
            string[] res = new string[allRarities.Count()];

            List<Filter> theList = (_rarityList.Count != 0 ? _rarityList : new(allRarities));

            int cnt = theList.Count;
            for (int x = 0; x < res.Count(); x++)
                res[x] = (cnt > x ? (theList[x]?.ToString() ?? "na") : "na");

            return res;
        }

        public string GetColors()
        {
            StringBuilder builder = new();
            foreach (Filter filter in _colorList)
            {
                builder.Append(((ColorFilter)filter).Color);
            }
            return builder.ToString();
        }
    }
}