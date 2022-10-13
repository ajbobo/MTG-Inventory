namespace MTG_CLI
{
    public interface ISQLManager
    {
        public ISQLManager Query(InternalQuery query);
        public ISQLManager WithParam(string param, string value);
        public ISQLManager WithParam(string param, long value);
        public ISQLManager WithParam(string param, int value);
        public ISQLManager WithFilters(FilterSettings filterSettings);
        public int Execute();
        public T? ExecuteScalar<T>();
        public void Read();
        public bool HasReader();
        public bool ReadNext();
        public T ReadValue<T>(string fieldName, T fallback);
        public void Close();
    }
}