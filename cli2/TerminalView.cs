using System.Data;
using Terminal.Gui;

namespace MTG_CLI
{
    class TerminalView
    {
        private Window _mainWindow;
        private MenuBar _menu;
        private StatusBar _statusBar;
        private FrameView _curSetFrame;
        private FrameView _curCardFrame;

        private Inventory _inventory;
        private string _curSetCode = "";
        public List<Scryfall.Set> SetList { get; set; } = new();

        public event Action<Scryfall.Set>? SelectedSetChanged;

        public TerminalView(Inventory inventory)
        {
            _inventory = inventory;

            Application.Init();

            _mainWindow = new Window("Magic: The Gathering -- Collection Inventory") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

            _menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_File", new MenuItem[] {
                    new MenuItem("E_xit", "", () => Application.RequestStop())
                }),
                new MenuBarItem("_Options", new MenuItem[] {
                    new MenuItem("Choose _Filters", "", ChooseFilters),
                    new MenuItem("Choose _Set", "", ChooseSet),
                }),
            });

            _statusBar = new StatusBar(new StatusItem[]{
                new StatusItem(Key.F, "Filters (~Shift-F~)", ChooseFilters ),
                new StatusItem(Key.S, "Choose Set (~Shift-S)", ChooseSet ),
            });

            _curSetFrame = new FrameView() { X = 0, Y = 0, Width = Dim.Percent(75), Height = Dim.Fill() };
            _curCardFrame = new FrameView() { X = Pos.Right(_curSetFrame), Y = Pos.Top(_curSetFrame) + 3, Width = Dim.Fill(), Height = Dim.Fill() };
        }

        private void ChooseFilters()
        {
            MessageBox.Query("Res", "You chose Filters", "ok");
        }

        private void ChooseSet()
        {
            var selectSetDlg = new Dialog("Select a Set");

            var setListView = new ListView(SetList ?? new List<Scryfall.Set>()) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            setListView.OpenSelectedItem += (args) =>
                {
                    Scryfall.Set selectedSet = (Scryfall.Set)args.Value;
                    SelectedSetChanged?.Invoke(selectedSet);
                    Application.RequestStop();
                };

            selectSetDlg.Add(setListView);

            Application.Run(selectSetDlg);
        }

        public void SetCurrentSet(Scryfall.Set curSet)
        {
            _curSetCode = curSet.Code;

            _curSetFrame.Title = curSet.Name;
            _curSetFrame.RemoveAll();
            _curSetFrame.Add(new Label("Loading cards...") { X = Pos.Center(), Y = 0 });

            if (!_mainWindow.Subviews.Contains(_curSetFrame))
                _mainWindow.Add(_curSetFrame);
        }

        public void SetCardList(List<Scryfall.Card> cardList)
        {
            _curSetFrame.RemoveAll();

            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("#"));
            table.Columns.Add(new DataColumn("Cnt"));
            table.Columns.Add(new DataColumn("Name"));
            table.Columns.Add(new DataColumn("Color"));
            table.Columns.Add(new DataColumn("Cost"));

            foreach (Scryfall.Card card in cardList)
            {
                DataRow row = table.NewRow();
                row["#"] = card.collector_number;
                row["Cnt"] = _inventory.GetTotalCardCount(_curSetCode, card.collector_number);
                row["Name"] = card.name;
                row["Color"] = String.Join("", card.color_identity?.ToArray() ?? new string[] { });
                row["Cost"] = card.mana_cost;
                table.Rows.Add(row);
            }

            var cardTable = new TableView(table) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            cardTable.FullRowSelect = true;
            cardTable.Style.AlwaysShowHeaders = true;
            cardTable.Style.ColumnStyles.Add(table.Columns["#"], new TableView.ColumnStyle() { MaxWidth = 5, MinWidth = 5 });
            cardTable.Style.ColumnStyles.Add(table.Columns["Cnt"], new TableView.ColumnStyle() { MaxWidth = 3, MinWidth = 3 });
            cardTable.Style.ColumnStyles.Add(table.Columns["Name"], new TableView.ColumnStyle() { MinWidth = 15 });
            cardTable.Style.ColumnStyles.Add(table.Columns["Color"], new TableView.ColumnStyle() { MaxWidth = 5, MinWidth = 5 });
            cardTable.SelectedCellChanged += (args) => UpdateCardFrame(cardList[args.NewRow]);

            _curSetFrame.Add(cardTable);
            cardTable.SetFocus();

            UpdateCardFrame(cardList[0]);
        }

        private void UpdateCardFrame(Scryfall.Card card)
        {
            _curCardFrame.RemoveAll();

            _curCardFrame.Title = $"{card.collector_number} - {card.name}";

            if (!_mainWindow.Subviews.Contains(_curCardFrame))
                _mainWindow.Add(_curCardFrame);
        }

        public void Start()
        {
            Application.Top.Add(_menu, _mainWindow, _statusBar);
            Application.Run();
        }
    }
}