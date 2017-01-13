using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine.SearchExecution
{
    class Ranker
    {
        private Dictionary<string, int> termsFreqInQuery;
        private Controller m_controller;
        Dictionary<string, Term> m_termsFromQuery;


        public Ranker(Controller controller)
        {
            m_controller = controller;
            termsFreqInQuery = new Dictionary<string, int>();
            m_termsFromQuery = new Dictionary<string, Term>();
        }

        //Input: string array for the query, each item in the array is a (parsed) term in the query
        //       documents list to rank (after language filter)
        //Output: list of documents relevent to the query, the first document is the most relevent
        public List<string> rank(string[] query, List<string> documentsToRank)
        {
            Dictionary<string, double> rankForDocumentByBM25 = new Dictionary<string, double>();
            List<string> docname = new List<string>();
            m_termsFromQuery = m_controller.getTermsFromQuery(query);
            foreach (string docName in documentsToRank)
            {
                rankForDocumentByBM25[docName] = RankDOCByBM25(m_controller.getDocumentsDic()[docName]);
                if (rankForDocumentByBM25[docName] > 0)
                {
                    docname.Add(docName + "rankGiven:" + rankForDocumentByBM25[docName]);
                }

            }
            return null;

        }

        private void calculateTermsFreqInQuery(string[] query)
        {
            foreach(string s in query)
            {
                if (!termsFreqInQuery.ContainsKey(s))
                {
                    termsFreqInQuery.Add(s, 1);
                }
                else
                {
                    termsFreqInQuery[s]++;
                }
            }
        }
        private double RankDOCByBM25(Document docToRank)
        {
            double k1=1.2;
            double k2=100;
            double b=0.75;
            double dl = docToRank.DocumentLength;
            double avgdl = m_controller.averageDocumentLength;
            int ri=0;
            int R=0;
            int ni;
            int N = m_controller.getDocumentsDic().Count;
            int fi;
            int qfi;
            double K = k1 * ((1 - b) + b * dl / avgdl);
            double numeratorInLog;
            double denumeratorInLog;
            double mult1;
            double mult2;
            double Rank = 0;
            foreach (KeyValuePair<string,int> termOfQuery in termsFreqInQuery)
            {
                if (m_termsFromQuery[termOfQuery.Key].M_tid.ContainsKey(docToRank.DocName))
                {
                    fi = m_termsFromQuery[termOfQuery.Key].M_tid[docToRank.DocName][0];
                }
                else
                    fi = 0;
                qfi = termsFreqInQuery[termOfQuery.Key];
                ni = m_controller.getMainDic()[termOfQuery.Key][1];
                numeratorInLog = (ri + 0.5) / (R - ri + 0.5);
                denumeratorInLog = (ni - ri + 0.5) / (N - ni - R + ri + 0.5);
                mult1 = ((k1 + 1) * fi) / (K + fi);
                mult2 = ((k2 + 1) * qfi) / (k2 + qfi);                
                Rank=Rank+ Math.Log((numeratorInLog / denumeratorInLog) * mult1 * mult2);
            }
            return Rank;
        }
    }
}
