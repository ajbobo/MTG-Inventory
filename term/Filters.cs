using System;
using Terminal.Gui;

namespace MTG_CLI
{

    abstract class Filter
    {
        public string DisplayName { protected set; get; } = "";
    }

    class RarityFilter : Filter
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

    class CountFilter : Filter
    {
        public static CountFilter CNT_ZERO { get; } = new() { DisplayName = "0" };
        public static CountFilter CNT_ONE_PLUS { get; } = new() { DisplayName = "1+" };
        public static CountFilter CNT_FOUR_PLUS { get; } = new() { DisplayName = "4+" };
        public static CountFilter CNT_LESS_THAN_FOUR { get; } = new() { DisplayName = "<4" };
        public static CountFilter CNT_ALL { get; } = new() { DisplayName = "<all cards>" };

        public static Filter[] GetAllValues()
        {
            return new[] { CNT_ALL, CNT_ZERO, CNT_ONE_PLUS, CNT_LESS_THAN_FOUR, CNT_FOUR_PLUS };
        }
    }

    class ColorFilter : Filter
    {
        public static ColorFilter WHITE { get; } = new() { DisplayName = "White (W)" };
        public static ColorFilter BLUE { get; } = new() { DisplayName = "Blue (U)" };
        public static ColorFilter BLACK { get; } = new() { DisplayName = "Black (B)" };
        public static ColorFilter RED { get; } = new() { DisplayName = "Red (R)" };
        public static ColorFilter GREEEN { get; } = new() { DisplayName = "Green (G)" };
        public static ColorFilter COLORLESS { get; } = new() { DisplayName = "Colorless" };

        public static Filter[] GetAllValues()
        {
            return new[] { WHITE, BLUE, BLACK, RED, GREEEN, COLORLESS };
        }
    }


    class FilterSettings
    {
        private List<Filter> _rarityList = new();
        private List<Filter> _countList = new();
        private List<Filter> _colorList = new();
        private Inventory _inventory;

        public FilterSettings(Inventory inventory)
        {
            _inventory = inventory;
        }

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

        private bool MatchesRarity(Scryfall.Card card)
        {
            if (_rarityList.Count == 0)
                return true;

            string rarity = card.Rarity.ToLower();
            if ((rarity.Equals("common") && _rarityList.Contains(RarityFilter.COMMON)) ||
                (rarity.Equals("uncommon") && _rarityList.Contains(RarityFilter.UNCOMMON)) ||
                (rarity.Equals("rare") && _rarityList.Contains(RarityFilter.RARE)) ||
                (rarity.Equals("mythic") && _rarityList.Contains(RarityFilter.MYTHIC)))
                return true;

            return false;
        }

        private bool MatchesCount(Scryfall.Card card)
        {
            if (_countList.Count == 0)
                return true;

            int count = _inventory.getCardCount(card);
            if ((_countList.Contains(CountFilter.CNT_ZERO) && count == 0) ||
                (_countList.Contains(CountFilter.CNT_ONE_PLUS) && count >= 1) ||
                (_countList.Contains(CountFilter.CNT_LESS_THAN_FOUR) && count < 4) ||
                (_countList.Contains(CountFilter.CNT_FOUR_PLUS) && count >= 4))
                return true;

            return false;
        }

        private bool MatchesColor(Scryfall.Card card)
        {
            if (_colorList.Count == 0)
                return true;

            List<string> colorIdentity = card.ColorIdentity;

            if (colorIdentity.Count == 0 && HasFilter(ColorFilter.COLORLESS))
                return true;

            bool include = false;
            foreach (string color in colorIdentity)
            {
                include |= color switch
                {
                    "W" => HasFilter(ColorFilter.WHITE),
                    "U" => HasFilter(ColorFilter.BLUE),
                    "B" => HasFilter(ColorFilter.BLACK),
                    "R" => HasFilter(ColorFilter.RED),
                    "G" => HasFilter(ColorFilter.GREEEN),
                    _ => false,
                };
            }

            return include;
        }

        public bool MatchesFilter(Scryfall.Card card)
        {
            List<string> colorIdentity = card.ColorIdentity;
            string rarity = card.Rarity.ToLower();
            int count = _inventory.getCardCount(card);

            if (!MatchesRarity(card))
                return false;

            if (!MatchesCount(card))
                return false;

            if (!MatchesColor(card))
                return false;

            return true;
        }

        public bool HasFilter(Filter filter)
        {
            return _rarityList.Contains(filter) || _countList.Contains(filter) || _colorList.Contains(filter);
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
                CheckBox checkBox = new(filter.DisplayName, _filterSettings.HasFilter(filter)) { X = 0, Y = views.Count, Width = Dim.Fill() };
                checkBox.Toggled += (enabled) => _filterSettings.ToggleFilter(filter, !enabled);
                views.Add(checkBox);
            }
            frame.Add(views.ToArray());
        }

        public void EditFilters()
        {
            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop();

            Dialog dlg = new("Edit Filters", ok) { Width = 50, Height = 11 };

            FrameView colorFrame = new("Color") { X = 0, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] colors = ColorFilter.GetAllValues();
            AddFilterCheckBoxes(colorFrame, colors);


            FrameView rarityFrame = new("Rarity") { X = Pos.Right(colorFrame) + 1, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] rarities = RarityFilter.GetAllValues();
            AddFilterCheckBoxes(rarityFrame, rarities);

            FrameView countFrame = new("Count") { X = Pos.Right(rarityFrame) + 1, Y = 0, Width = Dim.Percent(33.3f), Height = Dim.Fill() - 1 };
            Filter[] filters = CountFilter.GetAllValues();
            List<NStack.ustring> names = new();
            foreach (Filter filter in filters)
                names.Add(filter.DisplayName);
            int selected = 0;
            for (int x = 1; x < filters.Length; x++)
            {
                Filter filter = filters[x];
                if (_filterSettings.HasFilter(filter))
                    selected = x;
            }
            RadioGroup group = new(names.ToArray(), selected);
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