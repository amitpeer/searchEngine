using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class ManageSearch
    {
        public static void main()
        {
            Stopwatch m_stopwatch = new Stopwatch();
            m_stopwatch.Start();
            string path = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\corpus";
            string pathToSave = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\results";
            bool shouldStem = false;
            ReadFile readFile = new ReadFile(path);
            readFile.ExtractStopWordsFile();
            Parse parser = new Parse(readFile.getStopWords(),shouldStem);
            Indexer indexer = new Indexer(pathToSave);
            int numOfFiles=Directory.GetFiles(path).Length-1;
            int j = 11;
            //create miniPostingFile
            for (int i = 1; i <= numOfFiles; i=i+10)
            {
                List<string> batchOfDocs = readFile.getFiles(i, j);
                List<Dictionary<string, TermInfoInDoc>> documentsAfterParse = new List<Dictionary<string, TermInfoInDoc>>() ;
                foreach (string s in batchOfDocs)
                {
                    documentsAfterParse.Add(parser.parseDocument(s, shouldStem));
                }
                indexer.indexBatch(documentsAfterParse);   
                j = j +10;
            }
            indexer.MergeFiles();


        }

        public static void testReader()
        {
            string path = "C:\\Users\\adamz\\Documents\\Visual Studio 2015\\Projects\\folder\\testReader\\miniPosting1.bin";
            int counter = 0;
            List<Term> ls = new List<Term>();
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))

            {
                while (5< 10)
                {

                    string line = reader.ReadString();
                    Term ans = JsonConvert.DeserializeObject<Term>(line);
                    ls.Add(ans);
                    counter++;
                }
            }

            /*
            {
               
                //Console.WriteLine(ans.ToString());
               // Console.ReadLine();

            }*/
        }
    }
}
