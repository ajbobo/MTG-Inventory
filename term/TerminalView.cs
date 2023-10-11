using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage] // For now - maybe I can separate logic from UI?
    class TerminalView
    {
        readonly private string _dbSetCode = ConfigurationManager.AppSettings["DB_Card_Field_SetCode"]!;
        readonly private string _dbNumber = ConfigurationManager.AppSettings["DB_Card_Field_Number"]!;
        readonly private string _dbName = ConfigurationManager.AppSettings["DB_Card_Field_Name"]!;
        readonly private string _dbAttrs = ConfigurationManager.AppSettings["DB_Card_Field_Attrs"]!;
        readonly private string _dbCount = ConfigurationManager.AppSettings["DB_Card_Field_Count"]!;
        readonly private string _dbRarity = ConfigurationManager.AppSettings["DB_Card_Field_Rarity"]!;
        readonly private string _dbColor = ConfigurationManager.AppSettings["DB_Card_Field_ColorIdentity"]!;
        readonly private string _dbMana = ConfigurationManager.AppSettings["DB_Card_Field_ManaCost"]!;
        readonly private string _dbPrice = ConfigurationManager.AppSettings["DB_Card_Field_Price"]!;
        readonly private string _dbPriceFoil = ConfigurationManager.AppSettings["DB_Card_Field_PriceFoil"]!;
        readonly private string _dbFrontText = ConfigurationManager.AppSettings["DB_Card_Field_FrontText"]!;
        readonly private string _dbTypeLine = ConfigurationManager.AppSettings["DB_Card_Field_TypeLine"]!;

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
        private ChooseSetDialog _chooseSet;

        private ISQL_Connection _sql;
        private IAPI_Connection _api;

        private int _collectedCount;
        private FilterSettings _filterSettings;
        private bool _autoFind = true;

        public event Action<string, string>? SelectedSetChanged;
        public event Action? DataChanged;

        public TerminalView(ISQL_Connection sql, IAPI_Connection api)
        {
            _sql = sql;
            _api = api;
            _filterSettings = new();

            Application.Init();

            _top = Application.Top;
            _top.ColorScheme = Colors.ColorSchemes["Base"];

            Label lbl = new("Press Ctrl-S to choose a Set") { X = Pos.Center(), Y = Pos.Center() };
            _top.Add(lbl);

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
            _findCardDlg = new(sql);
            _findCardDlg.CardSelected += FoundCard;
            _editFilters = new(_filterSettings);
            // _editFilters.OnClose += SetCardList;
            _chooseSet = new(api);
            _chooseSet.SetSelected += (setCode, setName) => { SelectedSetChanged?.Invoke(setCode, setName); };
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
            string selectedName = row["Name"].ToString()!;

            // Starting after the current row, find the next one with the same Name
            int index = _cardTable.SelectedRow + 1;
            while (true) // Loop until a card is found, wrap back around if needed, stops when it finds the same card again
            {
                DataRow nextRow = _cardTable.Table.Rows[index];
                string nextName = nextRow["Name"].ToString()!;
                if (nextName.Equals(selectedName))
                {
                    _cardTable.SelectedRow = index;
                    _cardTable.EnsureSelectedCellIsVisible();
                    UpdateCardFrame(nextRow["#"].ToString()!);
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
            string cardNumber = _sql.Query(DB_Query.GET_CARD_NUMBER).WithParam("@Name", cardName).ExecuteScalar<string>()!;

            DataRow? cardRow = _cardTable.Table.Rows.Find(cardNumber);
            _cardTable.SelectedRow = _cardTable.Table.Rows.IndexOf(cardRow);
            _cardTable.EnsureSelectedCellIsVisible();

            UpdateCardFrame(cardNumber);
        }

        private void ChooseFilters()
        {
            _editFilters.EditFilters();
        }

        private void ChooseSet()
        {
            _chooseSet.ChooseSet();
        }

        public void SetCurrentSet(string setCode, string setName)
        {
            _curSetFrame.Title = setName;
            _curSetFrame.RemoveAll();
            _curSetFrame.Add(new Label("Loading cards...") { X = Pos.Center(), Y = 0 });

            if (!_top.Subviews.Contains(_curSetFrame))
                _top.Add(_curSetFrame);
        }

        public async void SetCardList(string setCode)
        {
            _collectedCount = 0;

            // This returns the filtered list of cards - FINISH ME
            List<CardData> cardList = await _api.GetCardsInSet(setCode);

            _curSetFrame.RemoveAll();

            DataTable table = new();
            table.PrimaryKey = new[] { table.Columns.Add("#") };
            table.Columns.Add("Cnt");
            table.Columns.Add("Rarity");
            table.Columns.Add("Name");
            table.Columns.Add("Color");
            table.Columns.Add("Cost");
            table.Columns.Add("Price");

            foreach (CardData curCard in cardList)
            {
                DataRow row = table.NewRow();
                row["#"] = curCard["collectorNumber"];
                string cnt = curCard["count"]?.ToString() ?? "0";
                row["Cnt"] = cnt;
                row["Rarity"] = curCard["rarity"].ToString()?[..1].ToUpper() ?? "";
                row["Name"] = curCard["name"];
                row["Color"] = curCard["color"];
                row["Cost"] = curCard["manaCost"];
                string price = curCard["price"]?.ToString() ?? "0.00";
                if (price.Length == 0 || cnt.Contains('*'))
                    price = curCard["priceFoil"]?.ToString() ?? "0.00";
                row["Price"] = $"{(price.Length > 0 ? '$' : "")}{price}";

                table.Rows.Add(row);

                if (!cnt.Equals("0"))
                    _collectedCount++;
            }

            _cardTable = new(table)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                FullRowSelect = true,
                MultiSelect = false
            };
            _cardTable.Style.AlwaysShowHeaders = true;
            _cardTable.Style.ExpandLastColumn = false;
            _cardTable.Style.ColumnStyles.Add(table.Columns["#"]!, new() { MaxWidth = 5, MinWidth = 5 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Cnt"]!, new() { MaxWidth = 3, MinWidth = 3 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Rarity"]!, new() { MinWidth = 2, MaxWidth = 2 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Name"]!, new() { MinWidth = 15 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Color"]!, new() { MaxWidth = 5, MinWidth = 5 });
            _cardTable.Style.ColumnStyles.Add(table.Columns["Price"]!, new() { MaxWidth = 8, MinWidth = 5 });
            _cardTable.SelectedCellChanged += (args) => UpdateCardFrame(table.Rows[args.NewRow]["#"].ToString()!);

            _curSetFrame.Add(_cardTable);
            _cardTable.SetFocus();
            _cardTable.CellActivated += (args) =>
            {
                DataTable table = args.Table;
                EditCardDialog dlg = new(_sql);
                dlg.DataChanged += () =>
                {
                    DataChanged?.Invoke(); 
                    UpdateCardTableRow();
                    if (_autoFind)
                        FindCard();
                };
                dlg.EditCard(table.Rows[args.Row]["#"]?.ToString()!);
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
                UpdateCardFrame(table.Rows[0]["#"].ToString()!);

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

            string cardNum = row["#"].ToString()!;
            UpdateCardFrame(cardNum);

            string newCount = _sql.Query(DB_Query.GET_SINGLE_CARD_COUNT).WithParam("@CollectorNumber", cardNum).ExecuteScalar<string>() ?? "0";
            row["Cnt"] = newCount;
        }

        private void UpdateCardFrame(string cardNumber)
        {
            _curCardFrame.RemoveAll();

            _sql.Query(DB_Query.GET_CARD_DETAILS).WithParam("@CollectorNumber", cardNumber).OpenToRead();
            if (!_sql.IsReady())
                return;

            _sql.ReadNext();
            string title = $"{_sql.ReadValue<string>(_dbNumber, "")} - {_sql.ReadValue<string>(_dbName, "")}";
            string frontText = _sql.ReadValue<string>(_dbFrontText, "");
            string typeLine = _sql.ReadValue<string>(_dbTypeLine, "");
            _sql.Close();

            _curCardFrame.Title = title;

            _sql.Query(DB_Query.GET_CARD_CTCS).WithParam("@CollectorNumber", cardNumber).OpenToRead();
            int cnt = 0;
            while (_sql.ReadNext())
            {
                int count = _sql.ReadValue<int>(_dbCount, 0);
                if (count > 0)
                {
                    string ctc = $"{count} - {_sql.ReadValue<string>(_dbAttrs, "")}";
                    _curCardFrame.Add(new Label(ctc) { X = 0, Y = cnt, Width = Dim.Fill() });
                    cnt++;
                }
            }
            _sql.Close();

            _curCardFrame.Add(new LineView() { X = 0, Y = cnt, Width = Dim.Fill() });

            if (!_top.Subviews.Contains(_curCardFrame))
                _top.Add(_curCardFrame);

            InsertCardDetails(typeLine, frontText, _curCardFrame, cnt + 1);
        }

        private void InsertCardDetails(string typeLine, string frontText, FrameView frame, int StartingY)
        {
            frame.Add(new Label(typeLine) { X = 0, Y = StartingY, Width = Dim.Fill() });
            TextView text = new() { X = 0, Y = StartingY + 2, Width = Dim.Fill(), Height = Dim.Fill() };
            text.ReadOnly = true;
            frame.Add(text);
            text.Text = frontText;
            text.WordWrap = true;
        }

        public void Start()
        {
            Application.Top.Add(_menu, _statusBar);
            Application.Run();
        }
    }
}