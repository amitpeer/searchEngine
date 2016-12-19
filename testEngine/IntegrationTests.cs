using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using searchEngine;
using System.Linq;

namespace testEngine
{
    [TestClass]
    public class IntegrationTests
    {

        Parse parse = new Parse(new HashSet<string> { { "a" } }, false);
        List<Dictionary<string, TermInfoInDoc>> terms = new List<Dictionary<string, TermInfoInDoc>>();
        Dictionary<string, Document> documents = new Dictionary<string, Document>();
        Dictionary<String, TermInfoInDoc> expectedTerms = new Dictionary<string, TermInfoInDoc>();
        private ReadFile readFile = new ReadFile("C:\\Users\\amitp\\Documents\\corpusTest");
        List<string> docs = new List<string>();

        [TestMethod]
        public void TestMethod1()
        {
            docs = readFile.getFiles(5, 6);
            foreach(string s in docs)
            {
                terms.Add(parse.parseDocument(s));               
            }
            documents = parse.getDocuments();
            Assert.AreEqual(3, documents["1"].Max_tf);
            Assert.AreEqual(4, documents["1"].NumOfUniqueTerms);
        }

    }
}
