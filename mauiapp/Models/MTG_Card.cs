namespace mauiapp;

public class MTG_Card
{
    public string Name { get; set; } = "";
    public string CastingCost { get; set; } = "";
    public string ColorIdentity {get; set; } = "";
    public string Rarity { get; set; } = "";
    public string TypeLine { get; set; } = "";
    public string FrontText { get; set; } = "";
    public decimal Price { get; set; } = 0;
    public decimal PriceFoil { get; set; } = 0;
    public string SetCode { get; set; } = "";
    public string CollectorNumber { get; set; } = "";
    public string FrontImageUrl {get; set; } = "";
    public string BackImageUrl {get; set; } = "";
}