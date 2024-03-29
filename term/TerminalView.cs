using System.Data;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage] // For now - maybe I can separate logic from UI?
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
        private ChooseSetDialog _chooseSet;

        private IAPI_Connection _api;

        private string _curSetCode = "";
        private int _collectedCount;
        private FilterSettings _filterSettings;
        private bool _autoFind = true;

        public event Action<string, string>? SelectedSetChanged;
        public event Action? DataChanged;

        public TerminalView(IAPI_Connection api)
        {
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
            _findCardDlg = new(_curSetCode, api);
            _findCardDlg.CardSelected += FoundCard;
            _editFilters = new(_filterSettings);
            _editFilters.OnClose += UpdateCardList;
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
            _findCardDlg.FindCard(_curSetCode);
        }

        private async void FoundCard(string cardName)
        {
            List<CardData> cardList = await _api.GetCardsInSet(_curSetCode);

            var filteredCards =
                from card in cardList
                where card.Card!.Name.Equals(cardName)
                select card;
            CardData foundCard = filteredCards.First();

            DataRow? cardRow = _cardTable.Table.Rows.Find(foundCard.Card!.CollectorNumber);
            _cardTable.SelectedRow = _cardTable.Table.Rows.IndexOf(cardRow);
            _cardTable.EnsureSelectedCellIsVisible();

            UpdateCardFrame(foundCard);
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
            _curSetCode = setCode;

            _curSetFrame.Title = setName;
            _curSetFrame.RemoveAll();
            _curSetFrame.Add(new Label("Loading cards...") { X = Pos.Center(), Y = 0 });

            if (!_top.Subviews.Contains(_curSetFrame))
                _top.Add(_curSetFrame);
        }

        private void UpdateCardList()
        {
            SetCardList(_curSetCode);
        }

        public async void SetCardList(string setCode)
        {
            _collectedCount = 0;

            // This returns the filtered list of cards
            List<CardData> cardList = await _api.GetCardsInSet(setCode, _filterSettings);

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
                MTG_Card cardDetail = curCard.Card!;
                DataRow row = table.NewRow();
                row["#"] = cardDetail.CollectorNumber;
                string cnt = curCard.TotalCount.ToString() + GetDecorations(curCard.CTCs);
                row["Cnt"] = cnt;
                row["Rarity"] = cardDetail.Rarity[..1].ToUpper();
                row["Name"] = cardDetail.Name;
                row["Color"] = cardDetail.ColorIdentity;
                row["Cost"] = cardDetail.CastingCost;
                string price = cardDetail.Price.ToString();
                if (price.Length == 0 || cnt.Contains('*'))
                    price = cardDetail.PriceFoil.ToString();
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
                EditCardDialog dlg = new(_api);
                dlg.DataChanged += (curCard) =>
                {
                    DataChanged?.Invoke();
                    UpdateCardTableRow(curCard);
                    if (_autoFind)
                        FindCard();
                };
                dlg.EditCard(table.Rows[args.Row]["#"]?.ToString()!, _curSetCode);
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

        private void UpdateCardTableRow(CardData selectedCard)
        {
            var row = _cardTable.Table.Rows[_cardTable.SelectedRow];

            UpdateCardFrame(selectedCard);

            string newCount = selectedCard.TotalCount.ToString() + GetDecorations(selectedCard.CTCs);
            row["Cnt"] = newCount;
        }

        private static string GetDecorations(List<CardTypeCount>? CTCs)
        {
            if (CTCs == null)
                return "";

            StringBuilder builder = new();
            foreach (CardTypeCount ctc in CTCs)
            {
                string type = ctc.CardType.ToLower();
                if (type.Equals("standard"))
                    continue;
                if (type.Contains("foil"))
                    builder.Append('*');
                if (!ctc.CardType.ToLower().Equals("foil"))
                    builder.Append('Ω');
            }

            return builder.ToString();
        }

        private async void UpdateCardFrame(string cardNum)
        {
            List<CardData> cardList = await _api.GetCardsInSet(_curSetCode, cardNum);
            CardData selectedCard = cardList[0];
            UpdateCardFrame(selectedCard);
        }

        private void UpdateCardFrame(CardData selectedCard)
        {
            _curCardFrame.RemoveAll();

            string title = $"{selectedCard.Card!.CollectorNumber} - {selectedCard.Card!.Name}";
            string frontText = selectedCard.Card!.FrontText;
            string typeLine = selectedCard.Card!.TypeLine;

            _curCardFrame.Title = title;

            int cnt = 0;
            foreach (CardTypeCount curCTC in selectedCard.CTCs ?? new())
            {
                int count = curCTC.Count;
                if (count > 0)
                {
                    string ctc = $"{count} - {curCTC.CardType}";
                    _curCardFrame.Add(new Label(ctc) { X = 0, Y = cnt, Width = Dim.Fill() });
                    cnt++;
                }
            }

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