﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class TermWithReader
    {
        private Term term;
        private String br;
        public TermWithReader(Term t, String b)
        {
            term = t;
            br = b;

        }
        public Term Term
        {
            get { return term; }
            set { term = value; }
        }

        public String Br
        {
            get { return br; }
            set { br = value; }
        }
    }
}
