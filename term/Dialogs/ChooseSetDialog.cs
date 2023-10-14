using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage]
    public class ChooseSetDialog
    {
        private IAPI_Connection _api;

        public event Action<string,string>? SetSelected;

        public ChooseSetDialog(IAPI_Connection api)
        {
            _api = api;
        }

        public async void ChooseSet()
        {
            Button cancel = new("Cancel");
            cancel.Clicked += () => Application.RequestStop();

            Dialog selectSetDlg = new("Select a Set", cancel) { Width = 45 };

            List<string> SetList = await _api.GetCollectableSets();

            ListView setListView = new(SetList) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 2 };
            setListView.OpenSelectedItem += (args) =>
                {
                    string selectedItem = (string)args.Value;
                    string setCode = selectedItem.Substring(1, selectedItem.IndexOf(')') - 1); // The item is "(code) Name" - get the value between ( and )
                    string setName = selectedItem.Substring(8);
                    SetSelected?.Invoke(setCode, setName);
                    Application.RequestStop();
                };

            selectSetDlg.Add(setListView);
            setListView.SetFocus();

            Application.Run(selectSetDlg);
        }
    }
}