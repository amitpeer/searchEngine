using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class ManageSearch
    {
        public void main()
        {
            string path = "C:\\Users\adamz\\Documents\\Visual Studio 2015\\Projects\\folder\\test1";
            string pathToSave = "C:\\Users\adamz\\Documents\\Visual Studio 2015\\Projects\\folder\\results";
            bool shouldStem = false;
            ReadFile readFile = new ReadFile(path);
            Parse parser = new Parse();
            Indexer indexer = new Indexer(pathToSave);
            readFile.ExtractStopWordsFile();
            int numOfFiles=Directory.GetFiles(path).Length-1;
            int j = 5;
            for (int i = 1; i < numOfFiles-1&&j<numOfFiles-1; i=i+5)
            {
                List<string> batchOfDocs = readFile.getFiles(i, j);
                List<Dictionary<string, TermInfoInDoc>> documentsAfterParse = new List<Dictionary<string, TermInfoInDoc>>() ;
                foreach (string s in batchOfDocs)
                {
                    documentsAfterParse.Add(parser.parseDocument(s, shouldStem));
                }
                indexer.indexBatch(documentsAfterParse);   
                j = j + 5;
            }



        }
    }
}
