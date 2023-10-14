namespace MTG_CLI
{
    public interface IFirestoreDB_Wrapper
    {
        public void Connect(string dbName);
        public Task<XCardData[]> GetDocumentField(string collection, string document, string field);
        public Task WriteDocumentField(string collection, string document, string field, XCardData[] data);
    }
}