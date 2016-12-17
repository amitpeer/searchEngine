using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class termComparer : Comparer<Term>
    {
        public override int Compare(Term x, Term y)
        {
            Term t1 = (Term)x;
            Term t2 = (Term)y;
            return t1.M_termName.CompareTo(t2.M_termName);
        }

    }
}
