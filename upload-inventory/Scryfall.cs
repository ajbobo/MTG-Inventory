using System.Collections.Generic;

namespace Scryfall
{
    class SetResponse
    {
        public string has_more { get; set; }
        public List<SetData> data { get; set; }
    }

    class SetData
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Released_At { get; set; }
        public string Icon_Svg_Uri { get; set; }
        public Type Set_Type { get; set; }
        public bool Digital { get; set; }
        
        public enum Type
        {
            CORE,
            EXPANSION,
            MASTERS,
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
            MEMORABILIA,
        }
    }
}
