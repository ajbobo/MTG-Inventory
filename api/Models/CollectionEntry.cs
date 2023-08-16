using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mtg_api;

public class CollectionEntry
{
    [Required]
    public string SetCode { get; set; } = "";

    [Required]
    public string CollectorNumber { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    // [Required]
    // public JObject Attrs { get; set; }

    [Required]
    public int Count { get; set; } = 0;

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Uuid { get; set; }
}