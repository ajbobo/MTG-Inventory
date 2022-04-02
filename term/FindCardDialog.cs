using NStack;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using System.Text;

namespace MTG_CLI
{
    class FindCardDialog
    {
        public event Action<Scryfall.Card>? CardSelected;

        private List<Scryfall.Card> _cardList;
        private CardNameValidator _validator;
        private bool _cardSelected = false;

        public FindCardDialog()
        {
            _cardList = new();
            _validator = new(_cardList);
        }

        public FindCardDialog(List<Scryfall.Card> cardList)
        {
            _cardList = cardList;
            _validator = new CardNameValidator(cardList);
        }

        public void FindCard()
        {
            if (_cardList.Count == 0)
                return;

            _validator.Text = "";

            Button ok = new("OK");
            ok.Clicked += () => { _cardSelected = true; Application.RequestStop(); };

            Button cancel = new("Cancel");
            cancel.Clicked += () => Application.RequestStop();

            Dialog dlg = new("Find Card by Name", ok, cancel) { Width = 50, Height = 6 };
            dlg.Closed += (topLevel) => 
            {
                if (_cardSelected)
                    OnCardSelected();
                _cardSelected = false;
            };

            Label lbl = new("Card Name: ") { X = 0, Y = 0 };
            TextValidateField editName = new(_validator) { X = Pos.Right(lbl), Y = Pos.Y(lbl), Width = Dim.Fill() };
            editName.KeyPress += (args) =>
            {
                if (args.KeyEvent.Key == Key.Enter)
                {
                    args.Handled = true;
                    _cardSelected = true;
                    Application.RequestStop();
                }
            };

            dlg.Add(lbl, editName);
            editName.SetFocus();

            Application.Run(dlg);
        }

        protected void OnCardSelected()
        {
            Scryfall.Card? card = _validator.SelectedCard;

            if (card != null)
                CardSelected?.Invoke(card);
        }

    }

    class CardNameValidator : ITextValidateProvider
    {
        private List<Scryfall.Card> _cardList;

        public Scryfall.Card? SelectedCard { get; protected set; }

        public CardNameValidator(List<Scryfall.Card> cardList)
        {
            _cardList = cardList;
        }

        private StringBuilder _typed = new StringBuilder();

        private string FindClosestWord()
        {
            string typedName = _typed.ToString().ToLower();
            foreach (Scryfall.Card card in _cardList)
            {
                string cardName = card.Name.ToLower();
                if (typedName.Length > 0 && cardName.StartsWith(typedName))
                {
                    SelectedCard = card;
                    return card.Name;
                }
            }
            SelectedCard = null;
            return "";
        }

        public bool Fixed => false;

        public bool IsValid => true;

        public ustring Text
        {
            get => FindClosestWord();
            set => InsertWord(value.ToString());
        }

        public ustring DisplayText => FindClosestWord();

        public int Cursor(int pos)
        {
            if (pos <= 0)
                return 0;

            return Math.Min(pos, _typed.Length);
        }

        public int CursorEnd()
        {
            return _typed.Length;
        }

        public int CursorLeft(int pos)
        {
            return Math.Max(pos - 1, 0);
        }

        public int CursorRight(int pos)
        {
            return Math.Min(pos + 1, _typed.Length);
        }

        public int CursorStart()
        {
            return 0;
        }

        public bool Delete(int pos)
        {
            if (pos < 0 || _typed.Length == 0)
                return false;

            _typed.Remove(pos, 1);

            return true;
        }

        public bool InsertAt(char ch, int pos)
        {
            pos = Math.Min(pos, _typed.Length);

            _typed.Insert(pos, ch);

            if (FindClosestWord() == "")
            {
                _typed.Remove(pos, 1);
                return false;
            }

            return true;
        }

        private void InsertWord(string? newStr)
        {
            if (newStr?.Length == 0)
                _typed.Clear();

            int index = 0;
            while (index < newStr?.Length)
            {
                if (InsertAt(newStr[index], index))
                    index++;
                else
                    break;
            }
        }
    }
}