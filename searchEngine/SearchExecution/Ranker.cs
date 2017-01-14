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
            Dictionary<string, double> rankForDocumentByHeader = new Dictionary<string, double>();
            Dictionary<string, double> FinalRankForDocs = new Dictionary<string, double>();
            Dictionary<string, double> rankForDocumentByInnerProduct = new Dictionary<string, double>();
            List<string> docname = new List<string>();
            m_termsFromQuery = m_controller.getTermsFromQuery(query);
            termsFreqInQuery = new Dictionary<string, int>();
            calculateTermsFreqInQuery(query);
            foreach (string docName in documentsToRank)
            {
                rankForDocumentByBM25[docName] = RankDOCByBM25(m_controller.getDocumentsDic()[docName]);
                rankForDocumentByHeader[docName]= RankDOCByAppearanceInHeader(m_controller.getDocumentsDic()[docName]);
                rankForDocumentByInnerProduct[docName] = RankDocByInnerProduct(m_controller.getDocumentsDic()[docName]);
                FinalRankForDocs[docName] = rankForDocumentByBM25[docName] + 0.1*rankForDocumentByHeader[docName]+ 0.2*rankForDocumentByInnerProduct[docName];
                //FinalRankForDocs[docName]= rankForDocumentByInnerProduct[docName];
            }
            writeSolutionTofile(FinalRankForDocs);
            return null;

        }


        private void writeSolutionTofile(Dictionary<string, double> rankDOCByBM25)
        {
            string[] writeTofile = new string[150];
            int i = 0;
            Dictionary < string, double> top50 = rankDOCByBM25.OrderByDescending(pair => pair.Value).Take(150).ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach(KeyValuePair<string,double> ranked in top50)
            {
                writeTofile[i] = "118 " + "0 " + ranked.Key + " 500 42 mt";
                i++;
            }
            // WriteAllLines creates a file, writes a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            System.IO.File.WriteAllLines(m_controller.m_pathToSave+"\\result118150.txt", writeTofile);
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
        private double RankDOCByAppearanceInHeader(Document docToRank)
        {
            double rankByHeader = 0;
            foreach (KeyValuePair<string, int> termOfQuery in termsFreqInQuery)
            {
               if (m_termsFromQuery[termOfQuery.Key].M_tid.ContainsKey(docToRank.DocName))
                {
                    rankByHeader=rankByHeader+ m_termsFromQuery[termOfQuery.Key].M_tid[docToRank.DocName][1]/termsFreqInQuery.Count;
                    if (m_termsFromQuery[termOfQuery.Key].M_tid[docToRank.DocName][1]==1)
                    {
                        m_termsFromQuery[termOfQuery.Key].M_tid[docToRank.DocName][1] = m_termsFromQuery[termOfQuery.Key].M_tid[docToRank.DocName][1];

                    }
                }
            }
            return rankByHeader*10;

        }
        private double RankDOCByBM25(Document docToRank)
        {
            int count = 0;
            double k1=1.2;
            double k2=300;
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
            if(docToRank.DocName== "FBIS3-6"|| docToRank.DocName == "FBIS3 - 51"|| docToRank.DocName=="FBIS3 - 1842")
            {

                count++;
            }
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
                Rank=Rank+ Math.Log((numeratorInLog / denumeratorInLog)) * mult1 * mult2;
            }
            return Rank;
        }
        private double RankDocByInnerProduct(Document docToRank)
        {
            double Rank = 0;
            double tf = 0;
            double idf = 0;
            foreach (KeyValuePair<string, int> termWeightQuery in termsFreqInQuery)
            {
                if (m_termsFromQuery[termWeightQuery.Key].M_tid.ContainsKey(docToRank.DocName))
                {
                    tf = (Double)(m_termsFromQuery[termWeightQuery.Key].M_tid[docToRank.DocName][0]) / m_controller.getDocumentsDic()[docToRank.DocName].Max_tf;
                    idf = Math.Log(m_controller.getDocumentsDic().Count/ m_termsFromQuery[termWeightQuery.Key].M_tid.Count, 2);
                    Rank = Rank +  tf * idf;
                }
            }
            return Rank;
        }
    }
}
