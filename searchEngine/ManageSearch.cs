using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace searchEngine
{
    class ManageSearch
    {
        private Dictionary <string, Document> documentsDic;
        private Dictionary <string, int[]> mainDic;
        private Parse parser;
        private ReadFile readFile;
        private string m_path = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\small corpus";
        private string m_pathToSave = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\results";
        private bool shouldStem;
        private string stemOnFileName;

        public ManageSearch(bool _shouldStem)
        {
            shouldStem = _shouldStem;
            stemOnFileName = shouldStem ? "STEM" : "";
        }

        public Dictionary<string, int[]> getMainDic() { return mainDic; }
        public Dictionary<string, Document> getDocumentsDic() { return documentsDic; }
        public void reset()
        {
            mainDic = new Dictionary<string, int[]>();
            documentsDic = new Dictionary<string, Document>();
        }

        public void main()
        {
            readFile = new ReadFile(m_path);
            readFile.ExtractStopWordsFile();
            parser = new Parse(readFile.getStopWords(),shouldStem);
            Indexer indexer = new Indexer(m_pathToSave, shouldStem);
            int numOfFiles=Directory.GetFiles(m_path).Length-1;
            int j = 11;
            //create miniPostingFile
            for (int i = 1; i <= numOfFiles; i=i+10)
            {
                List<string> batchOfDocs = readFile.getFiles(i, j);
                List<Dictionary<string, TermInfoInDoc>> documentsAfterParse = new List<Dictionary<string, TermInfoInDoc>>() ;
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
            //save mainDic to disk
            File.WriteAllBytes(m_pathToSave + "\\" + stemOnFileName + "MainDictionary.zip", zipCompress(mainDic));
            //save documentsDic to disk 
            File.WriteAllBytes(m_pathToSave + "\\" + stemOnFileName + "Documents.zip", zipCompress(documentsDic));
        }

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
        private void unZipMainDic()
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
        }
        private void unZipDocumentsDic()
        {
            GZipStream gZipStream = new GZipStream(File.OpenRead(m_pathToSave + "\\" + stemOnFileName + "Documents.zip"), CompressionMode.Decompress);
            try
            {
                documentsDic = (Dictionary<string, Document>)(new BinaryFormatter()).Deserialize(gZipStream);
            }
            finally
            {
                if (gZipStream != null)
                {
                    ((IDisposable)gZipStream).Dispose();
                }
            }
        }
    }
}
