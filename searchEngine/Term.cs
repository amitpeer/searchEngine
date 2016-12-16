using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class Term
    {
        private Dictionary<string, TermInfoInDoc> termInDocument;

        public Term (Dictionary<string, TermInfoInDoc> newTermInDocument)
        {
            termInDocument = newTermInDocument;
        }
        public Dictionary<string, TermInfoInDoc> TermInDocument
        {
            get { return termInDocument; }
            set { termInDocument = value; }
        }
    }
}
