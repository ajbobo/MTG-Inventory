using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MTG_CLI
{
    [TestClass]
    public class TestFilters
    {
        [TestMethod]
        public void TestRarity()
        {
            Filter[] all = RarityFilter.GetAllValues();
            Assert.AreEqual(4, all.Length);
            Assert.IsTrue(ConfirmDifferences(all));

            List<Filter> listAll = all.ToList();
            Assert.IsTrue(all.Contains(RarityFilter.COMMON));
            Assert.IsTrue(all.Contains(RarityFilter.UNCOMMON));
            Assert.IsTrue(all.Contains(RarityFilter.RARE));
            Assert.IsTrue(all.Contains(RarityFilter.MYTHIC));
        }

        [TestMethod]
        public void TestCount()
        {
            Filter[] all = CountFilter.GetAllValues();
            Assert.AreEqual(5, all.Length);
            Assert.IsTrue(ConfirmDifferences(all));

            List<Filter> listAll = all.ToList();
            Assert.IsTrue(all.Contains(CountFilter.CNT_ALL));
            Assert.IsTrue(all.Contains(CountFilter.CNT_FOUR_PLUS));
            Assert.IsTrue(all.Contains(CountFilter.CNT_LESS_THAN_FOUR));
            Assert.IsTrue(all.Contains(CountFilter.CNT_ONE_PLUS));
            Assert.IsTrue(all.Contains(CountFilter.CNT_ZERO));
        }

        [TestMethod]
        public void TestColor()
        {
            Filter[] all = ColorFilter.GetAllValues();
            Assert.AreEqual(6, all.Length);
            Assert.IsTrue(ConfirmDifferences(all));

            List<Filter> listAll = all.ToList();
            Assert.IsTrue(all.Contains(ColorFilter.WHITE));
            Assert.IsTrue(all.Contains(ColorFilter.BLUE));
            Assert.IsTrue(all.Contains(ColorFilter.BLACK));
            Assert.IsTrue(all.Contains(ColorFilter.RED));
            Assert.IsTrue(all.Contains(ColorFilter.GREEEN));
            Assert.IsTrue(all.Contains(ColorFilter.COLORLESS));
        }

        private bool ConfirmDifferences(Filter[] filters)
        {
            // No filters should have the same name
            for (int x = 0; x < filters.Length - 1; x++)
            {
                for (int y = x + 1; y < filters.Length; y++)
                {
                    string name1 = filters[x].ToString();
                    string name2 = filters[y].ToString();
                    if (name1.Equals(name2))
                        return false;
                }
            }
            return true;
        }
    }
}