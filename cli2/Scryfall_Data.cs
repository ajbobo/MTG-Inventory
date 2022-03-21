namespace Scryfall
{
    public class SetListResponse
    {
        // There's more in the response, but this is all I care about
        public List<Set> data { get; set; } = new();
    }

    public class Set
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "<unknown>";
        public SetType Set_Type { get; set; } = SetType.NONE;
        public bool Digital { get; set; } = false;
        public string Block_Code { get; set; } = "";
        public string Parent_Set_Code { get; set; } = "";

        override public string ToString()
        {
            return Name;
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
        public bool has_more { get; set; } = false;
        public List<Card> data { get; set; } = new();
    }

    public class Card
    {
        // There are tons of other fields returned, these are just the ones I need
        //     See https://scryfall.com/docs/api/cards
        public List<string> color_identity { get; set; } = new();
        public string mana_cost { get; set; } = "";
        public string name { get; set; } = "<unknown>";
        public string rarity { get; set; } = "";
        public string collector_number { get; set; } = "0";
        // I want set information, but "set" is a C# word, so I need to do more work to read those - FINISH ME
        // public string? set_name {get; set;}
        // public string? set {get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}