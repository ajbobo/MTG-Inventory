using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace mauiapp.ViewModels;

[QueryProperty("CardData", "cardData")]
public partial class CTCPageViewModel : ObservableObject
{
    [ObservableProperty]
    CardData cardData;

    [RelayCommand]
    async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
