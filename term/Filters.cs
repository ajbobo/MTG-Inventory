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
        CNT_LESS_THAN_FOUR,
        CNT_ALL
    };

    class FilterSettings
    {
        private List<Filter> _filterList = new();
        private Inventory _inventory;

        public FilterSettings(Inventory inventory)
        {
            _inventory = inventory;
            _filterList.Add(Filter.CNT_ALL);
        }

        public void ToggleFilter(Filter filter, bool enable)
        {
            if (!enable && _filterList.Contains(filter))
                _filterList.Remove(filter);

            if (enable && !_filterList.Contains(filter))
                _filterList.Add(filter);

            if (filter == Filter.CNT_ALL) // Disable all Count filters
            {
                ToggleFilter(Filter.CNT_ZERO, false);
                ToggleFilter(Filter.CNT_ONE_PLUS, false);
                ToggleFilter(Filter.CNT_LESS_THAN_FOUR, false);
                ToggleFilter(Filter.CNT_FOUR_PLUS, false);
                return;
            }
        }

        public bool MatchesFilter(Scryfall.Card card)
        {
            // A card only has to match one of the selected filters
            List<string> colorIdentity = card.ColorIdentity;
            string rarity = card.Rarity.ToLower();
            int count = _inventory.getCardCount(card);
            
            // First, are there any filters to apply?
            if (_filterList.Count == 0)
                return true;

            // Second, if the card has the wrong count, filter it out
            if (HasFilter(Filter.CNT_ZERO) && count != 0)
                return false;
            else if (HasFilter(Filter.CNT_ONE_PLUS) && count < 1)
                return false;
            else if (HasFilter(Filter.CNT_FOUR_PLUS) && count < 4)
                return false;
            else if (HasFilter(Filter.CNT_LESS_THAN_FOUR) && count >= 4)
                return false;

            // Third, if the card does not match a selected filter, filter it out
            if (HasFilter(Filter.WHITE) && !colorIdentity.Contains("W"))
                return false;
            else if (HasFilter(Filter.BLUE) && !colorIdentity.Contains("U"))
                return false;
            else if (HasFilter(Filter.BLACK) && !colorIdentity.Contains("B"))
                return false;
            else if (HasFilter(Filter.RED) && !colorIdentity.Contains("R"))
                return false;
            else if (HasFilter(Filter.GREEEN) && !colorIdentity.Contains("G"))
                return false;
            else if (HasFilter(Filter.COLORLESS) && colorIdentity.Count != 0)
                return false;
            else if (HasFilter(Filter.COMMON) && !rarity.Equals("common"))
                return false;
            else if (HasFilter(Filter.UNCOMMON) && !rarity.Equals("uncommon"))
                return false;
            else if (HasFilter(Filter.RARE) && !rarity.Equals("rare"))
                return false;
            else if (HasFilter(Filter.MYTHIC) && !rarity.Equals("mythic"))
                return false;
            
            return true;
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

            FrameView colorFrame = new("Color") { X = 0, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] colors = { Filter.WHITE, Filter.BLUE, Filter.BLACK, Filter.RED, Filter.COLORLESS };
            List<View> views = new();
            foreach (Filter color in colors)
            {
                CheckBox checkBox = new(color.ToString(), _filterSettings.HasFilter(color)) { X = 0, Y = views.Count, Width = Dim.Fill() };
                checkBox.Toggled += (enabled) => _filterSettings.ToggleFilter(color, !enabled);
                views.Add(checkBox);
            }
            colorFrame.Add(views.ToArray());

            FrameView rarityFrame = new("Rarity") { X = Pos.Right(colorFrame) + 1, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] rarities = { Filter.COMMON, Filter.UNCOMMON, Filter.RARE, Filter.MYTHIC };
            views.Clear();
            foreach (Filter rarity in rarities)
            {
                CheckBox checkBox = new(rarity.ToString(), _filterSettings.HasFilter(rarity)) { X = 0, Y = views.Count, Width = Dim.Fill() };
                checkBox.Toggled += (enabled) => _filterSettings.ToggleFilter(rarity, !enabled);
                views.Add(checkBox);
            }
            rarityFrame.Add(views.ToArray());

            FrameView countFrame = new("Count") { X = Pos.Right(rarityFrame) + 1, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] filters = { Filter.CNT_ALL, Filter.CNT_ZERO, Filter.CNT_ONE_PLUS, Filter.CNT_LESS_THAN_FOUR, Filter.CNT_FOUR_PLUS };
            NStack.ustring[] labels = { "<any count>", "0", "1+", "<4", "4+" };
            int selected = 0;
            for (int x = 1; x < filters.Length; x++)
            {
                Filter filter = filters[x];
                if (_filterSettings.HasFilter(filter))
                    selected = x;
            }
            RadioGroup group = new(labels, selected);
            group.SelectedItemChanged += (args) =>
            {
                _filterSettings.ToggleFilter(filters[args.PreviousSelectedItem], false);
                _filterSettings.ToggleFilter(filters[args.SelectedItem], true);
            };
            countFrame.Add(group);

            dlg.Add(colorFrame, rarityFrame, countFrame);

            dlg.Closed += (toplevel) => { OnClose?.Invoke(); };

            Application.Run(dlg);
        }
    }
}