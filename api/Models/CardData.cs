namespace mtg_api;

public class CardData
{
    public MTG_Card? Card { get; set; } = null;
    public List<CardTypeCount>? CTCs { get; set; } = null;
    public int TotalCount { get; set; } = 0;
    public string DecoratedCount { get; set; } = "0";
}