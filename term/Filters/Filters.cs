namespace MTG_CLI
{
    public abstract class Filter
    {
        public string DisplayName { protected set; get; } = "";

        override public string ToString()
        {
            return DisplayName;
        }
    }

    public class RarityFilter : Filter
    {
        public static RarityFilter COMMON { get; } = new() { DisplayName = "Common" };
        public static RarityFilter UNCOMMON { get; } = new() { DisplayName = "Uncommon" };
        public static RarityFilter RARE { get; } = new() { DisplayName = "Rare" };
        public static RarityFilter MYTHIC { get; } = new() { DisplayName = "Mythic" };

        public static Filter[] GetAllValues()
        {
            return new[] { COMMON, UNCOMMON, RARE, MYTHIC };
        }
    }

    public class CountFilter : Filter
    {
        public int Min { private set; get; }
        public int Max { private set; get; }

        public static CountFilter CNT_ZERO { get; } = new() { DisplayName = "0", Min = 0, Max = 0 };
        public static CountFilter CNT_ONE_PLUS { get; } = new() { DisplayName = "1+", Min = 1, Max = 1000 };
        public static CountFilter CNT_FOUR_PLUS { get; } = new() { DisplayName = "4+", Min = 4, Max = 1000 };
        public static CountFilter CNT_LESS_THAN_FOUR { get; } = new() { DisplayName = "<4", Min = 0, Max = 3 };
        public static CountFilter CNT_ALL { get; } = new() { DisplayName = "<all cards>" };

        public static Filter[] GetAllValues()
        {
            return new[] { CNT_ALL, CNT_ZERO, CNT_ONE_PLUS, CNT_LESS_THAN_FOUR, CNT_FOUR_PLUS };
        }
    }

    public class ColorFilter : Filter
    {
        public char Color { private set; get; }

        public static ColorFilter WHITE { get; } = new() { DisplayName = "White (W)", Color = 'W' };
        public static ColorFilter BLUE { get; } = new() { DisplayName = "Blue (U)", Color = 'U' };
        public static ColorFilter BLACK { get; } = new() { DisplayName = "Black (B)", Color = 'B' };
        public static ColorFilter RED { get; } = new() { DisplayName = "Red (R)", Color = 'R' };
        public static ColorFilter GREEN { get; } = new() { DisplayName = "Green (G)", Color = 'G' };
        public static ColorFilter COLORLESS { get; } = new() { DisplayName = "Colorless", Color = 'X' };

        public static Filter[] GetAllValues()
        {
            return new[] { WHITE, BLUE, BLACK, RED, GREEN, COLORLESS };
        }
    }
}