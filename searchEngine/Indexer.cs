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
            int counterPosting = 0;
            foreach (Dictionary<string, TermInfoInDoc> parserResult in documentsAfterParse)
            {
                List<string> terms = parserResult.Keys.ToList();
                foreach (string term in terms)
                {
                    if (miniPostingFile.ContainsKey(term))
                    {
                        miniPostingFile[term].TermInDocument.Add(parserResult[term].DocName, parserResult[term]);
                        counterPosting++;
                    }
                    else
                    {
                        Dictionary<string,TermInfoInDoc> m_term = new Dictionary<string, TermInfoInDoc>();
                        m_term.Add(parserResult[term].DocName, parserResult[term]);
                        Term termToInsert = new Term(m_term);
                        miniPostingFile.Add(term, termToInsert);
                    }
                 }
            }
                List<string> sortedTerms = miniPostingFile.Keys.ToList<string>();
                sortedTerms.Sort();
            BinaryWriter writer = new BinaryWriter(File.Open(path + "\\miniPosting" + counterFiles + ".bin", FileMode.Append));
            foreach (string s in sortedTerms)
                {
                    KeyValuePair<string, Term> toInsert = new KeyValuePair<string, Term>(s, miniPostingFile[s]);
                    string json = JsonConvert.SerializeObject(toInsert, Formatting.Indented);
                    writer.Write(json);
                }
            writer.Flush();           
            }
        }
    }