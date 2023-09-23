namespace mtg_api;

[TestClass]
public class TestTimedCache
{
    [TestMethod]
    public void TestSimpleCache()
    {
        var cache = new TimedCache<int>("testEntry", 3);
        cache.Put("val", 12);

        Assert.IsTrue(cache.Contains("val"));

        int time = 5;
        Console.WriteLine("Waiting for {0} seconds", time);
        Thread.Sleep(time * 1000);

        Assert.IsFalse(cache.Contains("val"));
    }
}