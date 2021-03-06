﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using searchEngine;
using System.Collections.Generic;
using System.IO;

namespace testEngine
{
    [TestClass]
    public class testParser
    {   
        Parse parse = new Parse(new HashSet<string> { { "a" } }, false);
        Dictionary<string, TermInfoInDoc> terms;
        Dictionary<String, TermInfoInDoc> expectedTerms = new Dictionary<string, TermInfoInDoc>();
        string doc;

        [TestMethod]
        public void TestOnlyAdam()
        {
            doc = "adam*";
            terms = parse.parseDocument(doc);
            insertToExpected("adam");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDD()
        {
            doc = "June 4";
            terms = parse.parseDocument(doc);
            insertToExpected("06-04");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDDWithAdam()
        {
            doc = "June 4 adam July 5";
            terms = parse.parseDocument(doc);
            insertToExpected("06-04;adam;07-05");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthDDYYYY()
        {
            doc = "April 28, 1990";
            terms = parse.parseDocument(doc);
            insertToExpected("1990-04-28");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthYYYY()
        {
            doc = "May 1994";
            terms = parse.parseDocument(doc);
            insertToExpected("1994-05");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testMonthMixed()
        {
            doc = "In may 1994 adam was nash-nash. May was pretty. jan 28, 2005 july 7 june 8000 october fest";
            terms = parse.parseDocument(doc);
            insertToExpected("in;1994-05;adam;was;nash-nash;may;was;pretty;2005-01-28;07-07;8000-06;october;fest");
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
        [TestMethod]
        public void testNumberFirstDate10()
        {
            doc = "3/4 llion";
            terms = parse.parseDocument(doc);
            insertToExpected("3/4;llion");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberUnderMillion1()
        {
            doc = "1204; 35.66 35 3/4 ";
            terms = parse.parseDocument(doc);
            insertToExpected("1204;35.66;35 3/4");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testPercent()
        {
            doc = "104 percent 10.6 percentage 96% ";
            terms = parse.parseDocument(doc);
            insertToExpected("104%;10.6%;96%");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testPrices1()
        {
            doc = "1.7320 Dollars 22 3/4 Dollars $450000 ";
            terms = parse.parseDocument(doc);
            insertToExpected("1.732 Dollars;22 3/4 Dollars;450000 Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testPrices2()
        {
            doc = "1000000 Dollars $450000000 $100 million 20.6m Dollars $100 billion 150bn Dollars ";
            terms = parse.parseDocument(doc);
            insertToExpected("1M Dollars;450M Dollars;100M Dollars;20.6M Dollars;100000M Dollars;150000M Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testPrices3()
        {
            doc = "100 billion U.S dollars 320 million U.S dollars 1 trillion U.S dollars ";
            terms = parse.parseDocument(doc);
            insertToExpected("100000M Dollars;320M Dollars;1000000M Dollars");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate11()
        {
            doc = "1,000,000 1,234,567 7 Million 7 Billion 7 Trillion";
            terms = parse.parseDocument(doc);
            insertToExpected("1M;1.234567M;7M;7000M;7000000M");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNumberFirstDate12()
        {
            doc = "between 5 and 6 shalom moshe";
            terms = parse.parseDocument(doc);
            insertToExpected("5-6;shalom;moshe");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNewRule1()
        {
            doc = " shalom 0940 GMT moshe";
            terms = parse.parseDocument(doc);
            insertToExpected("shalom;09:40;moshe");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNewRule2()
        {
            doc = " shalom 0940 GMT moshe United States 77";
            terms = parse.parseDocument(doc);
            insertToExpected("shalom;09:40;moshe;u.s;77");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNewRule3()
        {
            doc = " shalom 0940 GMT moshe United States";
            terms = parse.parseDocument(doc);
            insertToExpected("shalom;09:40;moshe;u.s");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNewRule4()
        {
            doc = " U.S. 0940 GMT moshe ";
            terms = parse.parseDocument(doc);
            insertToExpected("u.s;09:40;moshe");
            Assert.AreEqual(true, checkEqulas());
        }
        [TestMethod]
        public void testNewRule5()
        {
            doc = "Geneva, March 21 (CNA) -- The Convention on";
            terms = parse.parseDocument(doc);
        }
        //[TestMethod]
        //public void testNewRule6()
        //{
        //    StreamReader streamReader = new StreamReader("C:\\Users\\amitp\\Documents\\corpusTest\\FB396004");
        //    doc = streamReader.ReadToEnd();
        //    terms = parse.parseDocument(doc);
        //    streamReader.Close();
        //}
        [TestMethod]
        public void testNewRule7()
        {
            doc = "Geneva, March 21 (CNA) -- The Convention on United States, ";
            terms = parse.parseDocument(doc);
        }

        [TestMethod]
        public void testHeader1()
        {
            doc = "<DOCNO> FBIS3-900 </DOCNO> <F P=105> Mandarin </F> <TEXT> text </TEXT>";
            terms = parse.parseDocument(doc);
            insertToExpected("text", false, "FBIS3-900");
            Assert.AreEqual(true, checkEqulas());
            Assert.AreEqual("Mandarin", parse.getDocuments()[terms["text"].DocName].Language);
        }

        [TestMethod]
        public void testQuestionMarks()
        {
            doc = "<TEXT> !!!, ???, hello?, hello!, ?bubu?, ?!?!???!!, a???!!?!,  </TEXT>";
            terms = parse.parseDocument(doc);
            insertToExpected("hello;bubu");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testGereshMarks()
        {
            doc = "<TEXT>'</TEXT>";
            terms = parse.parseDocument(doc);
            insertToExpected("");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testStemmer()
        {
            parse = new Parse(new HashSet<string> { { "a" } }, true);
            doc = "<TEXT> works, work, working bubu</TEXT>";
            terms = parse.parseDocument(doc);
            insertToExpected("work;bubu");
            Assert.AreEqual(true, checkEqulas());
        }

        [TestMethod]
        public void testLanguage()
        {
            doc = "<F P=105> 1231 </F>";
            terms = parse.parseDocument(doc);
        }

        [TestMethod]
        public void testDocLengthEmptyDoc()
        {
            doc = "<DOCNO> a </DOCNO> <TEXT> </TEXT>";
            terms = parse.parseDocument(doc);
            Assert.AreEqual(0, parse.getDocuments()["a"].DocumentLength);
        }

        [TestMethod]
        public void testDocLengthOneWord()
        {
            doc = "<DOCNO> a </DOCNO> <TEXT> 1 </TEXT>";
            terms = parse.parseDocument(doc);
            Assert.AreEqual(1, parse.getDocuments()["a"].DocumentLength);
        }

        [TestMethod]
        public void testDocLengthManyWord()
        {
            doc = "<DOCNO> a </DOCNO> <TEXT> hello, I am nash nash </TEXT>";
            terms = parse.parseDocument(doc);
            Assert.AreEqual(5, parse.getDocuments()["a"].DocumentLength);
        }

        [TestMethod]
        public void testDocLengthanyWord2()
        {
            doc = "<DOCNO> a </DOCNO> <TEXT> hello, who are you? I am- not me-- </TEXT>";
            terms = parse.parseDocument(doc);
            Assert.AreEqual(8, parse.getDocuments()["a"].DocumentLength);
        }

        private void insertToExpected(string doc)
        {
            insertToExpected(doc, false, null);
        }

        private void insertToExpected(string doc, bool isTitle, string docName)
        {
            string[] docArray = doc.Split(';');
            foreach(string s in docArray)
            {
                if (expectedTerms.ContainsKey(s))
                {
                    expectedTerms[s].Tf++;
                }
                else
                {
                    expectedTerms.Add(s, new TermInfoInDoc(1, docName, isTitle));
                }
            }
        }

        private bool checkEqulas()
        {
            bool equal = true;
            List<string> termKeyList = new List<string>(this.terms.Keys);
            List<string> expectedKeyList = new List<string>(this.expectedTerms.Keys);
            int i = 0;
            foreach(string key in termKeyList)
            {
                if (expectedKeyList[i] != key)
                {
                    equal = false;
                    break;
                }
                i++;
            }
            return equal;
           /* if (expectedTerms.Count == terms.Count) // Require equal count.
            {
                equal = true;
                foreach (var pair in terms)
                {
                    TermInfoInDoc value;
                    if (expectedTerms.TryGetValue(pair.Key, out value))
                    {
                        // Require value be equal.
                        if (!pair.Key.Equals(pair.Value.DocName))
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
            return equal;*/
        }
    }
}
