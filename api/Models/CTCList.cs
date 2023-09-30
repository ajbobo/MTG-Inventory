using System.ComponentModel.DataAnnotations;

namespace mtg_api;

public class CTCList
{
    [Required]
    public List<CardTypeCount> CTCs { get; set; } = new List<CardTypeCount>();
}