﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTally.Votes
{
    public class VoteLineSequence : List<VoteLine>
    {
        public VoteLineSequence()
        {

        }

        public VoteLineSequence(IEnumerable<VoteLine> voteLines)
            : base(voteLines)
        {

        }
    }
}
