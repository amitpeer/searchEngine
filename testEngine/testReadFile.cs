using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testEngine;
using searchEngine;
using System.Linq;
using System.IO;

namespace testEngine
{
    [TestClass]
    public class TestReadFile
    {
        private ReadFile readFile = new ReadFile("C:\\Users\\amitp\\Documents\\corpusTest");
        List<string> docs = new List<string>();
        List<string> expectedDocs = new List<string>();

        [TestMethod]
        public void testEmptyFile()
        {
            docs = readFile.getFile(1);
            Assert.AreEqual(true, checkEquals());
        }

        [TestMethod]
        public void test2DocsOneLineEach()
        {
            docs = readFile.getFile(2);
            addToExpected("<DOCNO> 1 </DOCNO>\r\n<TEXT>\r\naaa\r\nbbb\r\n</TEXT>\r\n</DOC>");
            Assert.AreEqual(true, checkEquals());
        }
        /*
        [TestMethod]
        public void testOneDocsOneLine()
        {
            docs = readFile.getFile(3);
            addToExpected("aaa");
            Assert.AreEqual(true, checkEquals());
        }

        [TestMethod]
        public void testTwoDocsOneLine()
        {
            docs = readFile.getFile(4);
            addToExpected("aaa\r\nbbb");
            Assert.AreEqual(true, checkEquals());
        }

        [TestMethod]
        public void testTwoFiles()
        {
            docs = readFile.getFiles(2, 4);
            addToExpected("aaa;bbb;aaa");
            Assert.AreEqual(true, checkEquals());
        }

        [TestMethod]
        public void testTwoFilesByList()
        {
            docs = readFile.getFiles(new List<int> { 2, 4 });
            addToExpected("aaa;bbb;aaa\r\nbbb");
            Assert.AreEqual(true, checkEquals());
        }

        [TestMethod]
        public void testThreeFilesByList()
        {
            docs = readFile.getFiles(new List<int> { 2, 3, 4 });
            addToExpected("aaa;bbb;aaa;aaa\r\nbbb");
            Assert.AreEqual(true, checkEquals());
        }
  
    */
  /*      [TestMethod]
        public void testBig()
        {
            docs = readFile.getFile(5);
        }

        [TestMethod]
        public void dontReadStopWords()
        {
            docs = readFile.getFile(9);
            Assert.IsNull(docs);
        }

        [TestMethod]
        public void dontReadStopWords2()
        {
            docs = readFile.getFiles( 6, 6 );
            Assert.IsNull(docs);
        }

        [TestMethod]
        public void dontReadStopWords3()
        {
            docs = readFile.getFiles(new List<int> { 9 });
            Assert.IsNull(docs);
        }

        [TestMethod]
        public void readStopWords()
        {
            string abc = "abc";
            string bac = "bac";
            readFile.ExtractStopWordsFile();
            HashSet<string> stopWords = readFile.getStopWords();       
        }*/

        private bool checkEquals()
        {
            return Enumerable.SequenceEqual(docs.OrderBy(t => t), expectedDocs.OrderBy(t => t));
        }

        private void addToExpected(string doc)
        {
            string[] splittedDoc = doc.Split(';');
            foreach(string s in splittedDoc)
            {
                expectedDocs.Add(s);
            }
        }

        private string getStrBetweenTags(string value, string startTag, string endTag)
        {
            if (value.Contains(startTag) && value.Contains(endTag))
            {
                int index = value.IndexOf(startTag) + startTag.Length;
                return value.Substring(index, value.LastIndexOf(endTag) - index);
            }
            else
                return null;
        }
    }
}
