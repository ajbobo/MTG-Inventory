using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage]
    class EditFiltersDialog
    {
        private FilterSettings _filterSettings;

        public event Action? OnClose;

        public EditFiltersDialog(FilterSettings filterSettings)
        {
            _filterSettings = filterSettings;
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