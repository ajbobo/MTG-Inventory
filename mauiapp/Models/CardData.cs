namespace mauiapp;

public class CardData
{
    public MTG_Card Card { get; set; } = null;
    public List<CardTypeCount> CTCs { get; set; } = new();
    public int TotalCount { get; set; } = 0;
}