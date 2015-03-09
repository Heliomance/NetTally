﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTally.Tests
{
    [TestClass()]
    public class WebPageProviderTests
    {
        static WebPageProvider pageProvider;
        static PrivateObject privateWeb;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            pageProvider = new WebPageProvider();
            privateWeb = new PrivateObject(pageProvider);
        }


        [TestMethod()]
        public void ClearPageCacheTest()
        {
            pageProvider.ClearPageCache();

            Dictionary<string, CachedPage> accessPageCache = (Dictionary<string, CachedPage>)privateWeb.GetField("pageCache");
            Assert.IsTrue(accessPageCache.Count == 0);

            Dictionary<string, int> accessLoadedPages = (Dictionary<string, int>)privateWeb.GetField("lastPageLoaded");
            Assert.IsTrue(accessLoadedPages.Count == 0);
        }

        [TestMethod()]
        public void LoadPagesTest()
        {
            Assert.Fail();
        }
    }
}