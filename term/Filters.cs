using System;
using Terminal.Gui;

namespace MTG_CLI
{
    enum Filter
    {
        WHITE,
        BLUE,
        BLACK,
        RED,
        GREEEN,
        COLORLESS,

        COMMON,
        UNCOMMON,
        RARE,
        MYTHIC,

        CNT_ZERO,
        CNT_ONE_PLUS,
        CNT_FOUR_PLUS,
        CNT_LESS_THAN_FOUR
    };

    class FilterSettings
    {
        private List<Filter> _filterList = new();
        private Inventory _inventory;

        public FilterSettings(Inventory inventory)
        {
            _inventory = inventory;
        }

        public void ToggleFilter(Filter filter, bool enable)
        {
            if (!enable && _filterList.Contains(filter))
                _filterList.Remove(filter);
            
            if (enable && !_filterList.Contains(filter))
                _filterList.Add(filter);
        }

        public bool MatchesFilter(Scryfall.Card card)
        {
            // A card only has to match one of the selected filters
            List<string> colorIdentity = card.ColorIdentity;
            string rarity = card.Rarity.ToLower();
            int count = _inventory.getCardCount(card);
            if (_filterList.Count == 0)
                return true;
            if (HasFilter(Filter.WHITE) && colorIdentity.Contains("W"))
                return true;
            else if (HasFilter(Filter.BLUE) && colorIdentity.Contains("U"))
                return true;
            else if (HasFilter(Filter.BLACK) && colorIdentity.Contains("B"))
                return true;
            else if (HasFilter(Filter.RED) && colorIdentity.Contains("R"))
                return true;
            else if (HasFilter(Filter.GREEEN) && colorIdentity.Contains("G"))
                return true;
            else if (HasFilter(Filter.COLORLESS) && colorIdentity.Count == 0)
                return true;
            else if (HasFilter(Filter.COMMON) && rarity.Equals("common"))
                return true;
            else if (HasFilter(Filter.UNCOMMON) && rarity.Equals("uncommon"))
                return true;
            else if (HasFilter(Filter.RARE) && rarity.Equals("rare"))
                return true;
            else if (HasFilter(Filter.MYTHIC) && rarity.Equals("mythic"))
                return true;
            else if (HasFilter(Filter.CNT_ZERO) && count == 0)
                return true;
            else if (HasFilter(Filter.CNT_ONE_PLUS) && count >= 1)
                return true;
            else if (HasFilter(Filter.CNT_FOUR_PLUS) && count >= 4)
                return true;
            else if (HasFilter(Filter.CNT_LESS_THAN_FOUR) && count < 4)
                return true;

            return false;
        }

        public bool HasFilter(Filter filter)
        {
            return _filterList.Contains(filter);
        }
    }

    class EditFiltersDialog 
    {
        private FilterSettings _filterSettings;

        public event Action? OnClose;

        public EditFiltersDialog(FilterSettings filterSettings)
        {
            this._filterSettings = filterSettings;
        }

        public void EditFilters()
        {
            Button ok = new("OK");
            ok.Clicked += () => { Application.RequestStop(); };

            Dialog dlg = new("Edit Filters", ok);
            
            FrameView colorFrame = new() { X = 0, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() };
            Filter[] colors = { Filter.WHITE, Filter.BLUE, Filter.BLACK, Filter.RED, Filter.COLORLESS };
            List<View> views = new();
            foreach (Filter color in colors)
            {
                CheckBox checkBox = new(color.ToString(), _filterSettings.HasFilter(color)) { X = 0, Y = views.Count, Width = Dim.Fill() };
                checkBox.Toggled += (enabled) => _filterSettings.ToggleFilter(color, !enabled);
                views.Add(checkBox);
            }
            colorFrame.Add(views.ToArray());

            dlg.Add(colorFrame);

            dlg.Closed += (toplevel) => { OnClose?.Invoke(); };

            Application.Run(dlg);
        }
    }
}