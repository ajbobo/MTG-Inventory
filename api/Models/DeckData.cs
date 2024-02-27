using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace mtg_api;

public class DeckData
{
    [Key]
    public string Key { get; set; } = "";

    public string Name { get; set; } = "";

    [Required]
    public List<DeckCardCount> Cards { get; set; } = new List<DeckCardCount>();
}