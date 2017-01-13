using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace searchEngine
{
    class Controller
    {
        private Dictionary <string, Document> documentsDic;
        private Dictionary <string, int[]> mainDic;
        private Parse parser;
        private ReadFile readFile;
        private bool shouldStem;
        private string stemOnFileName;
        private string m_pathToCorpus;
        private string m_pathToSave;
        private Stopwatch stopwatch = new Stopwatch();
        private MainWindow mainWindow;
        private double averageDocumentLength;

        public Controller() { }

        public Parse getParser() { return parser; }

        public Controller(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public Dictionary<string, int[]> getMainDic() { return mainDic; }
        public Dictionary<string, Document> getDocumentsDic() { return documentsDic; }
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
            parser = new Parse(readFile.getStopWords(),shouldStem);
            Indexer indexer = new Indexer(m_pathToSave, shouldStem);
            int numOfFiles=Directory.GetFiles(m_pathToCorpus).Length-1;
            int j = 11;
            //create miniPostingFile
            for (int i = 1; i <= numOfFiles; i=i+10)
            {
                List<string> batchOfDocs = readFile.getFiles(i, j);
                List<Dictionary<string, TermInfoInDoc>> documentsAfterParse = new List<Dictionary<string, TermInfoInDoc>>();
                foreach (string s in batchOfDocs)
                {
                    documentsAfterParse.Add(parser.parseDocument(s));
                }
                indexer.indexBatch(documentsAfterParse);   
                j = j +10;
            }
            indexer.MergeFiles();
            mainDic = indexer.getMainDic();
            documentsDic = parser.getDocuments();
            averageDocumentLength = calculateAvaregeDocumentLength();

            //save mainDic to disk
            File.WriteAllBytes(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.zip", zipCompress(mainDic));
            //save documentsDic to disk 
            File.WriteAllBytes(m_pathToSave + "\\" + stemOnFileName + "Documents.zip", zipCompress(documentsDic));

            stopwatch.Stop();

        }

        public bool load(string path, bool shouldStem)
        {
            reset();
            m_pathToSave = path;
            stemOnFileName = shouldStem ? "STEM" : "";
            if (!unZipMainDic() || !unZipDocumentsDic())
            {
                reset();
                return false;
            }
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
            return "Time taken: Minutes " + (int)min+"\n Seconds "+ (stopwatch.Elapsed.TotalSeconds/60).ToString();
        }
        public Stopwatch getStopwatch() { return stopwatch; }   
        //COMPRESSING (TO DISK) METHODS:
        [MethodImpl(MethodImplOptions.Synchronized)]
        private byte[] zipCompress(object obj)
        {
            byte[] bArray;
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Compress);
                try
                {
                    (new BinaryFormatter()).Serialize(gzStream, obj);
                }
                finally
                {
                    if (gzStream != null)
                    {
                        ((IDisposable)gzStream).Dispose();
                    }
                }
                bArray = memoryStream.ToArray();
            }
            finally
            {
                if (memoryStream != null)
                {
                    ((IDisposable)memoryStream).Dispose();
                }
            }
            return bArray;
        }
        public SortedSet<string> getLanguagesInCorpus()
        {
           SortedSet<string> languages = new SortedSet<string>();
            foreach (KeyValuePair<string, Document> Doc in documentsDic)
            {
                string currentLanguage = Doc.Value.Language;
                if (!languages.Contains(currentLanguage) && currentLanguage!="")
                {
                    languages.Add(currentLanguage);
                }
            }
            return languages;


        }
        private bool unZipMainDic()
        {
            if (File.Exists(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.zip"))
            {
                GZipStream gZipStream = new GZipStream(File.OpenRead(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.zip"), CompressionMode.Decompress);
                try
                {
                    mainDic = (Dictionary<string, int[]>)(new BinaryFormatter()).Deserialize(gZipStream);
                }
                finally
                {
                    if (gZipStream != null)
                    {
                        ((IDisposable)gZipStream).Dispose();
                    }
                }
                return true;
            }
            else
                return false;

        }
        private bool unZipDocumentsDic()
        {
            if (File.Exists(m_pathToSave + "\\" + stemOnFileName + "Documents.zip"))
            {
                GZipStream gZipStream = new GZipStream(File.OpenRead(m_pathToSave + "\\" + stemOnFileName + "Documents.zip"), CompressionMode.Decompress);
                try
                {
                    documentsDic = (Dictionary<string, Document>)(new BinaryFormatter()).Deserialize(gZipStream);
                    averageDocumentLength = calculateAvaregeDocumentLength();
                }
                finally
                {
                    if (gZipStream != null)
                    {
                        ((IDisposable)gZipStream).Dispose();
                    }
                }
                return true;
            }
            else
                return false;

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
    }
}
