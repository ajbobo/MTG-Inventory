using System.Data;
using Terminal.Gui;
using Terminal.Gui.Views;
using Microsoft.Data.Sqlite;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    class TerminalView
    {
        private Toplevel _top;
        private MenuBar _menu;
        private StatusBar _statusBar;
        private StatusItem _autoStatus;
        private FrameView _curSetFrame;
        private FrameView _curStatsFrame;
        private TableView _cardTable;
        private FrameView _curCardFrame;
        private FindCardDialog _findCardDlg;
        private EditFiltersDialog _editFilters;

        private SQLManager _sql;

        private List<Scryfall.Card> _cardList;
        private int _collectedCount;
        private Inventory _inventory;
        private FilterSettings _filterSettings;
        private bool _autoFind = true;

        public event Action<string>? SelectedSetChanged;

        public TerminalView(Inventory inventory, SQLManager sql)
        {
            _sql = sql;
            _inventory = inventory;
            _filterSettings = new(_inventory);
            _cardList = new();

            Application.Init();

            _top = Application.Top;
            _top.ColorScheme = Colors.ColorSchemes["Base"];

            Label lbl = new("Press Ctrl-S to choose a Set") { X = Pos.Center(), Y = Pos.Center() };
            _top.Add(lbl);

            Label cacheStatus = new($"Using Cache: {_inventory.UsingCache}") { X = Pos.Center(), Y = Pos.Bottom(lbl) + 2 };
            _top.Add(cacheStatus);

            _menu = new(new MenuBarItem[] {
                new MenuBarItem("_File", new MenuItem[] {
                    new MenuItem("E_xit", "", () => Application.RequestStop())
                }),
                new MenuBarItem("_Options", new MenuItem[] {
                    new MenuItem("Choose _Set", "", ChooseSet),
                    new MenuItem("_Edit Filters", "", ChooseFilters),
                    new MenuItem("_Find a Card", "", FindCard),
                }),
            });

            _autoStatus = new(Key.A | Key.CtrlMask, "~Ctrl-A~ Auto On", ToggleAutoAdvance);

            _statusBar = new(new StatusItem[]{
                new StatusItem(Key.S | Key.CtrlMask, "~Ctrl-S~ Choose Set", ChooseSet ),
                new StatusItem(Key.E | Key.CtrlMask, "~Ctrl-E~ Edit Filters", ChooseFilters ),
                new StatusItem(Key.F | Key.CtrlMask, "~Ctrl-F~ Find Card", FindCard ),
                new StatusItem(Key.N | Key.CtrlMask, "~Ctrl-N~ Find Next", FindNext ),
                _autoStatus,
            });

            _curSetFrame = new() { X = 0, Y = 1, Width = Dim.Percent(75), Height = Dim.Fill() - 1 };
            _curStatsFrame = new() { X = Pos.Right(_curSetFrame), Y = Pos.Top(_curSetFrame), Width = Dim.Fill(), Height = 5 };
            _curCardFrame = new() { X = Pos.Right(_curSetFrame), Y = Pos.Bottom(_curStatsFrame), Width = Dim.Fill(), Height = Dim.Fill() - 1 };
            _cardTable = new() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _findCardDlg = new();
            _editFilters = new(_filterSettings);
            // _editFilters.OnClose += () => { SetCardList(_cardList); };
        }

        private void ToggleAutoAdvance()
        {
            _autoFind = !_autoFind;
            _autoStatus.Title = string.Format("~Ctrl-A~ Auto {0}", (_autoFind ? "On" : "Off"));
            _statusBar.SetNeedsDisplay();
        }

        private void FindNext()
        {
            if (_cardTable == null || _cardTable.Table == null)
                return;

            DataRow row = _cardTable.Table.Rows[_cardTable.SelectedRow];
            Scryfall.Card selectedCard = (Scryfall.Card)row["Name"];
            string selectedName = selectedCard.Name;

            // Starting after the current row, find the next one with the same Name
            int index = _cardTable.SelectedRow + 1;
            while (true) // Loop until a card is found, wrap back around if needed, stops when it finds the same card again
            {
                DataRow nextRow = _cardTable.Table.Rows[index];
                Scryfall.Card nextCard = (Scryfall.Card)nextRow["Name"];
                if (nextCard.Name.Equals(selectedName))
                {
                    _cardTable.SelectedRow = index;
                    _cardTable.EnsureSelectedCellIsVisible();
                    UpdateCardFrame(nextCard);
                    return;
                }

                // Advance to the next row, wrap around if needed
                index++;
                if (index >= _cardTable.Table.Rows.Count)
                    index = 0;
            }
        }

        private void FindCard()
        {
            _findCardDlg.FindCard();
        }

        private void FoundCard(Scryfall.Card card)
        {
            DataRow? cardRow = _cardTable.Table.Rows.Find(card.CollectorNumber);
            _cardTable.SelectedRow = _cardTable.Table.Rows.IndexOf(cardRow);
            _cardTable.EnsureSelectedCellIsVisible();

            UpdateCardFrame(card);
        }

        private void ChooseFilters()
        {
            _editFilters.EditFilters();
        }

        private List<T> CreateList<T>(params T[] elements)
        {
            return new List<T>(elements);
        }

        private void ChooseSet()
        {
            Dialog selectSetDlg = new("Select a Set") { Width = 45 };

            List<string> SetList = new();
            SqliteDataReader? reader = _sql.Query(SQLManager.InternalQuery.GET_ALL_SETS).Read();
            while (reader != null && reader.Read())
            {
                string name = reader.GetFieldValue<string>("Name");
                string code = string.Format("({0})",reader.GetFieldValue<string>("SetCode")); // Format it here as "(code)" so that it can be spaced nicely later
                SetList.Add(string.Format("{0,-7} {1}", code, name));
            }

            ListView setListView = new(SetList) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            setListView.OpenSelectedItem += (args) =>
                {
                    string selectedItem = (string)args.Value;
                    string setCode = selectedItem.Substring(1, selectedItem.IndexOf(')') - 1); // The item is "(code) Name" - get the value between ( and )
                    SelectedSetChanged?.Invoke(setCode);
                    Application.RequestStop();
                };

            selectSetDlg.Add(setListView);

            Application.Run(selectSetDlg);
        }

        public void SetCurrentSet(string setCode)
        {
            string setName = _sql.Query(GET_SET_NAME).WithParam("@SetCode", setCode).ExecuteScalar<string>() ?? "<unknown>";

            _curSetFrame.Title = setName;
            _curSetFrame.RemoveAll();
            _curSetFrame.Add(new Label("Loading cards...") { X = Pos.Center(), Y = 0 });

            if (!_top.Subviews.Contains(_curSetFrame))
                _top.Add(_curSetFrame);
        }

        public void SetCardList(string curSetCode)
        {
            _collectedCount = 0;

            // TODO: This should be the filtered list of cards
            SqliteDataReader? reader = _sql.Query(GET_SET_CARDS).WithParam("@SetCode", curSetCode).Read();

            _findCardDlg = new(new()); 
            _findCardDlg.CardSelected += FoundCard;

            _curSetFrame.RemoveAll();

            DataTable table = new();
            table.PrimaryKey = new[] { table.Columns.Add("#") };
            table.Columns.Add("Cnt");
            table.Columns.Add("Rarity");
            table.Columns.Add("Name"); //, typeof(Scryfall.Card)); // Store the actual card reference here, so it's easy to find later
            table.Columns.Add("Color");
            table.Columns.Add("Cost");

            while (reader != null && reader.Read())
            {
                // if (!_filterSettings.MatchesFilter(card))
                    // continue;

                DataRow row = table.NewRow();
                row["#"] = reader.GetFieldValue<int>("Collector_Number");
                row["Cnt"] = "tbd";
                row["Rarity"] = reader.GetFieldValue<string>("Rarity").ToUpper()[0];
                row["Name"] = reader.GetFieldValue<string>("Name");
                row["Color"] = reader.GetFieldValue<string>("ColorIdentity");
                row["Cost"] = reader.GetFieldValue<string>("ManaCost");
                table.Rows.Add(row);

                // if (_inventory.GetCardCount(card) > 0)
                    // _collectedCount++;
            }

            _cardTable = new(table) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _cardTable.FullRowSelect = true;
            _cardTable.MultiSelect = false;
            _cardTable.Style.AlwaysShowHeaders = true;
            _cardTable.Style.ExpandLastColumn = false;
            _cardTable.Style.ColumnStyles.Add(table.Columns["#"], new() { MaxWidth = 5, MinWidth = 5 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Cnt"], new() { MaxWidth = 3, MinWidth = 3 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Rarity"], new() { MinWidth = 2, MaxWidth = 2 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Name"], new() { MinWidth = 15 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Color"], new() { MaxWidth = 5, MinWidth = 5 });
            // _cardTable.SelectedCellChanged += (args) => UpdateCardFrame((Scryfall.Card)table.Rows[args.NewRow]["Name"]);

            _curSetFrame.Add(_cardTable);
            _cardTable.SetFocus();
            _cardTable.CellActivated += (args) =>
            {
                DataTable table = args.Table;
                EditCardDialog dlg = new(_inventory);
                dlg.DataChanged += async (card) =>
                {
                    await _inventory.WriteToFirebase(card);
                    _inventory.WriteToJsonBackup();
                    UpdateCardTableRow();
                    if (_autoFind)
                        FindCard();
                };
                dlg.EditCard((Scryfall.Card)table.Rows[args.Row]["Name"]);
            };
            _cardTable.KeyDown += (args) =>
            {
                if (args.KeyEvent.Key == (Key.A | Key.CtrlMask))
                {
                    // I can't find another way to keep the TableView from treating Ctrl-A as Select All
                    ToggleAutoAdvance();
                    args.Handled = true;
                }
            };

            // if (table.Rows.Count > 0)
                // UpdateCardFrame((Scryfall.Card)table.Rows[0]["Name"]);

            // UpdateStatsFrame();
        }

        private void UpdateStatsFrame()
        {
            _curStatsFrame.RemoveAll();

            _curStatsFrame.Title = "Statistics";
            int totalCards = _cardTable.Table.Rows.Count;
            int percent = (int)((float)_collectedCount / totalCards * 100);
            Label lblCount = new($"Card Count: {_collectedCount} / {totalCards}  {percent}%") { X = 0, Y = 0, Width = Dim.Fill(), Height = 1 };

            _curStatsFrame.Add(lblCount);

            if (!_top.Subviews.Contains(_curStatsFrame))
                _top.Add(_curStatsFrame);
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
                UpdateCardFrame(curCard, card);
            }
            else
            {
                _curCardFrame.RemoveAll();

                _curCardFrame.Title = $"{card.CollectorNumber} - {card.Name}";
                InsertCardDetails(card, _curCardFrame, 0);

                if (!_top.Subviews.Contains(_curCardFrame))
                    _top.Add(_curCardFrame);
            }
        }

        private void UpdateCardFrame(MTG_Card card, Scryfall.Card fullCard)
        {
            _curCardFrame.RemoveAll();

            _curCardFrame.Title = $"{card.CollectorNumber} - {card.Name}";

            card.SortCTCs();
            for (int x = 0; x < card.Counts.Count; x++)
            {
                _curCardFrame.Add(new Label(card.Counts[x].ToString()) { X = 0, Y = x, Width = Dim.Fill() });
            }
            _curCardFrame.Add(new LineView() { X = 0, Y = card.Counts.Count, Width = Dim.Fill() });

            InsertCardDetails(fullCard, _curCardFrame, card.Counts.Count + 1);

            if (!_top.Subviews.Contains(_curCardFrame))
                _top.Add(_curCardFrame);
        }

        private void InsertCardDetails(Scryfall.Card card, FrameView frame, int StartingY)
        {
            frame.Add(new Label(card.TypeLine) { X = 0, Y = StartingY, Width = Dim.Fill() });
            TextView text = new() { X = 0, Y = StartingY + 2, Width = Dim.Fill(), Height = Dim.Fill() };
            text.ReadOnly = true;
            text.WordWrap = true;
            text.Multiline = true;
            try
            {
                if (card.Text.Length > 0)
                    text.Text = card.Text;
                else if (card.Faces.Count > 0)
                    text.Text = card.Faces[0].Text;
            }
            catch (Exception)
            {
                // An exception is thrown when the text is first set - ignore it
            }
            frame.Add(text);
        }

        public void Start()
        {
            Application.Top.Add(_menu, _statusBar);
            Application.Top.Closing += (args) =>
            {
                _inventory.WriteToJsonBackup();
            };
            Application.Run();
        }
    }
}