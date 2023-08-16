using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace mtg_api;

public class MTG_Set
{
    [Key, Required]
    public string SetCode { get; set; } = "";

    [Required]
    public string SetName { get; set; } = "";

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Uuid { get; set; }
}