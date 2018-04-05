using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reprocessor.Tests
{
    [TestClass]
    public class SdeTests
    {
        private static SdeInteraction sde;
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            sde = new SdeInteraction();
        }

        [TestMethod]
        public void GetTheForgeRegionId()
        {
            Assert.AreEqual(10000002, sde.RegionIdFromName("The Forge"));
        }

        [TestMethod]
        public void GetAllRegions()
        {
            var regions = sde.GetAllRegions();

            Assert.IsTrue(regions.Count > 0);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        { 
            sde.Dispose();
        }
    }
}
