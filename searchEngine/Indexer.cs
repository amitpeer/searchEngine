using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class Indexer
    {
        Dictionary<string,int[]> m_termsDictionary;
        //get a list of parser result for a few document
        //the string represents the 
        public Indexer(){
            m_termsDictionary = new Dictionary<string, int[]>();

        }
        public void indexBatch(List<Dictionary<string,TermInfoInDoc>> documentsAfterParse)
        {
            Dictionary<string, Term> miniPostingFIle = new Dictionary<string, Term>();
            foreach(Dictionary<string,TermInfoInDoc> parserResult in documentsAfterParse)
            {
               List<string> terms=parserResult.Keys.ToList();
                foreach(string term in terms)
                {
                    if (m_termsDictionary.ContainsKey(term))
                    {
                        m_termsDictionary[term][0] = (m_termsDictionary[term])[0]++;
                    }
                    else
                    {
                        m_termsDictionary.Add(term,new int[] {1,0});
                    }
                    if (miniPostingFIle.ContainsKey(term))
                    {
                        miniPostingFIle[term].TermInDocument.Add(parserResult[term].)


                    }



                }








            }





        }

    }
}
