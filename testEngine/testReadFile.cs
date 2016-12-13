﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testEngine;
using searchEngine;
using System.Linq;

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
            addToExpected("aaa;bbb");
            Assert.AreEqual(true, checkEquals());
        }

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

        [TestMethod]
        public void testBig()
        {
            docs = readFile.getFile(5);
        }

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
    }
}
