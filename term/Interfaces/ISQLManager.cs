namespace MTG_CLI
{
    public interface ISQL_Connection
    {
        public ISQL_Connection Query(MTGQuery query);
        public ISQL_Connection WithParam(string param, string value);
        public ISQL_Connection WithParam(string param, long value);
        public ISQL_Connection WithParam(string param, int value);
        public ISQL_Connection WithFilters(FilterSettings filterSettings);
        public int Execute();
        public T? ExecuteScalar<T>();
        public void OpenToRead();
        public bool IsReady();
        public bool ReadNext();
        public T ReadValue<T>(string fieldName, T fallback);
        public void Close();
    }
}