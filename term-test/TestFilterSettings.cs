using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MTG_CLI
{
    [TestClass]
    public class TestFilterSettings
    {
        [TestMethod]
        public void TestNoFilters()
        {
            FilterSettings fs = new();
            CheckDefaults(fs, true, true, true);
        }

        [TestMethod]
        public void TestSingleColor()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(ColorFilter.RED, true);
            CheckColors(fs, new[] { ColorFilter.RED }, "R");

            fs.ToggleFilter(ColorFilter.RED, false);
            fs.ToggleFilter(ColorFilter.WHITE, true);
            CheckColors(fs, new[] { ColorFilter.WHITE }, "W");

            fs.ToggleFilter(ColorFilter.WHITE, false);
            fs.ToggleFilter(ColorFilter.BLACK, true);
            CheckColors(fs, new[] { ColorFilter.BLACK }, "B");

            fs.ToggleFilter(ColorFilter.BLACK, false);
            fs.ToggleFilter(ColorFilter.BLUE, true);
            CheckColors(fs, new[] { ColorFilter.BLUE }, "U");

            fs.ToggleFilter(ColorFilter.BLUE, false);
            fs.ToggleFilter(ColorFilter.GREEN, true);
            CheckColors(fs, new[] { ColorFilter.GREEN }, "G");

            fs.ToggleFilter(ColorFilter.GREEN, false);
            fs.ToggleFilter(ColorFilter.COLORLESS, true);
            CheckColors(fs, new[] { ColorFilter.COLORLESS }, "X");

            CheckDefaults(fs, false, true, true);
        }

        [TestMethod]
        public void TestMultipleColors()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(ColorFilter.RED, true);
            fs.ToggleFilter(ColorFilter.WHITE, true);
            CheckColors(fs, new[] { ColorFilter.RED, ColorFilter.WHITE }, "RW");

            fs.ToggleFilter(ColorFilter.BLACK, true);
            CheckColors(fs, new[] { ColorFilter.RED, ColorFilter.WHITE, ColorFilter.BLACK }, "RWB");

            fs.ToggleFilter(ColorFilter.RED, false);
            CheckColors(fs, new[] { ColorFilter.WHITE, ColorFilter.BLACK }, "WB");

            CheckDefaults(fs, false, true, true);
        }

        private static void CheckColors(FilterSettings fs, ColorFilter[] filters, string expected)
        {
            string colors = fs.GetColors();
            foreach (ColorFilter filter in filters)
                Assert.IsTrue(fs.HasFilter(filter));
            Assert.AreEqual(expected, colors);
        }

        [TestMethod]
        public void TestCount()
        {
            FilterSettings fs = new();

            fs.ToggleFilter(CountFilter.CNT_ALL, true);
            CheckCount(fs, 0, 1000);

            fs.ToggleFilter(CountFilter.CNT_FOUR_PLUS, true);
            Assert.IsFalse(fs.HasFilter(CountFilter.CNT_ALL));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_FOUR_PLUS));
            CheckCount(fs, 4, 1000);

            fs.ToggleFilter(CountFilter.CNT_LESS_THAN_FOUR, true);
            Assert.IsFalse(fs.HasFilter(CountFilter.CNT_FOUR_PLUS));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_LESS_THAN_FOUR));
            CheckCount(fs, 0, 3);

            fs.ToggleFilter(CountFilter.CNT_ONE_PLUS, true);
            Assert.IsFalse(fs.HasFilter(CountFilter.CNT_LESS_THAN_FOUR));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_ONE_PLUS));
            CheckCount(fs, 1, 1000);

            fs.ToggleFilter(CountFilter.CNT_ZERO, true);
            Assert.IsFalse(fs.HasFilter(CountFilter.CNT_ONE_PLUS));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_ZERO));
            CheckCount(fs, 0, 0);

            CheckDefaults(fs, true, true, false);
        }

        private void CheckCount(FilterSettings fs, int expectedMin, int expectedMax)
        {
            Assert.AreEqual(expectedMin, fs.GetMinCount());
            Assert.AreEqual(expectedMax, fs.GetMaxCount());
        }

        [TestMethod]
        public void TestSingleRarity()
        {
            FilterSettings fs = new();

            fs.ToggleFilter(RarityFilter.COMMON, true);
            CheckRarities(fs, new[] { RarityFilter.COMMON }, new[] { "Common" });

            fs.ToggleFilter(RarityFilter.COMMON, false);
            fs.ToggleFilter(RarityFilter.UNCOMMON, true);
            CheckRarities(fs, new[] { RarityFilter.UNCOMMON }, new[] { "Uncommon" });

            fs.ToggleFilter(RarityFilter.UNCOMMON, false);
            fs.ToggleFilter(RarityFilter.RARE, true);
            CheckRarities(fs, new[] { RarityFilter.RARE }, new[] { "Rare" });

            fs.ToggleFilter(RarityFilter.RARE, false);
            fs.ToggleFilter(RarityFilter.MYTHIC, true);
            CheckRarities(fs, new[] { RarityFilter.MYTHIC }, new[] { "Mythic" });

            CheckDefaults(fs, true, false, true);
        }

        [TestMethod]
        public void TestMultipleRarities()
        {
            FilterSettings fs = new();

            fs.ToggleFilter(RarityFilter.COMMON, true);
            fs.ToggleFilter(RarityFilter.RARE, true);
            CheckRarities(fs, new[] { RarityFilter.COMMON, RarityFilter.RARE }, new[] { "Common", "Rare" });

            fs.ToggleFilter(RarityFilter.COMMON, false);
            CheckRarities(fs, new[] { RarityFilter.RARE }, new[] { "Rare" });

            fs.ToggleFilter(RarityFilter.MYTHIC, true);
            CheckRarities(fs, new[] { RarityFilter.RARE, RarityFilter.MYTHIC }, new[] { "Rare", "Mythic" });

            CheckDefaults(fs, true, false, true);
        }

        private void CheckRarities(FilterSettings fs, RarityFilter[] filters, string[] expected)
        {
            string[] colors = fs.GetRarities();
            foreach (RarityFilter filter in filters)
                Assert.IsTrue(fs.HasFilter(filter));
            for (int x = 0; x < expected.Length; x++)
                Assert.AreEqual(expected[x], colors[x]);
        }

        [TestMethod]
        public void TestCountThenColor()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(CountFilter.CNT_ONE_PLUS, true);
            fs.ToggleFilter(ColorFilter.RED, true);

            Assert.IsTrue(fs.HasFilter(ColorFilter.RED));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_ONE_PLUS));
            CheckDefaults(fs, false, true, false);
        }

        [TestMethod]
        public void TestColorThenCount()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(ColorFilter.RED, true);
            fs.ToggleFilter(CountFilter.CNT_ONE_PLUS, true);

            Assert.IsTrue(fs.HasFilter(ColorFilter.RED));
            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_ONE_PLUS));
            CheckDefaults(fs, false, true, false);
        }

        [TestMethod]
        public void TestColorThenRarity()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(ColorFilter.RED, true);
            fs.ToggleFilter(RarityFilter.RARE, true);

            Assert.IsTrue(fs.HasFilter(ColorFilter.RED));
            Assert.IsTrue(fs.HasFilter(RarityFilter.RARE));
            CheckDefaults(fs, false, false, true);
        }

        [TestMethod]
        public void TestRarityThenColor()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(RarityFilter.RARE, true);
            fs.ToggleFilter(ColorFilter.RED, true);

            Assert.IsTrue(fs.HasFilter(ColorFilter.RED));
            Assert.IsTrue(fs.HasFilter(RarityFilter.RARE));
            CheckDefaults(fs, false, false, true);
        }

        [TestMethod]
        public void TestRarityThenCount()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(RarityFilter.RARE, true);
            fs.ToggleFilter(CountFilter.CNT_FOUR_PLUS, true);

            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_FOUR_PLUS));
            Assert.IsTrue(fs.HasFilter(RarityFilter.RARE));
            CheckDefaults(fs, true, false, false);
        }

        [TestMethod]
        public void TestCountThenRarity()
        {
            FilterSettings fs = new();
            fs.ToggleFilter(CountFilter.CNT_FOUR_PLUS, true);
            fs.ToggleFilter(RarityFilter.RARE, true);

            Assert.IsTrue(fs.HasFilter(CountFilter.CNT_FOUR_PLUS));
            Assert.IsTrue(fs.HasFilter(RarityFilter.RARE));
            CheckDefaults(fs, true, false, false);
        }

        private void CheckDefaults(FilterSettings fs, bool checkColor, bool checkRarity, bool checkCount)
        {
            if (checkColor)
            {
                Assert.AreEqual("", fs.GetColors());
                foreach (Filter filter in ColorFilter.GetAllValues())
                {
                    Assert.IsFalse(fs.HasFilter(filter));
                }
            }

            if (checkRarity)
            {
                Assert.AreEqual(4, fs.GetRarities().Length);
                foreach (Filter filter in RarityFilter.GetAllValues())
                {
                    Assert.IsFalse(fs.HasFilter(filter));
                }
            }

            if (checkCount)
            {
                Assert.AreEqual(0, fs.GetMinCount());
                Assert.AreEqual(1000, fs.GetMaxCount());
                foreach (Filter filter in CountFilter.GetAllValues())
                {
                    Assert.IsFalse(fs.HasFilter(filter));
                }
            }
        }
    }
}