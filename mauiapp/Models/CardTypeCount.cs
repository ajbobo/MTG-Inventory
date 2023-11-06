using Newtonsoft.Json;
using System.ComponentModel;

namespace mauiapp;

public class CardTypeCount : INotifyPropertyChanged
{
    private string _cardType;
    public string CardType
    {
        get => _cardType;
        set
        {
            _cardType = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardType)));
        }
    }


    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}