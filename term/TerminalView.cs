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
        private TableView _cardTable;
        private FrameView _curCardFrame;

        private Inventory _inventory;

        public List<Scryfall.Set> SetList { get; set; } = new();
        
        public event Action<Scryfall.Set>? SelectedSetChanged;

        public TerminalView(Inventory inventory)
        {
            _inventory = inventory;

            Application.Init();

            _mainWindow = new("Magic: The Gathering -- Collection Inventory") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

            _menu = new(new MenuBarItem[] {
                new MenuBarItem("_File", new MenuItem[] {
                    new MenuItem("E_xit", "", () => Application.RequestStop())
                }),
                new MenuBarItem("_Options", new MenuItem[] {
                    new MenuItem("Choose _Filters", "", ChooseFilters),
                    new MenuItem("Choose _Set", "", ChooseSet),
                }),
            });

            _statusBar = new(new StatusItem[]{
                new StatusItem(Key.F, "Filters (~Shift-F~)", ChooseFilters ),
                new StatusItem(Key.S, "Choose Set (~Shift-S~)", ChooseSet ),
                new StatusItem(Key.G, "Goto Card (~Shift-G~)", FindCard ),
            });

            _curSetFrame = new() { X = 0, Y = 0, Width = Dim.Percent(75), Height = Dim.Fill() };
            _cardTable = new() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _curCardFrame = new() { X = Pos.Right(_curSetFrame), Y = Pos.Top(_curSetFrame) + 3, Width = Dim.Fill(), Height = Dim.Fill() };
        }

        private void FindCard()
        {
            MessageBox.Query("Res", "You get to type a card name", "ok");
        }

        private void ChooseFilters()
        {
            MessageBox.Query("Res", "You chose Filters", "ok");
        }

        private void ChooseSet()
        {
            Dialog selectSetDlg = new("Select a Set");

            ListView setListView = new(SetList ?? new List<Scryfall.Set>()) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
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
            _curSetFrame.Title = curSet.Name;
            _curSetFrame.RemoveAll();
            _curSetFrame.Add(new Label("Loading cards...") { X = Pos.Center(), Y = 0 });

            if (!_mainWindow.Subviews.Contains(_curSetFrame))
                _mainWindow.Add(_curSetFrame);
        }

        public void SetCardList(List<Scryfall.Card> cardList)
        {
            _curSetFrame.RemoveAll();

            DataTable table = new();
            table.Columns.Add(new DataColumn("#"));
            table.Columns.Add(new DataColumn("Cnt"));
            table.Columns.Add(new DataColumn("Rarity"));
            table.Columns.Add(new DataColumn("Name") { DataType = typeof(Scryfall.Card) }); // Store the actual card reference here, so it's easy to find later
            table.Columns.Add(new DataColumn("Color"));
            table.Columns.Add(new DataColumn("Cost"));

            foreach (Scryfall.Card card in cardList)
            {
                DataRow row = table.NewRow();
                row["#"] = card.CollectorNumber;
                row["Cnt"] = _inventory.GetCardCountDisplay(card);
                row["Rarity"] = card.Rarity.ToUpper()[0];
                row["Name"] = card; 
                row["Color"] = String.Join("", card.ColorIdentity?.ToArray() ?? new string[] { });
                row["Cost"] = card.ManaCost;
                table.Rows.Add(row);
            }

            _cardTable = new(table) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _cardTable.FullRowSelect = true;
            _cardTable.Style.AlwaysShowHeaders = true;
            _cardTable.Style.ExpandLastColumn = false;
            _cardTable.Style.ColumnStyles.Add(table.Columns["#"], new() { MaxWidth = 5, MinWidth = 5 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Cnt"], new() { MaxWidth = 3, MinWidth = 3 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Rarity"], new() { MinWidth = 2, MaxWidth = 2 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Name"], new() { MinWidth = 15 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Color"], new() { MaxWidth = 5, MinWidth = 5 });
            _cardTable.SelectedCellChanged += (args) => UpdateCardFrame(cardList[args.NewRow]);

            _curSetFrame.Add(_cardTable);
            _cardTable.SetFocus();
            _cardTable.CellActivated += (args) =>
            {
                DataTable table = args.Table;
                EditCardDialog dlg = new(_inventory);
                dlg.DataChanged += () => {
                    MessageBox.Query("Dirty Data", "Database updates here", "OK");
                    UpdateCardTableRow(); 
                };
                dlg.EditCard((Scryfall.Card)table.Rows[args.Row]["Name"]);
            };

            UpdateCardFrame(cardList[0]);
        }

        private void UpdateCardTableRow()
        {
            var row = _cardTable.Table.Rows[_cardTable.SelectedRow];
            Scryfall.Card selectedCard = (Scryfall.Card)row["Name"];
            row["Cnt"] = _inventory.GetCardCountDisplay(selectedCard);
            UpdateCardFrame(selectedCard);
        }

        // To color the Rarity column, assign this as the ColorGetter to the column's ColumnStyle
        ColorScheme GetRarityColor(TableView.CellColorGetterArgs args)
        {
            string rarity = (string)args.CellValue;
            Color newColor = rarity switch
            {
                "C" => Color.White,
                "U" => Color.BrightBlue,
                "R" => Color.Red,
                "M" => Color.Magenta,
                _ => Color.Black
            };
            if (newColor == Color.Black)
                return args.RowScheme;

            ColorScheme scheme = new();
            scheme.Normal = new Terminal.Gui.Attribute(newColor, args.RowScheme.Normal.Background);
            scheme.HotFocus = new Terminal.Gui.Attribute(newColor, args.RowScheme.HotFocus.Background);
            scheme.HotNormal = new Terminal.Gui.Attribute(newColor, args.RowScheme.HotNormal.Background);
            return scheme;
        }

        private void UpdateCardFrame(Scryfall.Card card)
        {
            MTG_Card? curCard = _inventory.GetCard(card);
            if (curCard != null)
            {
                UpdateCardFrame(curCard);
            }
            else
            {
                _curCardFrame.RemoveAll();

                _curCardFrame.Title = $"{card.CollectorNumber} - {card.Name}";

                if (!_mainWindow.Subviews.Contains(_curCardFrame))
                    _mainWindow.Add(_curCardFrame);
            }
        }

        private void UpdateCardFrame(MTG_Card card)
        {
            _curCardFrame.RemoveAll();

            _curCardFrame.Title = $"{card.CollectorNumber} - {card.Name}";

            card.SortCTCs();
            for (int x = 0; x < card.Counts.Count; x++)
            {
                _curCardFrame.Add(new Label(card.Counts[x].ToString()) { X = 0, Y = x, Width = Dim.Fill() });
            }

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