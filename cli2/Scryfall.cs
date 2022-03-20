namespace Scryfall
{
    public class SetListResponse
    {
        public List<Set>? data { get; set; }

        public static SetListResponse NONE = new SetListResponse() { data = new List<Set>() };
    }

    public class Set
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public SetType Set_Type { get; set; } = SetType.NONE;
        public bool Digital { get; set; }

        override public string ToString()
        {
            return Name ?? "<unknown>";
        }

        public static Set NONE = new Set();

        public enum SetType
        {
            NONE,
            CORE,
            EXPANSION,
            MASTERS,
            ALCHEMY,
            MASTERPIECE,
            ARSENAL,
            FROM_THE_VAULT,
            SPELLBOOK,
            PREMIUM_DECK,
            DUEL_DECK,
            DRAFT_INNOVATION,
            TREASURE_CHEST,
            COMMANDER,
            PLANECHASE,
            ARCHENEMY,
            VANGUARD,
            FUNNY,
            STARTER,
            BOX,
            PROMO,
            TOKEN,
            MEMORABILIA
        }
    }

    public class CardListResponse
    {
        public bool? has_more { get; set; }
        public List<Card>? data { get; set; }

        public static CardListResponse NONE = new CardListResponse() { has_more = false, data = new List<Card>() };
    }

    public class Card
    {
        // There are tons of other fields returned, these are just the ones I need
        //     See https://scryfall.com/docs/api/cards
        public List<string>? color_identity { get; set; }
        public string? mana_cost { get; set; }
        public string? name { get; set; }
        public string? rarity { get; set; }
        public string? collector_number { get; set; }
        // I want set information, but "set" is a C# word, so I need to do more work to read those - FINISH ME
        // public string? set_name {get; set;}
        // public string? set {get; set; }

        public override string ToString()
        {
            return name ?? "<unknown>";
        }

        public static Card NONE = new Card() { collector_number = "", mana_cost = "", name = "<unknown>", rarity = "none", color_identity = new List<string>() };
    }
}