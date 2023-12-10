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

    [JsonIgnoreAttribute]
    private int _tempCount = -1;
    public int TempCount 
    { 
        get => _tempCount >= 0 ? _tempCount : _count;
        set
        {
            _tempCount = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TempCount)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void UpdateCount() 
    {
        Count = TempCount;
    }
}