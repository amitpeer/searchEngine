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
        private Dictionary<string, List<string>> resultsFromQueriesInFile = new Dictionary<string, List<string>>();

        public Searcher(Controller controller)
        {
            this.controller = controller;
            ranker = new Ranker(controller);
        }

        public Dictionary<string, List<string>> getResultsFromQueriesInFile() { return resultsFromQueriesInFile; }

        //Input: query (each word seperated by a space), and the languages(null = all, can be more than one language) for the doucments
        //Output: list of string, each item is a document ID, the first item is the most relevent and last is the least relevant. 
        // Maximum size of the output List is 50
        public List<string> search(string query, List<string> languages, bool shouldStem)
        {
            // get all documents to rank according to chosen languages
            List<string> documentsToRank = getDocumentsToRank(languages);

            // parse the query and send to ranker
            Parse parse = new Parse(null, shouldStem);
            return ranker.rank(query, documentsToRank,parse,shouldStem);
        }

        public Dictionary<string, List<string>> searchFile(string path, List<string> languages, bool shouldStem)
        {
            // get all documents to rank according to chosen languages
            List<string> documentsToRank = getDocumentsToRank(languages);

            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();
            string line;

            // Read the file and handle it line by line.
            using (System.IO.StreamReader queriesFile = new System.IO.StreamReader("queries.txt"))
            {
                while ((line = queriesFile.ReadLine()) != null)
                {
                    //split the line in the text file, to separate query ID and the query iteself
                    string[] splittedLine = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string queryId = splittedLine[0].Trim();
                    string query = splittedLine[1].Trim();

                    // parse the query and send to ranker
                    Parse parse = new Parse(null, shouldStem);
                    List<string> rank = ranker.rank(query, documentsToRank, parse, shouldStem);

                    //if results came back, add to the dictionary
                    results.Add(queryId, rank);
                }
            }
            resultsFromQueriesInFile = results;
            return results;     
        }


        // Input: languages to filter by 
        // Output: documents that are written in that language
        private List<string> getDocumentsToRank(List<string> languages)
        {
            // initialize the documents to rank list first with all of the documents
            List<string> documentsToRank = controller.getDocumentsDic().Keys.ToList();

            // filter documents by language if needed
            if (languages != null && languages.Count > 0)
            {
                documentsToRank = filterDocumentsByLanguage(languages);
            }
            return documentsToRank;
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
