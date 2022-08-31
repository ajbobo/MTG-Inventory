using System.Text;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using NStack;
using static MTG_CLI.SQLManager.InternalQuery;

namespace MTG_CLI
{
    class FindCardDialog
    {
        public event Action<string>? CardSelected;

        private CardNameValidator _validator;
        private bool _cardSelected = false;

        public FindCardDialog()
        {
            _validator = new();
        }

        public FindCardDialog(SQLManager sql)
        {
            _validator = new CardNameValidator(sql);
        }

        public void FindCard()
        {
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
            Label predictedText = new() { X = 11, Y = 0 };
            TextValidateField editName = new(_validator) { X = Pos.Right(lbl), Y = Pos.Y(lbl), Width = Dim.Fill() };
            predictedText.ColorScheme = new()
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.DarkGray)
            };
            editName.KeyUp += (args) =>
            {
                // This updates a label with the predicted (but untyped) text in a different color
                predictedText.Clear();
                string fullText = _validator.DisplayText?.ToString() ?? "";
                string typed = _validator.Text?.ToString() ?? "";
                predictedText.Text = fullText.Substring(typed.Length);
                predictedText.X = 11 + typed.Length;
                editName.SetNeedsDisplay();

                if (args.KeyEvent.Key == Key.Enter)
                {
                    args.Handled = true;
                    _cardSelected = true;
                    Application.RequestStop();
                }
            };

            dlg.Add(lbl, editName, predictedText);
            editName.SetFocus();

            Application.Run(dlg);
        }

        protected void OnCardSelected()
        {
            string? cardName = _validator.SelectedCard;

            if (cardName != null)
                CardSelected?.Invoke(cardName);
        }

    }

    class CardNameValidator : ITextValidateProvider
    {
        private List<string> _cardNames;

        public string? SelectedCard { get; protected set; }

        public CardNameValidator()
        {
            _cardNames = new();
        }

        public CardNameValidator(SQLManager sql)
        {
            _cardNames = new();

            sql.Query(GET_CARD_NAMES).Read();
            while (sql.ReadNext())
            {
                _cardNames.Add(sql.ReadValue<string>("Name", ""));
            }
            sql.Close();
        }

        private StringBuilder _typed = new StringBuilder();

        private string FindClosestWord()
        {
            string typedName = _typed.ToString().ToLower();
            foreach (string name in _cardNames)
            {
                string cardName = name.ToLower();
                if (typedName.Length > 0 && cardName.StartsWith(typedName))
                {
                    SelectedCard = name;
                    return name;
                }
            }
            SelectedCard = null;
            return "";
        }

        public bool Fixed => false;

        public bool IsValid => true;

        public ustring Text
        {
            get => _typed.ToString();
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