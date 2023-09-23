namespace mtg_api;

public interface ITimedCache<T>
{
    public T Get(string key);
    public bool Contains(string key);
    public void Put(string key, T obj);
}