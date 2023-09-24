using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mtg_api;

public class CollectionEntry
{
    public MTG_Card Card { get; set; } = new MTG_Card();
    public List<CardTypeCount> CTCs { get; set; } = new List<CardTypeCount>();
    public int Count { get; set; } = 0;
}