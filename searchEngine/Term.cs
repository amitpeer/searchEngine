using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class Term
    {
        private Dictionary<string, int[]> TID;
        public Term (Dictionary<string, int[]> newTermInDocument)
        {
            TID = newTermInDocument;
        }
        public Dictionary<string, int[]> tid
        {
            get { return TID; }
        }
    }
}
