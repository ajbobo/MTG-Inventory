using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace mtg_api;

public class MTG_Card
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string CastingCost { get; set; } = "";

    [Required]
    public string Rarity { get; set; } = "";

    [Required]
    public string TypeLine { get; set; } = "";

    [Required]
    public string FrontText { get; set; } = "";

    [Required]
    public decimal Price { get; set; } = 0;

    [Required]
    public decimal PriceFoil { get; set; } = 0;

    [Required]
    public string SetCode { get; set; } = "";

    [Required]
    public string CollectorNumber { get; set; } = "";

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Can I make SetCode+CollectorNumber the key?
    public Guid? Uuid { get; set; }
}