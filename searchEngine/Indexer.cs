using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class Indexer
    {
        private Dictionary<string, int[]> mainDic;
        private List<string> mergeColission = new List<string>();
        private string  m_pathToSave;
        private int counterFiles;
        private bool shouldStem;

        //get a list of parser result for a few document
        //the string represents the 
        public Indexer(string pathToSave, bool _shouldStem)
        {
            mainDic = new Dictionary<string, int[]>();
            m_pathToSave = pathToSave;
            shouldStem = _shouldStem;
        }

        public Dictionary<string, int[]> getMainDic()
        {
            return mainDic;
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
                }
            writer.Flush();
            writer.Close();
        }

        public void MergeFiles()
        {
            int counterniqueTerms = 0;
            int numOfFiles = Directory.GetFiles(m_pathToSave).Length;
            SortedDictionary<string,TermWithReader> termsInComparisonForMerge = new SortedDictionary<string, TermWithReader>();
            Dictionary<string, BinaryReader> BinaryReaders = new Dictionary<string, BinaryReader>();
            //open a binary reader for each file
            foreach (string file in Directory.GetFiles(m_pathToSave))
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (fileName.Contains("miniPosting"))
                {
                    BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open));
                    BinaryReaders.Add(file, br);
                }
            }
            //insert the first value for each reader
            foreach(KeyValuePair<string,BinaryReader> br in BinaryReaders)
                {
                BinaryReader b = br.Value;
                string pathOfFileRead = br.Key;
                tryagain:
                if (b.BaseStream.Position != b.BaseStream.Length)
                {
                    string line = b.ReadString();
                    Term currentTerm = JsonConvert.DeserializeObject<Term>(line);
                    if (termsInComparisonForMerge.ContainsKey(currentTerm.M_termName))
                    {
                        termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid = safeMerge(termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid, currentTerm.M_tid, currentTerm.M_termName);
                        //termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid = termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid.Concat(currentTerm.M_tid).ToDictionary(x => x.Key, x => x.Value);
                        goto tryagain;
                    }
                    else
                    {
                        termsInComparisonForMerge.Add(currentTerm.M_termName, new TermWithReader(currentTerm, pathOfFileRead));
                    }
                }
                else
                {
                    b.Close();
                    BinaryReaders.Remove(pathOfFileRead);
                    File.Delete(pathOfFileRead);
                }
            }
            string stemOnFileName = shouldStem ? "STEM" : "";
            BinaryWriter writer = new BinaryWriter(File.Open(m_pathToSave + "\\" + stemOnFileName + "MainPosting.bin", FileMode.Append));
            while (BinaryReaders.Count > 0)
            {
                TermWithReader twr = termsInComparisonForMerge.First().Value;
                termsInComparisonForMerge.Remove(termsInComparisonForMerge.First().Key);
                mainDic.Add(twr.Term.M_termName, new int[] { twr.Term.M_tid.Count, counterniqueTerms });
                counterniqueTerms++;
                WriteTermToFile(writer, twr.Term);
                //if the reader is not in the end of the file
                nextTerminBinaryReader:
                if (BinaryReaders[twr.Br].BaseStream.Position != BinaryReaders[twr.Br].BaseStream.Length)
                {
                    //get next term from BinaryReader
                    string line = BinaryReaders[twr.Br].ReadString();
                    Term currentTerm = JsonConvert.DeserializeObject<Term>(line);
                    if (termsInComparisonForMerge.ContainsKey(currentTerm.M_termName))
                    {
                        termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid = safeMerge(termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid, currentTerm.M_tid, currentTerm.M_termName);
                       // termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid=termsInComparisonForMerge[currentTerm.M_termName].Term.M_tid.Concat(currentTerm.M_tid).ToDictionary(x => x.Key, x => x.Value);
                        goto nextTerminBinaryReader;
                    }
                    else
                    {
                        termsInComparisonForMerge.Add(currentTerm.M_termName,new TermWithReader(currentTerm,twr.Br));
                    }
                }
                else
                {
                    BinaryReaders[twr.Br].Close();
                    BinaryReaders.Remove(twr.Br);
                    File.Delete(twr.Br);
                }
            }
            writer.Close();
        }
        private void WriteTermToFile(BinaryWriter writerToFile, Term t)
        {
            string json = JsonConvert.SerializeObject(t);
            writerToFile.Write(json);
            writerToFile.Write("\n");
        }

        private Dictionary<string, int[]> safeMerge(Dictionary<string, int[]> first, Dictionary<string, int[]> second, string termName)
        {
            Dictionary<string, int[]> ans = new Dictionary<string, int[]>(first);
            foreach(KeyValuePair<string, int[]> secondKeyValue in second)
            {
                if (!ans.ContainsKey(secondKeyValue.Key))
                {
                    ans.Add(secondKeyValue.Key, secondKeyValue.Value);
                }
                else
                {
                    mergeColission.Add(termName);
                }
            }
            return ans;
        }

    }

}