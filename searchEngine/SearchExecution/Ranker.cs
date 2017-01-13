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

        public Ranker()
        {
            termsFreqInQuery = new Dictionary<string, int>();
        }

        //Input: string array for the query, each item in the array is a (parsed) term in the query
        //       documents list to rank (after language filter)
        //Output: list of documents relevent to the query, the first document is the most relevent
        public List<string> rank(string[] query, List<string> documentsToRank)
        {
            throw new NotImplementedException();
        }

        private void calculateTermsFreqInQuery(string[] query)
        {

        }
    }
}
