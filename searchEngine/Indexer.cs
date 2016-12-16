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
                    int isTitle = parserResult[term].IsTitle == true ? 1 : 0;
                    if (miniPostingFile.ContainsKey(term))
                    {
                        miniPostingFile[term].tid.Add(parserResult[term].DocName, new int[2] { parserResult[term].Tf, isTitle });
                        counterPosting++;
                    }
                    else
                    {
                        Dictionary<string, int[]> m_term = new Dictionary<string, int[]>();
                        m_term.Add(parserResult[term].DocName, new int[2] { parserResult[term].Tf, isTitle });
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
                    string json = JsonConvert.SerializeObject(toInsert);
                    writer.Write(json);
                }
            writer.Flush();           
            }
        }
    }