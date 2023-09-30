namespace mtg_api;

public class CardResult
{
    public MTG_Card? Card { get; set; } = null;
    public List<CardTypeCount>? CTCs { get; set; } = null;
}