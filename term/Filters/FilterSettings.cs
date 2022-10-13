using System.Text;

namespace MTG_CLI
{
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