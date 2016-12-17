using Newtonsoft.Json;
using System;
using System.Collections;
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
        private string  m_pathToSave;
        private int counterFiles;

        //get a list of parser result for a few document
        //the string represents the 
        public Indexer(string pathToSave)
        {
            m_termsDictionary = new Dictionary<string, int[]>();
            m_pathToSave = pathToSave;
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
                        miniPostingFile[term].M_tid.Add(parserResult[term].DocName, new int[2] { parserResult[term].Tf, isTitle });
                        counterPosting++;
                    }
                    else
                    {
                        Dictionary<string, int[]> m_term = new Dictionary<string, int[]>();
                        m_term.Add(parserResult[term].DocName, new int[2] { parserResult[term].Tf, isTitle });
                        Term termToInsert = new Term(term,m_term);
                        miniPostingFile.Add(term, termToInsert);                                                                
                    }
                 }
            }
                List<string> sortedTerms = miniPostingFile.Keys.ToList<string>();
                sortedTerms.Sort();           
            BinaryWriter writer = new BinaryWriter(File.Open( m_pathToSave + "\\miniPosting" + counterFiles + ".bin", FileMode.Append));
            foreach (string s in sortedTerms)
                {
                Term toInsert = new Term(s, miniPostingFile[s].M_tid);
                string json = JsonConvert.SerializeObject(toInsert);
                writer.Write(json);
                //writer.Write("\n");
                }
            writer.Flush();                    
        }

        public void MergeFiles()
        {
            //    Comparer<Term> termComparer = new termComparer() ;
            int numOfFiles = Directory.GetFiles(m_pathToSave).Length;
            SortedDictionary<string,Term> termsInComparisonForMerge = new SortedDictionary<string, Term>();
            Dictionary<string, BinaryReader> BinaryReaders = new Dictionary<string, BinaryReader>();

            BinaryReader[] files = new BinaryReader[numOfFiles];
            int i = 0;
            foreach(string file in Directory.GetFiles(m_pathToSave))
            {
                files[i]=(new BinaryReader(File.Open(file, FileMode.Open)));
                i++;
            }
            foreach(BinaryReader br in files)
            {
                string line = br.ReadString();
                Term currentTerm = JsonConvert.DeserializeObject<Term>(line);
                if (termsInComparisonForMerge.ContainsKey(currentTerm.M_termName))
                {
                    termsInComparisonForMerge[currentTerm.M_termName].M_tid.Concat(currentTerm.M_tid).ToDictionary(x => x.Key, x => x.Value);
                }
                else
                {
                    termsInComparisonForMerge.Add(currentTerm.M_termName, currentTerm);
                }
           }

            









        }
    }
    }