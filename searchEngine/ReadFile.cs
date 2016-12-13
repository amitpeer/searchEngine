﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class ReadFile
    {
        private readonly string path;
        private string[] filePaths;

        public ReadFile(string directoryPath)
        {
            path = directoryPath;
            filePaths = Directory.GetFiles(path);
        }

        // Return a list of string, each item in the list is a document.
        // takes the documents from file number startIndex (included),
        // untill file number endIndex (not included).
        public List <string> getFiles(int startIndex, int endIndex)
        {
            List<string> docList = new List<string>();
            for (int i=startIndex; i<endIndex; i++)
            {
                docList.AddRange(getFile(i));
            }
            return docList;
        }

        // Return a list of string, each item in the list is a document.
        // takes the documents from the file numbers specified in indexList
        public List<string> getFiles(List<int> indexList)
        {
            List<string> docList = new List<string>();
            foreach (int i in indexList)
            {
                docList.AddRange(getFile(i));
            }
            return docList;
        }

        //Returns a list of string, each item in the list is a document.
        // takes the documents from file number fileIndex.
        public List <string> getFile(int fileIndex)
        {
            List<string> docList = new List<string>();
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(filePaths[fileIndex-1]))
                {
                    // Read the stream to a string, and write the string
                    string file = sr.ReadToEnd();
                    string[] delimeters = { "<DOC>", "</DOC>"};
                    string[] splittedFile = file.Split(delimeters , StringSplitOptions.RemoveEmptyEntries);
                    foreach(string s in splittedFile)
                    {
                        docList.Add(s.Trim());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return docList;
        }
    }
}