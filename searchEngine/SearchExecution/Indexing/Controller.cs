using Newtonsoft.Json;
using searchEngine.Indexing;
using searchEngine.SearchExecution.Indexing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace searchEngine
{
    class Controller
    {
        private Dictionary<string, Document> documentsDic;
        private Dictionary<string, int[]> mainDic;
        private Dictionary<string, List<string>> freqDic;
        private Parse parser;
        private ReadFile readFile;
        private bool shouldStem;
        private string stemOnFileName;
        private string m_pathToCorpus;
        public string m_pathToSave;
        private Stopwatch stopwatch = new Stopwatch();
        private MainWindow mainWindow;
        public double averageDocumentLength;

        public Controller() { }

        public Parse getParser() { return parser; }

        public Controller(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public Dictionary<string, int[]> getMainDic() { return mainDic; }
        public Dictionary<string, Document> getDocumentsDic() { return documentsDic; }
        public Dictionary<string, List<string>> getFreqDic() { return freqDic; }
        public void reset()
        {
            mainDic = new Dictionary<string, int[]>();
            documentsDic = new Dictionary<string, Document>();
        }
        public void startIndexing(bool _shouldStem, string _path, string _pathToSave)
        {
            stopwatch.Start();
            reset();
            shouldStem = _shouldStem;
            stemOnFileName = shouldStem ? "STEM" : "";
            m_pathToCorpus = _path;
            m_pathToSave = _pathToSave;
            readFile = new ReadFile(m_pathToCorpus);
            readFile.ExtractStopWordsFile();
            parser = new Parse(readFile.getStopWords(), shouldStem);
            Indexer indexer = new Indexer(m_pathToSave, shouldStem);
            int numOfFiles = Directory.GetFiles(m_pathToCorpus).Length - 1;
            int j = 11;
            //create miniPostingFile
            for (int i = 1; i <= numOfFiles; i = i + 10)
            {
                List<string> batchOfDocs = readFile.getFiles(i, j);
                List<Dictionary<string, TermInfoInDoc>> documentsAfterParse = new List<Dictionary<string, TermInfoInDoc>>();
                foreach (string s in batchOfDocs)
                {
                    documentsAfterParse.Add(parser.parseDocument(s));
                }
                indexer.indexBatch(documentsAfterParse);
                j = j + 10;
            }
            indexer.MergeFiles();

            // get maid dictionary and documents dictionary from the indexer 
            mainDic = indexer.getMainDic();
            documentsDic = parser.getDocuments();

            //calculate avarge document length
            averageDocumentLength = calculateAvaregeDocumentLength();
            setMagnitudeForCoSim();
            //save frequencies dictionary to disk
            saveFrequnciesToFile();                                
            
            //save mainDic to disk
            saveMainDic();

            //save documentsDic to disk 
            saveDocumentsDic();

            stopwatch.Stop();
        }
        private void setMagnitudeForCoSim()
        {
            int countTerm = 0;
            BinaryReader br = new BinaryReader(File.Open(m_pathToSave + "\\" + stemOnFileName + "MainPosting.bin", FileMode.Open));
            string lineInFile = "";
            foreach (KeyValuePair<string, int[]> termToWorkOn in mainDic)
            {             
                //read untill you get to the term
                lineInFile = br.ReadString();
                countTerm++;
                //Get the required Term according to lineInFile
                Term current = JsonConvert.DeserializeObject<Term>(lineInFile);
                foreach (KeyValuePair<string, int[]> docInfo in current.M_tid)
                {
                    double tf = ((Double)docInfo.Value[0] / (Double)documentsDic[docInfo.Key].Max_tf);
                    double idf = Math.Log(documentsDic.Count / current.M_tid.Count, 2);
                    documentsDic[docInfo.Key].MagnitudeForCosSim = documentsDic[docInfo.Key].MagnitudeForCosSim + Math.Pow(tf * idf, 2);
                }
            }
            br.Close();
        }
        private Term getTermFromPosting(int pointer)
        {
            BinaryReader br;
            //intialize the binary reader and line for the new term
            br = new BinaryReader(File.Open(m_pathToSave + "\\" + stemOnFileName + "MainPosting.bin", FileMode.Open));
            string lineInFile = "";
            //read untill you get to the term
            for (int i = 0; i <= pointer; i++)
            {
                lineInFile = br.ReadString();
            }
            //Get the required Term according to lineInFile
            Term term = JsonConvert.DeserializeObject<Term>(lineInFile);
            br.Close();
            return term;
        }
   
        public bool load(string path, bool shouldStem)
        {
            reset();
            m_pathToSave = path;
            stemOnFileName = shouldStem ? "STEM" : "";
            try
            {
                loadMainDic();
                loadDocumentsDic();
                loadFreqDic();
            }
            catch (Exception e) { return false; }
            return true;
        }
        public void setNewPaths(string pathToCorpus, string pathToSave)
        {
            m_pathToCorpus = pathToCorpus;
            m_pathToSave = pathToSave;
        }
        public int getNumberOfUniqueTerms() { return mainDic != null ? mainDic.Count : 0; }
        public int getNumberOfParsedDocs() { return documentsDic != null ? documentsDic.Count : 0; }
        public string getTime()
        {
            double min = stopwatch.Elapsed.TotalMinutes;
            return "Time taken: Minutes " + (int)min + "\n Seconds " + (stopwatch.Elapsed.TotalSeconds/60).ToString();
        }
        public Stopwatch getStopwatch() { return stopwatch; }

        public SortedSet<string> getLanguagesInCorpus()
        {
            SortedSet<string> languages = new SortedSet<string>();
            foreach (KeyValuePair<string, Document> Doc in documentsDic)
            {
                string currentLanguage = Doc.Value.Language;
                if (!languages.Contains(currentLanguage) && currentLanguage != "")
                {
                    languages.Add(currentLanguage);
                }
            }
            return languages;


        }

        //Input: Array of string, each item in the array is a term in the query
        //Output: Dictionary: Keys = term, Values = Term object
        public Dictionary<string, Term> getTermsFromQuery(string[] query)
        {
            // Trim all the terms in the array
            int j = 0;
            foreach (string term in query)
            {
                query[j] = term.Trim();
                j++;          
            }

            string lineInFile = "";
            BinaryReader br;
            Dictionary<string, Term> terms = new Dictionary<string, Term>();
            foreach (string termInQuery in query)
            {
                if (!mainDic.ContainsKey(termInQuery))
                    continue;
                //intialize the binary reader and line for the new term
                br = new BinaryReader(File.Open(m_pathToSave + "\\"+ stemOnFileName+"MainPosting.bin", FileMode.Open));
                lineInFile = "";

                //Get the pointer of the term for it's location in the Posting
                int pointer = mainDic[termInQuery][2];

                //read untill you get to the term
                for (int i = 0; i <= pointer; i++)
                {
                    lineInFile = br.ReadString();
                }

                //Get the required Term according to lineInFile
                Term term = JsonConvert.DeserializeObject<Term>(lineInFile);

                //Add the Term to the Dictionary
                terms.Add(term.M_termName, term);

                //Close the binary Reader
                br.Close();
            }
            return terms;
        }

        private double calculateAvaregeDocumentLength()
        {
            double average = 0;
            foreach (Document d in documentsDic.Values)
            {
                average += d.DocumentLength;
            }
            return average / documentsDic.Count;
        }

        private void loadMainDic()
        {
            try
            {
                int linesCounter = 0;
                string lines = "";
                using (BinaryReader br = new BinaryReader(File.Open(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.bin", FileMode.Open)))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        lines += br.ReadString();
                        linesCounter++;
                    }
                    MainDicToSave mainDicToSave = JsonConvert.DeserializeObject<MainDicToSave>(lines);
                    this.mainDic = mainDicToSave.MainDic;
                }
            }
            catch(Exception e) { throw e; }
        }

        private void loadDocumentsDic()
        {
            try
            {
                int linesCounter = 0;
                string lines = "";
                using (BinaryReader br = new BinaryReader(File.Open(m_pathToSave + "\\" + stemOnFileName + "Documents.bin", FileMode.Open)))
                {
                while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            lines += br.ReadString();
                            linesCounter++;
                        }
                DocumentDicToSave documentsDicToSave = JsonConvert.DeserializeObject<DocumentDicToSave>(lines);
                this.documentsDic = documentsDicToSave.DocumentsDic;
                averageDocumentLength = calculateAvaregeDocumentLength();
                }
            }
            catch (Exception e) { throw e; }
        }

        private void loadFreqDic()
        {
            try
            {
                int linesCounter = 0;
                string lines = "";
                using (BinaryReader br = new BinaryReader(File.Open(m_pathToSave + "\\" + stemOnFileName + "FrequencyDic.bin", FileMode.Open)))
                {
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        lines += br.ReadString();
                        linesCounter++;
                    }
                    FrequencyDicToSave freqDicToSave = JsonConvert.DeserializeObject<FrequencyDicToSave>(lines);
                    freqDic = freqDicToSave.FrequencyDic;
                }
            }
            catch (Exception e) { throw e; }
        }

        private void saveMainDic()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.bin", FileMode.Append)))
            {
                MainDicToSave mainDicToSave = new MainDicToSave(mainDic);
                string json = JsonConvert.SerializeObject(mainDicToSave);
                writer.Write(json);
            }
        }

        private void saveDocumentsDic()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(m_pathToSave + "\\" + stemOnFileName + "Documents.bin", FileMode.Append)))
            {
                DocumentDicToSave documentsDicToSave = new DocumentDicToSave(documentsDic);
                string json = JsonConvert.SerializeObject(documentsDicToSave);
                writer.Write(json);
            }
        }

        private void saveFrequnciesToFile()
        {
            freqDic = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, Dictionary<string, int>> stringSuggest in parser.frequencies)
            {
                List<string> options = new List<string>();
                options = stringSuggest.Value.OrderByDescending(pair => pair.Value).Take(5).ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToList();
                freqDic.Add(stringSuggest.Key, options);
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(m_pathToSave + "\\" + stemOnFileName + "FrequencyDic.bin", FileMode.Append)))
            {
                FrequencyDicToSave freqDicToSave = new FrequencyDicToSave(freqDic);
                string json = JsonConvert.SerializeObject(freqDicToSave);
                writer.Write(json);
            }
            parser.frequencies = null;
        }
    }
}
