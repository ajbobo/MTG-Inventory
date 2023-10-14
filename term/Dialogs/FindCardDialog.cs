using System.Text;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using NStack;
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage]
    class FindCardDialog
    {
        public event Action<string>? CardSelected;

        private IAPI_Connection _api;
        private CardNameValidator _validator;
        private bool _cardSelected = false;

        public FindCardDialog(string setCode, IAPI_Connection api)
        {
            _api = api;
            _validator = new CardNameValidator(setCode, _api);
        }

        public void FindCard(string setCode)
        {
            _validator.RefreshNames(setCode);
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
                string fullText = _validator.DisplayText?.ToString()!;
                string typed = _validator.Text?.ToString()!;
                predictedText.Text = fullText.Substring(typed.Length);
                predictedText.X = 11 + typed.Length;
                editName.SetNeedsDisplay();

                // Sometimes a leftover Key.Enter makes it here from EditCardDialog - ignore it
                if (args.KeyEvent.Key == Key.Enter && typed.Length > 0)
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

    public class CardNameValidator : ITextValidateProvider
    {
        private IAPI_Connection _api;
        private List<string> _cardNames;

        public string? SelectedCard { get; protected set; }

        public CardNameValidator(string setCode, IAPI_Connection api)
        {
            _cardNames = new();
            _api = api;

            RefreshNames(setCode);
        }

        public async void RefreshNames(string setCode)
        {
            _cardNames.Clear();

            List<XCardData> cards = await _api.GetCardsInSet(setCode);

            foreach (XCardData card in cards)
            {
                string name = card["name"].ToString() ?? "";
                if (!_cardNames.Contains(name))
                    _cardNames.Add(name);
            }
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
            set => InsertWord(value.ToString()!);
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
            return Math.Min(Math.Max(pos, 0) + 1, _typed.Length);
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

        private void InsertWord(string newStr)
        {
            if (newStr.Length == 0)
                _typed.Clear();

            int index = 0;
            while (index < newStr.Length)
            {
                if (InsertAt(newStr[index], index))
                    index++;
                else
                    break;
            }
        }
    }
}