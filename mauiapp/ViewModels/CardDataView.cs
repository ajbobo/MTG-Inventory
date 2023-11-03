using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace mauiapp;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<CardData> cardList;

    [ObservableProperty]
    ObservableCollection<MTG_Set> setList;

    [ObservableProperty]
    public MTG_Set selectedSet;

    public MainPageViewModel()
    {
        CardList = new ObservableCollection<CardData>();
        SetList = new ObservableCollection<MTG_Set>();
    }
}