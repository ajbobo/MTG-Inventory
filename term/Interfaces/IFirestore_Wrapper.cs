namespace MTG_CLI
{
    public interface IFirestore_Wrapper
    {
        public void Connect(string dbName);
        public Task<CardData[]> GetDocumentField(string collection, string document, string field);
        public Task WriteDocumentField(string collection, string document, string field, CardData[] data);
    }
}