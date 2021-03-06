﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTally.Utility;

namespace NetTally.Tests
{
    [TestClass]
    public class VoteCounterTests
    {
        #region Setup
        static VoteCounterImpl voteCounterRaw;
        static IQuest sampleQuest;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            StringUtility.InitStringComparers(Platform.UnicodeHashFunction.HashFunction);
            sampleQuest = new Quest();
            voteCounterRaw = VoteCounterImpl.Instance;
        }

        [TestInitialize]
        public void Initialize()
        {
            VoteCounter.Instance.Reset();
            VoteCounter.Instance.PostsList.Clear();
        }
        #endregion

        [TestMethod]
        public void ResetTest()
        {
            VoteCounter.Instance.Reset();

            Assert.AreEqual(0, VoteCounter.Instance.GetVotersCollection(VoteType.Vote).Count);
            Assert.AreEqual(0, VoteCounter.Instance.GetVotesCollection(VoteType.Vote).Count);
            Assert.AreEqual(0, VoteCounter.Instance.GetVotersCollection(VoteType.Rank).Count);
            Assert.AreEqual(0, VoteCounter.Instance.GetVotesCollection(VoteType.Rank).Count);

            Assert.AreEqual(0, VoteCounter.Instance.ReferencePlanNames.Count);
            Assert.AreEqual(0, VoteCounter.Instance.ReferencePlans.Count);
            Assert.AreEqual(0, VoteCounter.Instance.ReferenceVoters.Count);
            Assert.AreEqual(0, VoteCounter.Instance.ReferenceVoterPosts.Count);
            Assert.AreEqual(0, VoteCounter.Instance.FutureReferences.Count);

            Assert.AreEqual(0, VoteCounter.Instance.PlanNames.Count);
            Assert.AreEqual("", VoteCounter.Instance.Title);
        }

        #region Get vote collections
        [TestMethod]
        public void GetVotesCollectionTest1()
        {
            Assert.AreEqual(voteCounterRaw.VotesWithSupporters, VoteCounter.Instance.GetVotesCollection(VoteType.Vote));
        }

        [TestMethod]
        public void GetVotesCollectionTest2()
        {
            Assert.AreEqual(voteCounterRaw.VotesWithSupporters, VoteCounter.Instance.GetVotesCollection(VoteType.Plan));
        }

        [TestMethod]
        public void GetVotesCollectionTest3()
        {
            Assert.AreEqual(voteCounterRaw.RankedVotesWithSupporters, VoteCounter.Instance.GetVotesCollection(VoteType.Rank));
        }

        [TestMethod]
        public void GetVotersCollectionTest1()
        {
            Assert.AreEqual(voteCounterRaw.VoterMessageId, VoteCounter.Instance.GetVotersCollection(VoteType.Vote));
        }

        [TestMethod]
        public void GetVotersCollectionTest2()
        {
            Assert.AreEqual(voteCounterRaw.VoterMessageId, VoteCounter.Instance.GetVotersCollection(VoteType.Plan));
        }

        [TestMethod]
        public void GetVotersCollectionTest3()
        {
            Assert.AreEqual(voteCounterRaw.RankedVoterMessageId, VoteCounter.Instance.GetVotersCollection(VoteType.Rank));
        }
        #endregion

        #region Add Vote param checks
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddVoteParamsTest1()
        {
            VoteCounter.Instance.AddVotes(null, null, null, VoteType.Vote);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddVoteParamsTest2()
        {
            VoteCounter.Instance.AddVotes(new List<string>(), null, null, VoteType.Vote);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddVoteParamsTest3()
        {
            VoteCounter.Instance.AddVotes(new List<string>(), "me", null, VoteType.Vote);
        }

        [TestMethod]
        public void AddVoteParamsTest4()
        {
            VoteCounter.Instance.AddVotes(new List<string>(), "me", "1", VoteType.Vote);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddVoteParamsTest5()
        {
            VoteCounter.Instance.AddVotes(new List<string>(), "", "1", VoteType.Vote);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddVoteParamsTest6()
        {
            VoteCounter.Instance.AddVotes(new List<string>(), "me", "", VoteType.Vote);
        }
        #endregion

        #region Add Votes
        [TestMethod]
        public void AddVoteTypeVoteTest()
        {
            string voteLine = "[x] First test";
            string voter = "me";
            string postId = "1";
            List<string> vote = new List<string> { voteLine };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote, voter, postId, voteType);

            var votes = VoteCounter.Instance.GetVotesCollection(voteType);
            var voters = VoteCounter.Instance.GetVotersCollection(voteType);

            Assert.IsTrue(votes.Keys.Contains(voteLine));
            Assert.IsTrue(votes[voteLine].Contains(voter));

            Assert.IsTrue(voters.ContainsKey(voter));
            Assert.AreEqual(postId, voters[voter]);
        }

        [TestMethod]
        public void AddPlanTypeVoteTest()
        {
            string voteLine = "[x] First test";
            string voter = "me";
            string planname = "◈PlanPlan";
            string postId = "1";
            List<string> vote = new List<string> { voteLine };
            VoteType voteType = VoteType.Plan;

            VoteCounter.Instance.AddVotes(vote, planname, postId, voteType);
            VoteCounter.Instance.AddVotes(vote, voter, postId, VoteType.Vote);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine].Contains(voter));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine].Contains(planname));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter));
            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(planname));
            Assert.AreEqual(postId, VoteCounter.Instance.GetVotersCollection(voteType)[voter]);
            Assert.AreEqual(postId, VoteCounter.Instance.GetVotersCollection(voteType)[planname]);

            Assert.IsTrue(VoteCounter.Instance.HasPlan("PlanPlan"));
        }

        [TestMethod]
        public void AddRankTypeVoteTest()
        {
            string voteLine = "[1] First test";
            string voter = "me";
            string postId = "1";
            List<string> vote = new List<string> { voteLine };
            VoteType voteType = VoteType.Rank;

            VoteCounter.Instance.AddVotes(vote, voter, postId, voteType);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine].Contains(voter));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter));
            Assert.AreEqual(postId, VoteCounter.Instance.GetVotersCollection(voteType)[voter]);
        }

        [TestMethod]
        public void AddVoteMultiTest1()
        {
            string voteLine = "[x] First test";
            string voter1 = "me";
            string postId1 = "1";
            string voter2 = "you";
            string postId2 = "2";
            List<string> vote = new List<string> { voteLine };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote, voter2, postId2, voteType);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine].Contains(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine].Contains(voter2));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter2));
            Assert.AreEqual(postId1, VoteCounter.Instance.GetVotersCollection(voteType)[voter1]);
            Assert.AreEqual(postId2, VoteCounter.Instance.GetVotersCollection(voteType)[voter2]);
        }

        [TestMethod]
        public void AddVoteMultiTest2()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] 『b』First『/b』 test";
            string voter1 = "me";
            string postId1 = "1";
            string voter2 = "you";
            string postId2 = "2";
            List<string> vote1 = new List<string> { voteLine1 };
            List<string> vote2 = new List<string> { voteLine2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter2, postId2, voteType);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine1));
            Assert.IsFalse(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine2));
            Assert.AreEqual(1, VoteCounter.Instance.GetVotesCollection(voteType).Count);
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine1].Contains(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine1].Contains(voter2));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter2));
            Assert.AreEqual(postId1, VoteCounter.Instance.GetVotersCollection(voteType)[voter1]);
            Assert.AreEqual(postId2, VoteCounter.Instance.GetVotersCollection(voteType)[voter2]);
        }

        [TestMethod]
        public void AddVoteReplacementTest1()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] Second test";
            string voter1 = "me";
            string postId1 = "1";
            string postId2 = "2";
            List<string> vote1 = new List<string> { voteLine1 };
            List<string> vote2 = new List<string> { voteLine2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter1, postId2, voteType);

            Assert.IsFalse(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(voteLine2));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[voteLine2].Contains(voter1));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter1));
            Assert.AreEqual(postId2, VoteCounter.Instance.GetVotersCollection(voteType)[voter1]);
        }

        #endregion

        #region Matches
        private void TestMatch(string line1, string line2)
        {
            string voter1 = "me";
            string postId1 = "1";
            string voter2 = "you";
            string postId2 = "2";
            List<string> vote1 = new List<string> { line1 };
            List<string> vote2 = new List<string> { line2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter2, postId2, voteType);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(line1));
            //Assert.IsFalse(voteCounter.GetVotesCollection(voteType).Keys.Contains(line2));
            Assert.AreEqual(1, VoteCounter.Instance.GetVotesCollection(voteType).Count);
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[line1].Contains(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[line1].Contains(voter2));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter2));
            Assert.AreEqual(postId1, VoteCounter.Instance.GetVotersCollection(voteType)[voter1]);
            Assert.AreEqual(postId2, VoteCounter.Instance.GetVotersCollection(voteType)[voter2]);
        }

        private void TestMismatch(string line1, string line2)
        {
            string voter1 = "me";
            string postId1 = "1";
            string voter2 = "you";
            string postId2 = "2";
            List<string> vote1 = new List<string> { line1 };
            List<string> vote2 = new List<string> { line2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter2, postId2, voteType);

            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(line1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType).Keys.Contains(line2));
            Assert.AreEqual(2, VoteCounter.Instance.GetVotesCollection(voteType).Count);
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[line1].Contains(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotesCollection(voteType)[line2].Contains(voter2));

            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter1));
            Assert.IsTrue(VoteCounter.Instance.GetVotersCollection(voteType).ContainsKey(voter2));
            Assert.AreEqual(postId1, VoteCounter.Instance.GetVotersCollection(voteType)[voter1]);
            Assert.AreEqual(postId2, VoteCounter.Instance.GetVotersCollection(voteType)[voter2]);
        }

        [TestMethod]
        public void TestMatches1()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] First test";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches2()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] 『b』First『/b』 test";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches3()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] first TEST";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches4()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] First  test";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches5()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "-[x] First test";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches6()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "『b』[x] First test『/b』";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches7()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] First test.";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches8()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] First t'est.";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches9()
        {
            string voteLine1 = "[x] “First Test”";
            string voteLine2 = "[x] \"First Test\"";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches10()
        {
            string voteLine1 = "[x] Don't go";
            string voteLine2 = "[x] Donʼt go";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches11()
        {
            string voteLine1 = "[x] Don't go";
            string voteLine2 = "[x] Don’t go";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches12()
        {
            string voteLine1 = "[x] Don't go";
            string voteLine2 = "[x] Don`t go";

            TestMatch(voteLine1, voteLine2);
        }

        [TestMethod]
        public void TestMatches13()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] First &test";

            TestMatch(voteLine1, voteLine2);
        }



        [TestMethod]
        public void TestMismatches1()
        {
            string voteLine1 = "[x] First test";
            string voteLine2 = "[x] Second test";

            TestMismatch(voteLine1, voteLine2);
        }


        #endregion

        [TestMethod]
        public void FindVotesForVoterTest1()
        {
            string voteLine1 = "[x] Vote for stuff 1";
            string voteLine2 = "[x] Vote for stuff 2";
            string voter1 = "me";
            string voter2 = "you";
            string postId1 = "1";
            string postId2 = "2";
            List<string> vote1 = new List<string> { voteLine1 };
            List<string> vote2 = new List<string> { voteLine2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter2, postId2, voteType);
            VoteCounter.Instance.ReferenceVoters.Add(voter1);
            VoteCounter.Instance.ReferenceVoters.Add(voter2);

            var votes = VoteCounter.Instance.GetVotesFromReference("[x] me", "Him");
            Assert.AreEqual(1, votes.Count);
            Assert.IsTrue(votes.Contains(voteLine1));
        }

        [TestMethod]
        public void FindVotesForVoterTest2()
        {
            string voteLine1 = "[x] Vote for stuff 1";
            string voteLine2 = "[x] Vote for stuff 2";
            string voter1 = "me";
            string voter2 = "you";
            string postId1 = "1";
            string postId2 = "2";
            List<string> vote1 = new List<string> { voteLine1 };
            List<string> vote2 = new List<string> { voteLine1, voteLine2 };
            VoteType voteType = VoteType.Vote;

            VoteCounter.Instance.AddVotes(vote1, voter1, postId1, voteType);
            VoteCounter.Instance.AddVotes(vote2, voter2, postId2, voteType);
            VoteCounter.Instance.ReferenceVoters.Add(voter1);
            VoteCounter.Instance.ReferenceVoters.Add(voter2);

            var votes = VoteCounter.Instance.GetVotesFromReference("[x] you", "Him");
            Assert.AreEqual(2, votes.Count);
            Assert.IsTrue(votes.Contains(voteLine1));
            Assert.IsTrue(votes.Contains(voteLine2));
        }

        [TestMethod]
        public void TallyVotesTest()
        {
            //TODO
        }

        [TestMethod]
        public async Task NameReferenceTest()
        {
            // Check for non-case sensitivity in referencing other voters.
            PostComponents p1 = new PostComponents("Beyogi", "12345", "[x] Vote for something");
            PostComponents p2 = new PostComponents("Mini", "12345", "[x] beyogi");
            VoteCounter.Instance.PostsList.Add(p1);
            VoteCounter.Instance.PostsList.Add(p2);
            await VoteCounter.Instance.TallyPosts(sampleQuest);

            Assert.AreEqual(2, VoteCounter.Instance.GetVotersCollection(VoteType.Vote).Count);
            Assert.AreEqual(1, VoteCounter.Instance.GetVotesCollection(VoteType.Vote).Count);
            Assert.IsTrue(VoteCounter.Instance.HasVote("[x] Vote for something\r\n", VoteType.Vote));
        }

    }
}