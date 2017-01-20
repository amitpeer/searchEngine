using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using searchEngine;

namespace searchEngine.SearchExecution
{
    class Searcher
    {
        private Controller controller;
        private Ranker ranker;

        public Searcher(Controller controller)
        {
            this.controller = controller;
            ranker = new Ranker(controller);
        }

        //Input: query (each word seperated by a space), and the languages(null = all, can be more than one language) for the doucments
        //Output: list of string, each item is a document ID, the first item is the most relevent and last is the least relevant. 
        // Maximum size of the output List is 50
        public List<string> search(string query, List<string> languages, bool shouldStem)
        {
            // initialize the documents to rank list first with all of the documents
            List<string> documentsToRank = controller.getDocumentsDic().Keys.ToList();

            // filter documents by language if needed
            if (languages != null && languages.Count > 0)
            {
                documentsToRank = filterDocumentsByLanguage(languages);
            }

            // parse the query and send to ranker
            Parse parse = new Parse(null, shouldStem);
            return ranker.rank(query, documentsToRank,parse,shouldStem);
        }
        public bool saveResults(string pathToFile)
        {

            return ranker.writeSolutionTofile(pathToFile);

        }
        // Input: languages to filter by 
        // Output: documents that are written in that language
        private List<string> filterDocumentsByLanguage(List<string> languages)
        {
            List<string> filteredDocuments = new List<string>();
            foreach (string language in languages)
            {
                filteredDocuments.AddRange(controller.getDocumentsDic().Values
                                                     .Where(d => d.Language.Equals(language))
                                                     .Select(d => d.getDocumentName())
                                                     .ToList());
            }
            return filteredDocuments;
        }
    }
}
