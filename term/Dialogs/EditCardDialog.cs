using System.Diagnostics.CodeAnalysis;
using System.Text;
using Terminal.Gui;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage]
    class EditCardDialog : RefreshableDialog
    {
        public event Action<CardData>? DataChanged;

        private bool _isDirty;
        private CardData _curCard = new();
        private Dictionary<string, int> _ctcList = new();
        private IAPI_Connection _api;

        public EditCardDialog(IAPI_Connection api)
        {
            _isDirty = false;
            _api = api;
        }

        private void UpdateInventory()
        {
            _isDirty = true;

            List<CardTypeCount> updatedCTCs = new();
            int total = 0;
            foreach (string attrs in _ctcList.Keys)
            {
                if (_ctcList[attrs] <= 0)
                    continue;
                    
                updatedCTCs.Add(new CardTypeCount()
                {
                    CardType = attrs,
                    Count = _ctcList[attrs]
                });
                total += _ctcList[attrs];
            }
            _curCard.TotalCount = total;
            _curCard.CTCs = updatedCTCs;
            _api.UpdateCardData(_curCard);
        }

        public async void EditCard(string collectorNumber, string setCode)
        {
            List<CardData> cardList = await _api.GetCardsInSet(setCode, collectorNumber);
            CardData selectedCard = cardList[0];
            _curCard = selectedCard;
            _ctcList.Clear();
            _ctcList.Add("Standard", 0);
            foreach (CardTypeCount ctc in selectedCard.CTCs ?? new List<CardTypeCount>())
            {
                string attrs = ctc.CardType;
                int cnt = ctc.Count;

                if (attrs.Length > 0 && !_ctcList.ContainsKey(attrs))
                    _ctcList.Add(attrs, cnt);
                else if (_ctcList.ContainsKey(attrs))
                    _ctcList[attrs] = cnt;
            }

            Button ok = new("OK");
            ok.Clicked += () => 
            {
                UpdateInventory();
                Application.RequestStop();
            };

            Dialog dlg = new(string.Format("Edit - {0}", _curCard.Card!.Name), ok) { Width = 55, Height = _ctcList.Count + 5 };
            RefreshDialog(dlg, ok);
            dlg.Closed += (args) =>
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    DataChanged?.Invoke(_curCard);
                }
            };

            Application.Run(dlg);
        }

        protected string FormatCTC(string attrs, int count)
        {
            return string.Format("{0} - {1}", count, attrs);
        }

        protected void AdjustCount(string attrs, int newCount, Label label, Button newFocus)
        {
            newCount = Math.Max(newCount, 0);
            _ctcList[attrs] = newCount;
            label.Text = FormatCTC(attrs, newCount);
            newFocus.SetFocus();
        }

        protected void RefreshDialog(Dialog editDialog, Button ok)
        {
            if (_ctcList == null)
                return;

            ClearTmpViews(editDialog);

            int ctc_count = 0;
            foreach (string attrs in _ctcList.Keys)
            {
                if (!attrs.Equals("Standard") && _ctcList[attrs] <= 0)
                    continue;

                Label ctcName = TmpView<Label>(new(FormatCTC(attrs, _ctcList[attrs])) { X = 0, Y = ctc_count, Width = 25, Height = 1 });

                Button addOne = TmpView<Button>(new("+1") { X = Pos.Right(ctcName) + 1, Y = ctc_count });
                addOne.Clicked += () => { AdjustCount(attrs, _ctcList[attrs] + 1, ctcName, ok); };

                Button subOne = TmpView<Button>(new("-1") { X = Pos.Right(addOne) + 1, Y = ctc_count });
                subOne.Clicked += () => { AdjustCount(attrs, _ctcList[attrs] - 1, ctcName, ok); };

                Button setFour = TmpView<Button>(new("=4") { X = Pos.Right(subOne) + 1, Y = ctc_count });
                setFour.Clicked += () => { AdjustCount(attrs, 4, ctcName, ok); };

                Button delete = TmpView<Button>(new("X") { X = Pos.Right(setFour) + 1, Y = ctc_count });
                delete.Clicked += () => { AdjustCount(attrs, 0, ctcName, ok); };

                editDialog.Add(ctcName, addOne, subOne, setFour, delete);
                if (ctc_count == 0)
                    addOne.SetFocus();

                ctc_count++;
            }

            Button newCTC = TmpView<Button>(new("New Card Type") { X = Pos.Center(), Y = ctc_count });
            newCTC.Clicked += () =>
            {
                EditCTCDialog ctcDialog = new();
                ctcDialog.DataChanged += (attrs, count) =>
                {
                    if (!_ctcList?.ContainsKey(attrs) ?? false)
                        _ctcList?.Add(attrs, count);
                    else if (_ctcList?.ContainsKey(attrs) ?? false)
                        _ctcList[attrs] = count;
                    RefreshDialog(editDialog, ok);
                };
                ctcDialog.NewCTC();
            };

            editDialog.Add(newCTC);
            editDialog.Height = ctc_count + 5;
            editDialog.LayoutSubviews();
        }
    }

    [ExcludeFromCodeCoverage] 
    class EditCTCDialog : RefreshableDialog
    {
        public event Action<string, int>? DataChanged;

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
                TextField txt = TmpView<TextField>(new() { X = 0, Y = x, Width = 15 });
                txt.Data = x;
                txt.Text = _attrList[x];
                txt.TextChanged += (str) =>
                {
                    string newStr = txt?.Text.ToString() ?? "";
                    _attrList[(int)(txt?.Data ?? 0)] = newStr;
                };
                ctcDialog.Add(txt);
            }

            Button add = TmpView<Button>(new("Add") { X = 0, Y = _attrList.Count });
            add.Clicked += () =>
            {
                _attrList.Add(_attrList.Count < ATTRS.Length ? ATTRS[_attrList.Count] : "");
                RefreshDialog(ctcDialog);
            };

            Button sub = TmpView<Button>(new("Sub") { X = Pos.Right(add), Y = _attrList.Count });
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
            StringBuilder builder = new();
            foreach (string attr in _attrList)
            {
                if (builder.Length > 0)
                    builder.Append(" | ");
                builder.Append(attr.ToLower());
            }

            DataChanged?.Invoke(builder.ToString(), 1);
        }
    }
}