namespace mauiapp;

public class MTG_Set
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconUrl { get; set; } = "";

    public override string ToString()
    {
        return $"({Code}) {Name}";
    }
}