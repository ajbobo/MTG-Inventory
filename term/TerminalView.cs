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
        private string _curSetCode = "";
        public List<Scryfall.Set> SetList { get; set; } = new();
        private MTG_Card? _curCard;
        private bool _isDirty = false;

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
                new StatusItem(Key.S, "Choose Set (~Shift-S~)", ChooseSet ),
                new StatusItem(Key.G, "Goto Card (~Shift-G~)", FindCard ),
            });

            _curSetFrame = new FrameView() { X = 0, Y = 0, Width = Dim.Percent(75), Height = Dim.Fill() };
            _cardTable = new TableView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _curCardFrame = new FrameView() { X = Pos.Right(_curSetFrame), Y = Pos.Top(_curSetFrame) + 3, Width = Dim.Fill(), Height = Dim.Fill() };
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

            DataTable table = new();
            table.Columns.Add(new DataColumn("#"));
            table.Columns.Add(new DataColumn("Cnt"));
            table.Columns.Add(new DataColumn("Rarity"));
            table.Columns.Add(new DataColumn("Name"));
            table.Columns.Add(new DataColumn("Color"));
            table.Columns.Add(new DataColumn("Cost"));

            foreach (Scryfall.Card card in cardList)
            {
                DataRow row = table.NewRow();
                row["#"] = card.collector_number;
                row["Cnt"] = _inventory.GetCardCountDisplay(_curSetCode, card.collector_number);
                row["Rarity"] = card.rarity.ToUpper()[0];
                row["Name"] = card.name;
                row["Color"] = String.Join("", card.color_identity?.ToArray() ?? new string[] { });
                row["Cost"] = card.mana_cost;
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
                string collectorNumber = table.Rows[args.Row]["#"]?.ToString() ?? "";
                EditCard(cardList[args.Row], _curSetCode);
            };

            UpdateCardFrame(cardList[0]);
        }

        private void UpdateCardTableRow()
        {
            if (_curCard == null)
                return;

            int row = _cardTable.SelectedRow;
            _cardTable.Table.Rows[row]["Cnt"] = _inventory.GetCardCountDisplay(_curCard);
            UpdateCardFrame(_curCard);
        }

        private void EditCard(Scryfall.Card curCard, string curSetCode)
        {
            MTG_Card? mtgCard = _inventory.GetCard(curSetCode, curCard.collector_number);
            _curCard = mtgCard;
            if (mtgCard == null) // The card isn't in Inventory right now - It may need to be added later - FINISH ME
                return;

            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop();

            Dialog editDialog = new(string.Format("Edit - {0}", mtgCard.Name), ok) { Width = 55, Height = mtgCard.Counts.Count + 5 };
            for (int x = 0; x < mtgCard.Counts.Count; x++)
            {
                CardTypeCount ctc = mtgCard.Counts[x];

                Label ctcName = new(ctc.ToString()) { X = 0, Y = x, Width = 25, Height = 1 };

                Button addOne = new("+1") { X = Pos.Right(ctcName) + 1, Y = x };
                addOne.Clicked += () => { ctc.AdjustCount(1); ctcName.Text = ctc.ToString(); _isDirty = true; };

                Button subOne = new("-1") { X = Pos.Right(addOne) + 1, Y = x };
                subOne.Clicked += () => { ctc.AdjustCount(-1); ctcName.Text = ctc.ToString(); _isDirty = true; };

                Button setFour = new("=4") { X = Pos.Right(subOne) + 1, Y = x };
                setFour.Clicked += () => { ctc.Count = 4; ctcName.Text = ctc.ToString(); _isDirty = true; };

                Button delete = new("X") { X = Pos.Right(setFour) + 1, Y = x };
                delete.Clicked += () => { MessageBox.Query("Delete", "Not implemented yet", "OK"); };

                editDialog.Add(ctcName, addOne, subOne, setFour, delete);
                if (x == 0)
                    addOne.SetFocus();
            }
            Button newCTC = new("New Card Type") { X = Pos.Center(), Y = mtgCard.Counts.Count };
            newCTC.Clicked += () => { MessageBox.Query("New CTC", "Add a new CTC now", "OK"); };
            editDialog.Add(newCTC);

            editDialog.Closed += (args) =>
                {
                    if (_isDirty)
                    {
                        MessageBox.Query("Dirty Data", "Database updates here", "OK");
                        _isDirty = false;
                        UpdateCardTableRow();
                    }
                };

            Application.Run(editDialog);
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
            MTG_Card? curCard = _inventory.GetCard(_curSetCode, card.collector_number);
            if (curCard != null)
            {
                UpdateCardFrame(curCard);
            }
            else
            {
                _curCardFrame.RemoveAll();

                _curCardFrame.Title = $"{card.collector_number} - {card.name}";

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