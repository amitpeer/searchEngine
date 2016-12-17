using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    [Serializable]
    public class Term
    {
        string m_termName;
        private Dictionary<string, int[]> TID;
        public Term (string termName, Dictionary<string, int[]> newTermInDocument)
        {
            m_termName = termName;
            TID = newTermInDocument;
        }
        public Dictionary<string, int[]> tid
        {
            get { return TID; }
        }
    }
}
