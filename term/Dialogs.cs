using System.Text;
using Microsoft.Data.Sqlite;
using Terminal.Gui;
using static MTG_CLI.SQLManager.InternalQuery;

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
        public event Action? DataChanged;

        private Inventory _inventory;
        private bool _isDirty;
        private string _curCollectorNumber = "";
        private string _curCardName = "";
        private Dictionary<string, int> _ctcList = new();
        private SQLManager _sql;

        public EditCardDialog(Inventory inventory, SQLManager sql)
        {
            _inventory = inventory;
            _isDirty = false;
            _sql = sql;
        }

        private void UpdateInventory(string collector_number, string attrs, int count)
        {
            _sql.Query(UPDATE_CARD_CTC)
                .WithParam("@Collector_Number", collector_number)
                .WithParam("@Attrs", attrs)
                .WithParam("@Count", count)
                .Execute();
            _isDirty = true;
        }

        public void EditCard(string collector_number)
        {
            _curCollectorNumber = collector_number;

            SqliteDataReader? reader = _sql.Query(GET_CARD_CTCS).WithParam("@Collector_Number", collector_number).Read();
            _ctcList.Clear();
            _ctcList.Add("Standard", 0);
            while (reader?.Read() ?? false)
            {
                _curCardName = _sql.SafeRead<string>(reader, "Name", "");
                string attrs = _sql.SafeRead<string>(reader, "Attrs", "");
                int cnt = _sql.SafeRead<int>(reader, "Count", 0);

                if (attrs.Length > 0 && !_ctcList.ContainsKey(attrs))
                    _ctcList.Add(attrs, cnt);
                else if (_ctcList.ContainsKey(attrs))
                    _ctcList[attrs] = cnt;
            }


            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop();

            Dialog dlg = new(string.Format("Edit - {0}", _curCardName), ok) { Width = 55, Height = _ctcList.Count + 5 };
            RefreshDialog(dlg, ok);
            dlg.Closed += (args) =>
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    DataChanged?.Invoke();
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
            _ctcList[attrs] = newCount;
            UpdateInventory(_curCollectorNumber, attrs, newCount);
            label.Text = FormatCTC(attrs, newCount);
            newFocus.SetFocus();
        }

        protected void RefreshDialog(Dialog editDialog, Button ok)
        {
            if (_ctcList == null)
                return;

            ClearTmpViews(editDialog);

            int x = 0;
            foreach (string attrs in _ctcList.Keys)
            {
                if (!attrs.Equals("Standard") && _ctcList[attrs] <= 0)
                    continue;

                Label ctcName = tmpView<Label>(new(FormatCTC(attrs, _ctcList[attrs])) { X = 0, Y = x, Width = 25, Height = 1 });

                Button addOne = tmpView<Button>(new("+1") { X = Pos.Right(ctcName) + 1, Y = x });
                addOne.Clicked += () => { AdjustCount(attrs, _ctcList[attrs] + 1, ctcName, ok); };

                Button subOne = tmpView<Button>(new("-1") { X = Pos.Right(addOne) + 1, Y = x });
                subOne.Clicked += () => { AdjustCount(attrs, _ctcList[attrs] - 1, ctcName, ok); };

                Button setFour = tmpView<Button>(new("=4") { X = Pos.Right(subOne) + 1, Y = x });
                setFour.Clicked += () => { AdjustCount(attrs, 4, ctcName, ok); };

                Button delete = tmpView<Button>(new("X") { X = Pos.Right(setFour) + 1, Y = x });
                delete.Clicked += () => { AdjustCount(attrs, 0, ctcName, ok); };

                editDialog.Add(ctcName, addOne, subOne, setFour, delete);
                if (x == 0)
                    addOne.SetFocus();

                x++;
            }

            Button newCTC = tmpView<Button>(new("New Card Type") { X = Pos.Center(), Y = (_ctcList?.Count ?? 0) });
            newCTC.Clicked += () =>
            {
                EditCTCDialog ctcDialog = new();
                ctcDialog.DataChanged += (attrs, count) =>
                {
                    if (!_ctcList?.ContainsKey(attrs) ?? false)
                        _ctcList?.Add(attrs, count);
                    else if (_ctcList?.ContainsKey(attrs) ?? false)
                        _ctcList[attrs] = count;
                    UpdateInventory(_curCollectorNumber, attrs, count);
                    RefreshDialog(editDialog, ok);
                };
                ctcDialog.NewCTC();
            };

            editDialog.Add(newCTC);
            editDialog.Height = _ctcList?.Count + 5;
            editDialog.LayoutSubviews();
        }
    }

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