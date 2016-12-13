using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using searchEngine;
using System.Collections.Generic;

namespace testEngine
{
    [TestClass]
    public class UnitTest1
    {
        Parse parse = new Parse();
        Dictionary<string, int> terms;
        Dictionary<String, int> expectedTerms = new Dictionary<string, int>();
        string doc;

        [TestMethod]
        public void TestOnlyAdam()
        {
            doc = "adam*";
            terms = parse.parseDocument(doc);
            expectedTerms.Add("adam", 1);
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDD()
        {
            doc = "June 4";
            terms = parse.parseDocument(doc);
            expectedTerms.Add("06-04", 1);
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDDWithAdam()
        {
            doc = "June 4 adam July 5";
            terms = parse.parseDocument(doc);
            expectedTerms.Add("06-04", 1);
            expectedTerms.Add("adam", 1);
            expectedTerms.Add("07-05", 1);
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDDYYYY()
        {
            doc = "April 28, 1990";
            terms = parse.parseDocument(doc);
            expectedTerms.Add("1990-04-28", 1);
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthYYYY()
        {
            doc = "May 1994";
            terms = parse.parseDocument(doc);
            expectedTerms.Add("1994-05", 1);
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthMixed()
        {
            doc = "In may 1994 adam was nash-nash. May was pretty. jan 28, 2005 july 7 june 8000 october fest";
            terms = parse.parseDocument(doc);
            insertToExpected("In;1994-05;adam;was;nash-nash;May;was;pretty;2005-01-28;07-07;8000-06;october;fest");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testMonthMixed2()
        {
            doc = "may 5  may 5";
            terms = parse.parseDocument(doc);
            insertToExpected("05-05;05-05");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testNumberFirstDate()
        {
            doc = "16th march 1990";
            terms = parse.parseDocument(doc);
            insertToExpected("1990-03-16");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testNumberFirstDate2()
        {
            doc = "16th march 91";
            terms = parse.parseDocument(doc);
            insertToExpected("1991-03-16");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testNumberFirstDate3()
        {
            doc = "adam 10th feb 15 amit";
            terms = parse.parseDocument(doc);
            insertToExpected("adam;2015-02-10;amit");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate4()
        {
            doc = "adam 14 may 1991 amit";
            terms = parse.parseDocument(doc);
            insertToExpected("adam;1991-05-14;amit");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate5()
        {
            doc = "adam 14 may amit yonatan. 78 aug * * jan 50";
            terms = parse.parseDocument(doc);
            insertToExpected("adam;05-14;amit;yonatan;78;aug;jan;50");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate6()
        {
            doc = "$100";
            terms = parse.parseDocument(doc);
            insertToExpected("100 Dollars");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testNumberFirstDate7()
        {
            doc = "$450,000";
            terms = parse.parseDocument(doc);
            insertToExpected("450000 Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate8()
        {
            doc = "$100.8";
            terms = parse.parseDocument(doc);
            insertToExpected("100.8 Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate9()
        {
            doc = "$100 million";
            terms = parse.parseDocument(doc);
            insertToExpected("100M Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        private void insertToExpected(string doc)
        {
            string[] docArray = doc.Split(';');
            foreach(string s in docArray)
            {
                if(expectedTerms.ContainsKey(s))
                    expectedTerms[s]++;
                else
                {
                    expectedTerms.Add(s, 1);
                }
            }
        }

        private bool checkEqulas()
        {
            bool equal = false;
            if (expectedTerms.Count == terms.Count) // Require equal count.
            {
                equal = true;
                foreach (var pair in terms)
                {
                    int value;
                    if (expectedTerms.TryGetValue(pair.Key, out value))
                    {
                        // Require value be equal.
                        if (value != pair.Value)
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        // Require key be present.
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }
    }
}
