using Terminal.Gui;

namespace MTG_CLI
{
    abstract class RefreshableDialog
    {
        static int cnt = 0;

        protected void ClearTmpViews(Dialog dlg)
        {
            if (dlg.Visible)
            {
                List<View> removable = new();
                foreach (View view in dlg.Subviews[0].Subviews)
                {
                    if (view.Id.StartsWith("tmp"))
                    {
                        removable.Add(view);
                    }
                }
                foreach (View view in removable)
                    dlg.Subviews[0].Remove(view);
                dlg.SetNeedsDisplay();
            }
        }

        protected T tmpView<T>(T view) where T : View
        {
            view.Id = "tmp" + cnt;
            cnt++;
            return view;
        }
    }

    class EditCardDialog : RefreshableDialog
    {
        public event Action<MTG_Card>? DataChanged;

        private Inventory _inventory;
        private bool _isDirty;
        private Scryfall.Card? _selectedCard;
        private List<CardTypeCount>? _ctcList;

        public EditCardDialog(Inventory inventory)
        {
            _inventory = inventory;
            _isDirty = false;
        }

        private void UpdateInventory(Scryfall.Card card, CardTypeCount ctc)
        {
            _inventory.AddCard(card, ctc);
            _isDirty = true;
        }

        public void EditCard(Scryfall.Card selectedCard)
        {
            _selectedCard = selectedCard;
            MTG_Card? mtgCard = _inventory.GetCard(selectedCard);
            _ctcList = mtgCard?.Counts ?? new();
            if (_ctcList.Count == 0) // If the card doesn't have any CTCs, it needs a standard one
                _ctcList.Add(new CardTypeCount());

            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop();

            Dialog dlg = new(string.Format("Edit - {0}", selectedCard.Name), ok) { Width = 55, Height = _ctcList.Count + 5 };
            RefreshDialog(dlg);
            dlg.Closed += (args) =>
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    MTG_Card? card = _inventory.GetCard(_selectedCard);
                    if (card != null)
                        DataChanged?.Invoke(card);
                }
            };

            Application.Run(dlg);
        }

        protected void RefreshDialog(Dialog editDialog)
        {
            if (_selectedCard == null || _ctcList == null)
                return;

            ClearTmpViews(editDialog);

            for (int x = 0; x < (_ctcList?.Count ?? 0); x++)
            {
                CardTypeCount ctc = _ctcList?[x] ?? new();

                Label ctcName = tmpView<Label>(new(ctc.ToString()) { X = 0, Y = x, Width = 25, Height = 1 });

                Button addOne = tmpView<Button>(new("+1") { X = Pos.Right(ctcName) + 1, Y = x });
                addOne.Clicked += () => { ctc.AdjustCount(1); UpdateInventory(_selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button subOne = tmpView<Button>(new("-1") { X = Pos.Right(addOne) + 1, Y = x });
                subOne.Clicked += () => { ctc.AdjustCount(-1); UpdateInventory(_selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button setFour = tmpView<Button>(new("=4") { X = Pos.Right(subOne) + 1, Y = x });
                setFour.Clicked += () => { ctc.Count = 4; UpdateInventory(_selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button delete = tmpView<Button>(new("X") { X = Pos.Right(setFour) + 1, Y = x });
                delete.Clicked += () => { MessageBox.Query("Delete", "Not implemented yet", "OK"); };

                editDialog.Add(ctcName, addOne, subOne, setFour, delete);
                if (x == 0)
                    addOne.SetFocus();
            }

            Button newCTC = tmpView<Button>(new("New Card Type") { X = Pos.Center(), Y = (_ctcList?.Count ?? 0) });
            newCTC.Clicked += () =>
            {
                EditCTCDialog ctcDialog = new();
                ctcDialog.DataChanged += (ctc) => { _ctcList?.Add(ctc); UpdateInventory(_selectedCard, ctc); RefreshDialog(editDialog); };
                ctcDialog.NewCTC();
            };

            editDialog.Add(newCTC);
            editDialog.Height = _ctcList?.Count + 5;
            editDialog.LayoutSubviews();
        }
    }

    class EditCTCDialog : RefreshableDialog
    {
        public event Action<CardTypeCount>? DataChanged;

        private static readonly string[] ATTRS = { "Foil", "Prerelease", "Spanish", "Alt Art" };

        private List<string> _attrList; // Populate this while the Dialog is open, it will be copied to a CTC later

        public EditCTCDialog()
        {
            _attrList = new();
        }

        public void NewCTC()
        {
            _attrList.Add(ATTRS[0]);

            Button ok = new("OK");
            ok.Clicked += () => { UpdateCTC(); Application.RequestStop(); };

            Button cancel = new("Cancel");
            cancel.Clicked += () => Application.RequestStop();

            Dialog dlg = new("Create new Card Type", ok, cancel) { Width = 25, Height = 15 };
            RefreshDialog(dlg);

            Application.Run(dlg);
        }

        private void RefreshDialog(Dialog ctcDialog)
        {
            ClearTmpViews(ctcDialog);

            for (int x = 0; x < _attrList.Count; x++)
            {
                TextField txt = tmpView<TextField>(new() { X = 0, Y = x, Width = 15 });
                txt.Data = x;
                txt.Text = _attrList[x];
                txt.TextChanged += (str) =>
                {
                    string newStr = txt?.Text.ToString() ?? "";
                    _attrList[(int)(txt?.Data ?? 0)] = newStr;
                };
                ctcDialog.Add(txt);
            }

            Button add = tmpView<Button>(new("Add") { X = 0, Y = _attrList.Count });
            add.Clicked += () =>
            {
                _attrList.Add(_attrList.Count < ATTRS.Length ? ATTRS[_attrList.Count] : "");
                RefreshDialog(ctcDialog);
            };

            Button sub = tmpView<Button>(new("Sub") { X = Pos.Right(add), Y = _attrList.Count });
            sub.Clicked += () =>
            {
                if (_attrList.Count > 0)
                    _attrList.RemoveAt(_attrList.Count - 1);
                RefreshDialog(ctcDialog);
            };

            ctcDialog.Add(add, sub);
            ctcDialog.LayoutSubviews();
        }

        private void UpdateCTC()
        {
            CardTypeCount ctc = new();
            foreach (string attr in _attrList)
                ctc.Attrs.Add(attr.ToLower());
            ctc.Count = 1;

            DataChanged?.Invoke(ctc);
        }
    }
}