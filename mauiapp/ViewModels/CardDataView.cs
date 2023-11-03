using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace mauiapp;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<CardData> cardList;

    public MainPageViewModel()
    {
        CardList = new ObservableCollection<CardData>();
    }
}