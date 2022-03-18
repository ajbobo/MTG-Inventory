namespace MTG_CLI
{
    public delegate void CommandFunction();

    public sealed class Commands
    {
        private static Dictionary<ConsoleKey, CommandFunction> KeyMap = new Dictionary<ConsoleKey, CommandFunction>();
        private static void NoOp() { }

        public static void MapKeyToFunction(ConsoleKey key, CommandFunction func)
        {
            KeyMap[key] = func;
        }

        public static void ClearFunctions(params ConsoleKey[] keys)
        {
            foreach (ConsoleKey key in keys)
            KeyMap[key] = NoOp;
        }

        public static void ExecuteCommand(ConsoleKey key)
        {
            if (KeyMap.ContainsKey(key))
                KeyMap[key]();
        }
    }
}