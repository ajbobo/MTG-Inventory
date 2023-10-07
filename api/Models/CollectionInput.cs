using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mtg_api;

public class CollectionInput
{
    public string SetCode { get; set; } = "";

    public string CollectorNumber { get; set; } = "";

    [Key]
    public string Key { get; set; } = "";

    public string Name { get; set; } = "";

    [Required]
    public List<CardTypeCount> CTCs { get; set; } = new List<CardTypeCount>();

    public int TotalCount { get; set; } = 0;
}