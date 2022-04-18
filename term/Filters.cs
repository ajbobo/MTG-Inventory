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
        }

        public void ToggleFilter(Filter filter, bool enable)
        {
            if (filter == Filter.CNT_ALL) // Disable all Count filters
            {
                ToggleFilter(Filter.CNT_ZERO, false);
                ToggleFilter(Filter.CNT_ONE_PLUS, false);
                ToggleFilter(Filter.CNT_LESS_THAN_FOUR, false);
                ToggleFilter(Filter.CNT_FOUR_PLUS, false);
                return;
            }

            if (!enable && _filterList.Contains(filter))
                _filterList.Remove(filter);

            if (enable && !_filterList.Contains(filter))
                _filterList.Add(filter);
        }

        public bool MatchesFilter(Scryfall.Card card)
        {
            // TODO: This doesn't always work the right way when using Count filters -- Rebuild it based on the WebUI version
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

            // Third, if the card matches any color filter, keep it
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

            // Fourth, if the card's rarity is selected, keep it
            if (HasFilter(Filter.COMMON) && rarity.Equals("common"))
                return true;
            else if (HasFilter(Filter.UNCOMMON) && rarity.Equals("uncommon"))
                return true;
            else if (HasFilter(Filter.RARE) && rarity.Equals("rare"))
                return true;
            else if (HasFilter(Filter.MYTHIC) && rarity.Equals("mythic"))
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

        private void AddFilterCheckBoxes(View frame, Filter[] filters)
        {
            List<View> views = new();
            foreach (Filter filter in filters)
            {
                CheckBox checkBox = new(filter.ToString(), _filterSettings.HasFilter(filter)) { X = 0, Y = views.Count, Width = Dim.Fill() };
                checkBox.Toggled += (enabled) => _filterSettings.ToggleFilter(filter, !enabled);
                views.Add(checkBox);
            }
            frame.Add(views.ToArray());
        }

        public void EditFilters()
        {
            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop(); 

            Dialog dlg = new("Edit Filters", ok);

            FrameView colorFrame = new("Color") { X = 0, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] colors = { Filter.WHITE, Filter.BLUE, Filter.BLACK, Filter.RED, Filter.COLORLESS };
            AddFilterCheckBoxes(colorFrame, colors);


            FrameView rarityFrame = new("Rarity") { X = Pos.Right(colorFrame) + 1, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] rarities = { Filter.COMMON, Filter.UNCOMMON, Filter.RARE, Filter.MYTHIC };
            AddFilterCheckBoxes(rarityFrame, rarities);

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