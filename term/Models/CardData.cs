namespace MTG_CLI;

public class CardData
{
    public MTG_Card? Card { get; set; } = null;
    public List<CardTypeCount>? CTCs { get; set; } = null;
    public int TotalCount {get; set; } = 0;
}