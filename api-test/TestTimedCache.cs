namespace mtg_api;

[TestClass]
public class TestTimedCache
{
    [TestMethod]
    public void TestSimpleCache()
    {
        var cache = new TimedCache<int>(3);
        cache.Put("val", 12);

        Assert.IsTrue(cache.Contains("val"));

        int time = 5;
        Thread.Sleep(time * 1000);

        Assert.IsFalse(cache.Contains("val"));
    }

    [TestMethod]
    public void TestCacheRefresh()
    {
        var cache = new TimedCache<int>(3);
        cache.OnRefresh += (key) => { return 13; };
        cache.Put("val", 12);

        Assert.IsTrue(cache.Contains("val"));
        Assert.IsTrue(cache.Get("val") == 12);

        int time = 5;
        Thread.Sleep(time * 1000);

        Assert.IsFalse(cache.Contains("val"));

        int val = cache.Get("val"); // This will refresh the lost value

        Assert.IsTrue(val == 13);
    }

}