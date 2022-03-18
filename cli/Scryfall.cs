namespace Scryfall
{
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
    }

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