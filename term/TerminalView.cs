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
                    // new MenuItem("_Edit Filters", "", ChooseFilters),
                    new MenuItem("_Find a Card", "", FindCard),
                }),
            });

            _autoStatus = new(Key.A | Key.CtrlMask, "~Ctrl-A~ Auto On", ToggleAutoAdvance);

            _statusBar = new(new StatusItem[]{
                new StatusItem(Key.S | Key.CtrlMask, "~Ctrl-S~ Choose Set", ChooseSet ),
                // new StatusItem(Key.E | Key.CtrlMask, "~Ctrl-E~ Edit Filters", ChooseFilters ),
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
            string selectedName = row["Name"].ToString() ?? "";

            // Starting after the current row, find the next one with the same Name
            int index = _cardTable.SelectedRow + 1;
            while (true) // Loop until a card is found, wrap back around if needed, stops when it finds the same card again
            {
                DataRow nextRow = _cardTable.Table.Rows[index];
                string nextName = nextRow["Name"].ToString() ?? "";
                if (nextName.Equals(selectedName))
                {
                    _cardTable.SelectedRow = index;
                    _cardTable.EnsureSelectedCellIsVisible();
                    UpdateCardFrame(nextRow["#"].ToString() ?? "");
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

        private void FoundCard(string cardName)
        {
            string cardNumber = _sql.Query(GET_CARD_NUMBER).WithParam("@Name", cardName).ExecuteScalar<string>() ?? "";

            DataRow? cardRow = _cardTable.Table.Rows.Find(cardNumber);
            _cardTable.SelectedRow = _cardTable.Table.Rows.IndexOf(cardRow);
            _cardTable.EnsureSelectedCellIsVisible();

            UpdateCardFrame(cardNumber);
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
            _sql.Query(SQLManager.InternalQuery.GET_ALL_SETS).Read();
            while (_sql.ReadNext())
            {
                string name = _sql.ReadValue<string>( "Name", "");
                string code = string.Format("({0})", _sql.ReadValue<string>("SetCode", "")); // Format it here as "(code)" so that it can be spaced nicely later
                SetList.Add(string.Format("{0,-7} {1}", code, name));
            }
            _sql.Close();

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
            _sql.Query(GET_SET_CARDS).WithParam("@SetCode", curSetCode).Read();

            _findCardDlg = new(new());
            _findCardDlg.CardSelected += FoundCard;

            _curSetFrame.RemoveAll();

            DataTable table = new();
            table.PrimaryKey = new[] { table.Columns.Add("#") };
            table.Columns.Add("Cnt");
            table.Columns.Add("Rarity");
            table.Columns.Add("Name");
            table.Columns.Add("Color");
            table.Columns.Add("Cost");

            while (_sql.ReadNext())
            {
                DataRow row = table.NewRow();
                row["#"] = _sql.ReadValue<string>("Collector_Number", "");
                string cnt = _sql.ReadValue<string>("Cnt", "");
                row["Cnt"] = cnt;
                row["Rarity"] = _sql.ReadValue<string>("Rarity", "").ToUpper()[0];
                row["Name"] = _sql.ReadValue<string>("Name", "");
                row["Color"] = _sql.ReadValue<string>("ColorIdentity", "");
                row["Cost"] = _sql.ReadValue<string>("ManaCost", "");
                table.Rows.Add(row);

                if (!cnt.Equals("0"))
                    _collectedCount++;
            }
            _sql.Close();

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
            _cardTable.SelectedCellChanged += (args) => UpdateCardFrame(table.Rows[args.NewRow]["#"].ToString() ?? "");

            _curSetFrame.Add(_cardTable);
            _cardTable.SetFocus();
            _cardTable.CellActivated += (args) =>
            {
                DataTable table = args.Table;
                EditCardDialog dlg = new(_inventory, _sql);
                dlg.DataChanged += async () =>
                {
                    await _inventory.WriteToFirebase();
                    _inventory.WriteToJsonBackup();
                    UpdateCardTableRow();
                    if (_autoFind)
                        FindCard();
                };
                dlg.EditCard(table.Rows[args.Row]["#"]?.ToString() ?? "");
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

            if (table.Rows.Count > 0)
                UpdateCardFrame(table.Rows[0]["#"].ToString() ?? "");

            UpdateStatsFrame();
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

            string cardNum = row["#"].ToString() ?? "";
            UpdateCardFrame(cardNum);

            string newCount = _sql.Query(GET_SINGLE_CARD_COUNT).WithParam("@Collector_Number", cardNum).ExecuteScalar<string>() ?? "0";
            row["Cnt"] = newCount;
        }

        private void UpdateCardFrame(string cardNumber)
        {
            _curCardFrame.RemoveAll();

            _sql.Query(GET_CARD_DETAILS).WithParam("@Collector_Number", cardNumber).Read();
            if (!_sql.HasReader())
                return;

            _sql.ReadNext();
            string title = $"{_sql.ReadValue<string>("Collector_Number", "")} - {_sql.ReadValue<string>("Name", "")}";
            string frontText = _sql.ReadValue<string>("FrontText", "");
            string typeLine = _sql.ReadValue<string>("TypeLine", "");
            _sql.Close();

            _curCardFrame.Title = title;

            _sql.Query(GET_CARD_CTCS).WithParam("@Collector_Number", cardNumber).Read();
            int cnt = 0;
            while (_sql.ReadNext())
            {
                int count = _sql.ReadValue<int>("Count", 0);
                if (count > 0)
                {
                    string ctc = $"{count} - {_sql.ReadValue<string>("Attrs", "")}";
                    _curCardFrame.Add(new Label(ctc) { X = 0, Y = cnt, Width = Dim.Fill() });
                    cnt++;
                }
            }
            _sql.Close();

            _curCardFrame.Add(new LineView() { X = 0, Y = cnt, Width = Dim.Fill() });

            InsertCardDetails(typeLine, frontText, _curCardFrame, cnt + 1);

            if (!_top.Subviews.Contains(_curCardFrame))
                _top.Add(_curCardFrame);
        }

        private void InsertCardDetails(string typeLine, string frontText, FrameView frame, int StartingY)
        {
            frame.Add(new Label(typeLine) { X = 0, Y = StartingY, Width = Dim.Fill() });
            TextView text = new() { X = 0, Y = StartingY + 2, Width = Dim.Fill(), Height = Dim.Fill() };
            text.ReadOnly = true;
            text.WordWrap = true;
            text.Multiline = true;
            try
            {
                text.Text = frontText;
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