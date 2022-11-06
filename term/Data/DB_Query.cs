using term;

namespace MTG_CLI
{
    public class DB_Query
    {
        // I don't want to create a new object every time one of these is needed, so I'm going to keep a cache of them, creating them as they are needed
        private static Dictionary<int, DB_Query> _queries = new();

        public string Query { private set; get; }

        private DB_Query(string value)
        {
            Query = value;
        }

        private static DB_Query GetOrInsert(int id, string query)
        {
            if (_queries.ContainsKey(id))
                return _queries[id];

            DB_Query val = new DB_Query(query);
            _queries[id] = val;
            return val;
        }

        // The queries are defined in individual .sql files, and converted to code via Sylvan.BuildTools.Resources
        //    See https://github.com/MarkPflug/Sylvan.BuildTools.Resources
        // The id number assigned to each query MUST be unique
        public static DB_Query CREATE_SET_TABLE => GetOrInsert(1, MTGQueries.CreateSetTable);
        public static DB_Query INSERT_SET => GetOrInsert(2, MTGQueries.InsertSet);
        public static DB_Query GET_ALL_SETS => GetOrInsert(3, MTGQueries.GetAllSets);
        public static DB_Query GET_SET_NAME => GetOrInsert(4, MTGQueries.GetSetName);
        public static DB_Query GET_SET_CODE => GetOrInsert(5, MTGQueries.GetSetCode);
        public static DB_Query CREATE_CARD_TABLE => GetOrInsert(6, MTGQueries.CreateCardTable);
        public static DB_Query INSERT_CARD => GetOrInsert(7, MTGQueries.InsertCard);
        public static DB_Query GET_SET_CARDS => GetOrInsert(8, MTGQueries.GetSetCards);
        public static DB_Query GET_SINGLE_CARD_COUNT => GetOrInsert(9, MTGQueries.GetSingleCardCount);
        public static DB_Query GET_CARD_DETAILS => GetOrInsert(10, MTGQueries.GetCardDetails);
        public static DB_Query GET_CARD_NAMES => GetOrInsert(11, MTGQueries.GetCardNames);
        public static DB_Query GET_CARD_NUMBER => GetOrInsert(12, MTGQueries.GetCardNumber);
        public static DB_Query CREATE_USER_INVENTORY => GetOrInsert(13, MTGQueries.CreateUserInventory);
        public static DB_Query ADD_TO_USER_INVENTORY => GetOrInsert(14, MTGQueries.AddToUserInventory);
        public static DB_Query GET_USER_INVENTORY => GetOrInsert(15, MTGQueries.GetUserInventory);
        public static DB_Query GET_CARD_CTCS => GetOrInsert(16, MTGQueries.GetCardCTCs);
        public static DB_Query UPDATE_CARD_CTC => GetOrInsert(17, MTGQueries.UpdateCardCTC);
    };
}