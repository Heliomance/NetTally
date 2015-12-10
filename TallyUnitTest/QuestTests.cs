﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetTally.Tests
{
    [TestClass]
    public class QuestTests
    {
        static readonly List<string> propertiesRaised = new List<string>();
        IQuest a;

        [TestInitialize()]
        public void Initialize()
        {
            a = new Quest();
        }


        #region Constructor
        [TestMethod]
        public void TestDefaultObject()
        {
            Assert.AreEqual(Quest.NewThreadEntry, a.ThreadName);
            Assert.AreEqual("fake-thread.00000", a.DisplayName);
            Assert.AreEqual(0, a.PostsPerPage);
            Assert.AreEqual(1, a.StartPost);
            Assert.AreEqual(0, a.EndPost);
            Assert.AreEqual(true, a.ReadToEndOfThread);
            Assert.AreEqual(false, a.CheckForLastThreadmark);
            Assert.AreEqual(false, a.AllowRankedVotes);
            Assert.AreEqual(0, a.ThreadmarkPost);
            Assert.AreEqual(PartitionMode.None, a.PartitionMode);
            Assert.IsNull(a.ForumAdapter);

            Assert.AreEqual(Quest.NewThreadEntry, a.ToString());
        }

        [TestMethod]
        public void TestToString()
        {
            a.DisplayName = "Test Display";
            Assert.AreEqual("Test Display", a.ToString());
            // If display name is empty, fall back to the thread name.  Since we haven't specified, it's the NewThreadEntry value.
            a.DisplayName = "";
            Assert.AreEqual(Quest.NewThreadEntry, a.ToString());
        }
        #endregion

        #region Display Name and Thread Name
        [TestMethod]
        public void TestDisplayName()
        {
            Assert.AreEqual("fake-thread.00000", a.DisplayName);

            a.DisplayName = "testing-thread";
            Assert.AreEqual("testing-thread", a.DisplayName);

            a.DisplayName = "";
            Assert.AreEqual("fake-thread.00000", a.DisplayName);

            a.ThreadName = "http://forums.sufficientvelocity.com/";
            Assert.AreEqual("forums.sufficientvelocity.com", a.DisplayName);
            a.DisplayName = "/";
            Assert.AreEqual("/", a.DisplayName);

            // Check that unicode control and formatting characters are removed
            a.DisplayName = "MonkeyPhone";
            Assert.AreEqual("MonkeyPhone", a.DisplayName);
            a.DisplayName = "Monkey\u200bPhone";
            Assert.AreEqual("MonkeyPhone", a.DisplayName);

        }

        public void TestCleanURL()
        {
            // Largely tested via ThreadName and SiteName
        }

        [TestMethod]
        public void TestThreadName()
        {
            a.ThreadName = "http://forums.sufficientvelocity.com/";
            Assert.AreEqual("http://forums.sufficientvelocity.com/", a.ThreadName);
            a.ThreadName = "http://forums.sufficientvelocity.com/threads/renascence-a-homura-quest.10402/";
            Assert.AreEqual("http://forums.sufficientvelocity.com/threads/renascence-a-homura-quest.10402/", a.ThreadName);
            a.ThreadName = "http://forums.sufficientvelocity.com/threads/renascence-a-homura-quest.10402/page-221";
            Assert.AreEqual("http://forums.sufficientvelocity.com/threads/renascence-a-homura-quest.10402/", a.ThreadName);
            a.ThreadName = "http://www.fandompost.com/oldforums/showthread.php?39239-Yurikuma-Arashi-Discussion-Thread&p=288335#post288335";
            Assert.AreEqual("http://www.fandompost.com/oldforums/showthread.php?39239-Yurikuma-Arashi-Discussion-Thread", a.ThreadName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullThreadName()
        {
            a.ThreadName = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestEmptyThreadName()
        {
            a.ThreadName = "";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullDisplayName()
        {
            a.DisplayName = null;
        }

        #endregion

        #region StartPost
        [TestMethod]
        public void TestSetStart()
        {
            int testPost = 448;

            a.StartPost = testPost;

            Assert.AreEqual(testPost, a.StartPost);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSetStartZero()
        {
            int testPost = 0;

            a.StartPost = testPost;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSetStartNegative()
        {
            int testPost = -20;

            a.StartPost = testPost;
        }
        #endregion

        #region EndPost
        [TestMethod]
        public void TestSetEnd()
        {
            int testPost = 448;

            a.EndPost = testPost;

            Assert.AreEqual(testPost, a.EndPost);
        }

        [TestMethod]
        public void TestSetEndZero()
        {
            int testPost = 0;

            a.EndPost = testPost;

            Assert.AreEqual(testPost, a.EndPost);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSetEndNegative()
        {
            int testPost = -20;

            a.EndPost = testPost;
        }
        #endregion

        #region Flags
        [TestMethod]
        public void TestReadToEndOfThread()
        {
            a.EndPost = 1;
            Assert.AreEqual(false, a.ReadToEndOfThread);

            a.EndPost = 1000000;
            Assert.AreEqual(false, a.ReadToEndOfThread);

            a.EndPost = 0;
            Assert.AreEqual(true, a.ReadToEndOfThread);

            a.EndPost = Int32.MaxValue;
            Assert.AreEqual(false, a.ReadToEndOfThread);

            a.ThreadmarkPost = 517;
            Assert.AreEqual(true, a.ReadToEndOfThread);
        }

        [TestMethod]
        public void TestCheckForLastThreadmark()
        {
            a.CheckForLastThreadmark = true;
            Assert.AreEqual(true, a.CheckForLastThreadmark);
        }
        #endregion

        #region ForumAdapters
        [TestMethod]
        public void TestForumAdapters()
        {
            //var adapter = a.GetForumAdapter();
            //Assert.IsInstanceOfType(adapter, typeof(XenForoAdapter));

            //a.ThreadName = "http://forums.sufficientvelocity.com/";
            //var adapterTask = a.GetForumAdapterAsync(System.Threading.CancellationToken.None);
            //adapter = adapterTask.Result;
            //Assert.IsInstanceOfType(adapter, typeof(XenForoAdapter));
        }
        #endregion

        #region Events
        [TestMethod]
        public void TestEventRaising()
        {
            INotifyPropertyChanged pc = a as INotifyPropertyChanged;

            Assert.IsNotNull(pc);

            pc.PropertyChanged += A_PropertyChanged;

            propertiesRaised.Clear();

            a.StartPost = 10;
            Assert.IsTrue(propertiesRaised.Contains("StartPost"));

            propertiesRaised.Clear();

            a.EndPost = 20;
            Assert.IsTrue(propertiesRaised.Contains("EndPost"));

            propertiesRaised.Clear();

            a.DisplayName = "Display";
            Assert.IsTrue(propertiesRaised.Contains("DisplayName"));

            propertiesRaised.Clear();

            a.ThreadName = "http://www.example.com";
            Assert.IsTrue(propertiesRaised.Contains("ThreadName"));

            propertiesRaised.Clear();

            a.CheckForLastThreadmark = true;
            Assert.IsTrue(propertiesRaised.Contains("CheckForLastThreadmark"));
        }

        void A_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            propertiesRaised.Add(e.PropertyName);
        }
        #endregion

    }
}