using mtg_api;
using System.Runtime.Caching;

namespace api_test;

[TestClass]
public class Test_CacheController
{
    [TestInitialize]
    public void SetupCache()
    {
        var cache = MemoryCache.Default;

        cache.Add("one", 1, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
        cache.Add("two", 2, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
        cache.Add("three", 3, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
        cache.Add("four", 4, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
    }

    [TestMethod]
    public void TestCacheDescriptions()
    {
        var cache = MemoryCache.Default;

        var controller = new CacheController(cache);
        var res = controller.GetCacheDescriptions();

        Assert.IsNotNull(res);
        Assert.AreEqual(4, res.Count); // The number of cache entries is set, but the order is undefined
    }

    [TestMethod]
    public void TestDeleteCaches()
    {
        var cache = MemoryCache.Default;

        var controller = new CacheController(cache);
        controller.DeleteCaches();

        var res = controller.GetCacheDescriptions();
        Assert.AreEqual(0, res.Count);
    }
}