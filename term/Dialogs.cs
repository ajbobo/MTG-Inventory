using Terminal.Gui;

namespace MTG_CLI
{
    class EditCardDialog
    {
        public event Action? DataChanged;

        private Inventory _inventory;
        private bool _isDirty;

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
            MTG_Card? mtgCard = _inventory.GetCard(selectedCard);
            List<CardTypeCount> ctcList = mtgCard?.Counts ?? new();
            if (ctcList.Count == 0) // If the card doesn't have any CTCs, it needs a standard one
                ctcList.Add(new CardTypeCount());

            Button ok = new("OK");
            ok.Clicked += () => Application.RequestStop();

            Dialog editDialog = new(string.Format("Edit - {0}", selectedCard.Name), ok) { Width = 55, Height = ctcList.Count + 5 };
            for (int x = 0; x < ctcList.Count; x++)
            {
                CardTypeCount ctc = ctcList[x];

                Label ctcName = new(ctc.ToString()) { X = 0, Y = x, Width = 25, Height = 1 };

                Button addOne = new("+1") { X = Pos.Right(ctcName) + 1, Y = x };
                addOne.Clicked += () => { ctc.AdjustCount(1); UpdateInventory(selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button subOne = new("-1") { X = Pos.Right(addOne) + 1, Y = x };
                subOne.Clicked += () => { ctc.AdjustCount(-1); UpdateInventory(selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button setFour = new("=4") { X = Pos.Right(subOne) + 1, Y = x };
                setFour.Clicked += () => { ctc.Count = 4; UpdateInventory(selectedCard, ctc); ctcName.Text = ctc.ToString(); };

                Button delete = new("X") { X = Pos.Right(setFour) + 1, Y = x };
                delete.Clicked += () => { MessageBox.Query("Delete", "Not implemented yet", "OK"); };

                editDialog.Add(ctcName, addOne, subOne, setFour, delete);
                if (x == 0)
                    addOne.SetFocus();
            }
            Button newCTC = new("New Card Type") { X = Pos.Center(), Y = ctcList.Count };
            newCTC.Clicked += () => { MessageBox.Query("New CTC", "Add a new CTC now", "OK"); };
            editDialog.Add(newCTC);

            editDialog.Closed += (args) => 
            {
                if (_isDirty)
                {
                    _isDirty = false;
                    DataChanged?.Invoke();
                }
            };

            Application.Run(editDialog);
        }
    }
}