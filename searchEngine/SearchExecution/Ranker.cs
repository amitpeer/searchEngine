using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using WordNetClasses;
using Wnlib;
using NHunspell;

namespace searchEngine.SearchExecution
{
    class Ranker
    {
        private Controller m_controller;
        private Dictionary<string, int> termsFreqInQuery;
        private Dictionary<string, Term> m_termsFromQuery;

        private List<string> documentsToRank;

        public Ranker(Controller controller)
        {
            m_controller = controller;
            termsFreqInQuery = new Dictionary<string, int>();
            m_termsFromQuery = new Dictionary<string, Term>();
        }
        //Input: string array for the query, each item in the array is a (parsed) term in the query
        //       documents list to rank (after language filter)
        //Output: list of documents relevent to the query, the first document is the most relevent
        public List<string> rank(string query, List<string> documentsToRank,Parse parser, bool shouldStem)
        {
            this.documentsToRank = documentsToRank;
            // rank the original query
            query = query.Trim();
            Dictionary<string, double> rankOriginalQuery = rankQuery(parser.parseQuery(query, shouldStem), false);

            //build a new query from the synamouns
            string[] newQuery = buildNewSynonyms(query.Split(' '));
            newQuery = parser.parseQuery(query, shouldStem);

            // rank the new query
            Dictionary<string, double> rankNewQuery = rankQuery(newQuery,true);

            //build the final rank dictionary
            Dictionary<string, double> finalRankAllQueries = new Dictionary<string, double>();
            foreach(KeyValuePair<string, double> keyValue in rankOriginalQuery)
            {
                // ***Assuming the keys are identical and in the same order in rankOriginalQuery & rankNewQuery***
                finalRankAllQueries.Add(keyValue.Key, 0.8 * (keyValue.Value) + 0.2 * (rankNewQuery[keyValue.Key]));
            }
            // sort the dictionary by the rank (dictionary values) and return the documents (dictionary keys) as a list
            return finalRankAllQueries.OrderByDescending(pair => pair.Value).Take(50).ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToList();
        }

        private string[] buildNewSynonyms(string[] query)
        {
            List<string> newQueryAsList = new List<string>();
            Hunspell hunspell = new Hunspell("en_us.aff", "en_us.dic");
            MyThes r = new MyThes("Thes\\th_en_US_new.dat");
            foreach (string termInQuery in query)        
            {
                // Get all synonyms for this term.
                ThesResult tr = r.Lookup(termInQuery);
                if (tr == null)
                {
                    // no synonyms found, add the original term to the new query
                    // newQueryAsList.Add(termInQuery);
                    tr = r.Lookup(hunspell.Stem(termInQuery)[0]);
                    if(tr==null)
                    continue;
                }
                List<string> termSynonyms = tr.GetSynonyms().Keys.ToList();
                // Build a new query from first 4 synonyms if exists
                for (int i=0; i<4; i++)
                {
                   // hunspell.
                    if (termSynonyms.Any())
                    {
                        if (termSynonyms[0].Split(' ')[0] != termInQuery)
                        {
                            newQueryAsList.Add(termSynonyms[0].Split(' ')[0]);
                        }
                        termSynonyms.RemoveAt(0);
                    }
                }
            }
            return newQueryAsList.ToArray();          
        }

        private Dictionary<string, double> rankQuery(string[] query,bool isSynonyms)
        {
            Dictionary<string, double> rankForDocumentByBM25 = new Dictionary<string, double>();
            Dictionary<string, double> rankForDocumentByHeader = new Dictionary<string, double>();
            Dictionary<string, double> rankForDocumentByInnerProduct = new Dictionary<string, double>();
            Dictionary<string, double> FinalRankForDocs = new Dictionary<string, double>();
            List<string> docname = new List<string>();
            m_termsFromQuery = m_controller.getTermsFromQuery(query);
            termsFreqInQuery = new Dictionary<string, int>();
            calculateTermsFreqInQuery(query);
            foreach (string docName in documentsToRank)
            {
                rankForDocumentByBM25[docName] = RankDOCByBM25(m_controller.getDocumentsDic()[docName]);
                rankForDocumentByHeader[docName] = RankDOCByAppearanceInHeader(m_controller.getDocumentsDic()[docName]);
                rankForDocumentByInnerProduct[docName] = RankDocByCosSimilarity(m_controller.getDocumentsDic()[docName]);
                 FinalRankForDocs[docName] = 0.7*rankForDocumentByBM25[docName] + 0.3 * rankForDocumentByHeader[docName] +0.3*rankForDocumentByInnerProduct[docName];
            }
            return FinalRankForDocs;
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
            return rankByHeader*5;
        }
        private double RankDOCByBM25(Document docToRank)
        {
            int count = 0;
            double k1 = 1.2;
            double k2 = 300;
            double b = 0.75;
            double dl = docToRank.DocumentLength;
            double avgdl = m_controller.averageDocumentLength;
            int ri = 0;
            int R = 0;
            int ni;
            int N = m_controller.getDocumentsDic().Count;
            int fi;
            int qfi;
            double K = k1 * ((1 - b) + b * (dl / avgdl));
            double numeratorInLog;
            double denumeratorInLog;
            double mult1;
            double mult2;
            double Rank = 0;
            foreach (KeyValuePair<string, int> termOfQuery in termsFreqInQuery)
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
                Rank = Rank + Math.Log((numeratorInLog / denumeratorInLog)) * mult1 * mult2;
            }
            return Rank;
        }
        private double RankDocByCosSimilarity(Document docToRank)
        {
            double cosSimRank = 0;
            double sim = 0;
            double tf = 0;
            double idf = 0;
            foreach (KeyValuePair<string, int> termWeightQuery in termsFreqInQuery)
            {
                if (m_termsFromQuery.ContainsKey(termWeightQuery.Key))
                {
                    if (m_termsFromQuery[termWeightQuery.Key].M_tid.ContainsKey(docToRank.DocName))
                    {
                        tf = (Double)(m_termsFromQuery[termWeightQuery.Key].M_tid[docToRank.DocName][0]) / m_controller.getDocumentsDic()[docToRank.DocName].Max_tf;
                        idf = Math.Log(m_controller.getDocumentsDic().Count / m_termsFromQuery[termWeightQuery.Key].M_tid.Count, 2);
                        sim = sim + tf * idf;
                    }
                }
            }
            cosSimRank = sim/(Math.Sqrt(docToRank.MagnitudeForCosSim)*(termsFreqInQuery.Count));
            return cosSimRank;
        }
    }
}
