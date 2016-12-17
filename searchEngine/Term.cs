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
        private string m_termName;
        Dictionary<string, int[]> m_tid;
        public Term (string termName, Dictionary<string, int[]> newTermInDocument)
        {
            m_termName = termName;
            m_tid = newTermInDocument;
        }
        public Dictionary<string, int[]> M_tid
        {
            get { return m_tid; }
            set { m_tid = value; }
        }
        public string M_termName
        {
            get { return m_termName; }
            set { m_termName = value; }
        }
        public override string ToString()
        {
            string ans;
            ans = this.m_termName + "|";
            foreach(KeyValuePair<string,int[]> var in this.m_tid)
            {
                ans = ans + var.Key + ":" + var.Value[0] + "," + var.Value[1] + ";";
            }
            return ans;
        }
    }
}
