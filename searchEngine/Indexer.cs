using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class Indexer
    {
        Dictionary<string, int[]> m_termsDictionary;
        private string path;
        private int counterFiles;

        //get a list of parser result for a few document
        //the string represents the 
        public Indexer(string pathToSave)
        {
            m_termsDictionary = new Dictionary<string, int[]>();
            path = pathToSave;

        }
        public void indexBatch(List<Dictionary<string, TermInfoInDoc>> documentsAfterParse)
        {
            Dictionary<string, Term> miniPostingFile = new Dictionary<string, Term>();
            counterFiles++;
            foreach (Dictionary<string, TermInfoInDoc> parserResult in documentsAfterParse)
            {
                List<string> terms = parserResult.Keys.ToList();
                foreach (string term in terms)
                {
                    /*
                    if (m_termsDictionary.ContainsKey(term))
                    {
                        m_termsDictionary[term][0] = (m_termsDictionary[term])[0]++;
                    }
                    else
                    {
                        m_termsDictionary.Add(term,new int[] {1,0});
                    }
                    if (miniPostingFile.ContainsKey(term))
                    {
                        miniPostingFile[term].TermInDocument.Add(parserResult[term].DocName, parserResult[term]);
                    }
                    else
                    {
                        Dictionary<string,TermInfoInDoc> m_term = new Dictionary<string, TermInfoInDoc>();
                        m_term.Add(parserResult[term].DocName, parserResult[term]);
                        Term termToInsert = new Term(m_term);
                        miniPostingFile.Add(term, termToInsert);
                    }
                 }*/
                }
                List<string> sortedTerms = miniPostingFile.Keys.ToList<string>();
                sortedTerms.Sort();
                foreach (string s in sortedTerms)
                {
                    KeyValuePair<string, Term> toInsert = new KeyValuePair<string, Term>(s, miniPostingFile[s]);
                    string json = JsonConvert.SerializeObject(toInsert, Formatting.Indented);
                    if (File.Exists(path + "\\miniPosting" + counterFiles + ".json")){
                        File.AppendAllText(path + "\\miniPosting" + counterFiles + ".json", json);
                        }
                    else
                        System.IO.File.WriteAllText(path + "\\miniPosting" + counterFiles + ".json", json);
                }
            }
        }
    }
}