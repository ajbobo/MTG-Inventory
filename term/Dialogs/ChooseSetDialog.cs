using Terminal.Gui;

namespace MTG_CLI
{
    public class ChooseSetDialog
    {
        private SQLManager _sql;

        public event Action<string>? SetSelected;

        public ChooseSetDialog(SQLManager sql)
        {
            _sql = sql;
        }

        public void ChooseSet()
        {
            Button cancel = new("Cancel");
            cancel.Clicked += () => Application.RequestStop();

            Dialog selectSetDlg = new("Select a Set", cancel) { Width = 45 };

            List<string> SetList = new();
            _sql.Query(SQLManager.InternalQuery.GET_ALL_SETS).Read();
            while (_sql.ReadNext())
            {
                string name = _sql.ReadValue<string>("Name", "");
                string code = string.Format("({0})", _sql.ReadValue<string>("SetCode", "")); // Format it here as "(code)" so that it can be spaced nicely later
                SetList.Add(string.Format("{0,-7} {1}", code, name));
            }
            _sql.Close();

            ListView setListView = new(SetList) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 2 };
            setListView.OpenSelectedItem += (args) =>
                {
                    string selectedItem = (string)args.Value;
                    string setCode = selectedItem.Substring(1, selectedItem.IndexOf(')') - 1); // The item is "(code) Name" - get the value between ( and )
                    SetSelected?.Invoke(setCode);
                    Application.RequestStop();
                };

            selectSetDlg.Add(setListView);
            setListView.SetFocus();

            Application.Run(selectSetDlg);
        }
    }
}