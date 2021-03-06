﻿using SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class Parse
    {
        Dictionary<string, string> dates;
        Dictionary<string, Document> documents = new Dictionary<string, Document>();
        public Dictionary<string, Dictionary<string, int>> frequencies = new Dictionary<string, Dictionary<string, int>>();
        string currentTerm = "";
        HashSet<string> stopWords;
        private bool shouldStem;
        int counterDocs;
        Stemmer stemmer;
        private int documentLength;

        public Parse(HashSet<string> _stopWords, bool _shouldStem)
        {
            stopWords = _stopWords;
            shouldStem = _shouldStem;
            stemmer = new Stemmer();
            dates = new Dictionary<string, string>()
            {
                {"JANUARY","01" },{"JAN","01" },{"FEBUARY","02" },{"FEB","02" },{"MARCH","03" }, {"MAR", "03" },{"APRIL","04" },{"APR","04" },{"MAY","05" },{"JUNE","06" },{"JUN","06" },
                {"JULY","07" },{"JUL","07" },{"AUGUST","08" },{"AUG","08" },{"SEPTEMBER","09" },{"SEP","09" },
                {"OCTOBER","10" },{"OCT","10" },{"NOVEMBER","11" },{"NOV","11" },{"DECEMBER","12" },{"DEC","12" },
            };       
        }

        public Dictionary<string, Document> getDocuments()
        {
            return documents;
        }

        // parrse only a query, not a document
        public string[] parseQuery(string query, bool shouldStem)
        {
            this.shouldStem = shouldStem;
            Dictionary<string, TermInfoInDoc> terms = new Dictionary<string, TermInfoInDoc>();
            parseContent(terms, query, false, "");
            return terms.Keys.ToArray();
        }

        public Dictionary<string, TermInfoInDoc> parseDocument(string doc)
        {
            documentLength = 0;
            counterDocs++;
            Dictionary<string, TermInfoInDoc> terms = new Dictionary<string, TermInfoInDoc>();
            string title;
            Document document = parseHeader(ref doc, out title); // after parseHeader, doc will have only what's between <TEXT> </TEXT>
            // if the document has a title, send it first to be parsed and inserted to the dictionary
            if (title != null)
            {
                parseContent(terms, title, true, document.DocName);
            }
            parseContent(terms, doc, false, document.DocName);
            if (document.DocName != "" && !documents.ContainsKey(document.DocName))
            {
                document.Max_tf = findMaxTf(terms);
                document.NumOfUniqueTerms = terms.Count;          
                documents.Add(document.DocName, document);
                documents[document.DocName].DocumentLength = documentLength;
            }
            return terms;
        }

        private Document parseHeader(ref string doc, out string title)
        {
            Document document = new Document();
            string docName;
            string date;
            string language;
            docName = getStrBetweenTags(doc, "<DOCNO>", "</DOCNO>");
            date = getStrBetweenTags(doc, "<DATE1>", "</DATE1>");
            language = getLanguage(doc, "<F P=105>", "</F>");
            language = fixLanguge(language);
            title = getStrBetweenTags(doc, "<TI>", "</TI>");
            // if there is no <TEXT> tag, it's a test and we leave doc the same:
            doc = getStrBetweenTags(doc, "<TEXT>", "</TEXT>") != null ? getStrBetweenTags(doc, "<TEXT>", "</TEXT>") : doc;
            // if there is a language, we need to cut it from doc
            if (language != null)
            {
                if (doc.IndexOf("</F>") != -1)
                {
                    doc = doc.Substring(doc.IndexOf("</F>"));
                }
            }
            //Insert values to Document
            document.Date = date != null ? date.Trim() : "";
            document.DocName = docName != null ? docName.Trim() : "";
            document.Language = language != null ? language.Trim() : "";
            return document;
        }

        private Dictionary<string,TermInfoInDoc> parseContent(Dictionary <string, TermInfoInDoc> terms, string doc, bool isHeader, string docName)
        {
           bool shouldContinue = false;
           char[] delimiters = { ' ', '\n', ';', ':', '"', '(', ')', '[', ']', '{', '}', '*', '\r' };
           string[] delimitersString = { " ", "\n", "\r", ";", ":", "\"", "(", ")", "[", "]", "{", "}", "*" ,"--","---" };
           string[] initialArrayOfDoc= doc.Trim(delimiters).Split(delimitersString, StringSplitOptions.RemoveEmptyEntries);
           documentLength = initialArrayOfDoc.Length; //insert the length of the doucment to the documents dictionary
           for(int i = 0; i < initialArrayOfDoc.Length; i++)
            {
                string currentTerm = initialArrayOfDoc[i];
                //doesnt contain a digit then
                if (!currentTerm.Any(char.IsDigit))
                {
                    if (currentTerm.ToUpper() == "BETWEEN")
                    {
                        if (i < initialArrayOfDoc.Length - 3)
                        {
                            string part1OfString = initialArrayOfDoc[i + 1];
                            string andPart = initialArrayOfDoc[i + 2];
                            string part2OfString = initialArrayOfDoc[i + 3];
                            double part1v, part2v;
                            if (andPart.ToLower() == "and" && Double.TryParse(part1OfString, out part1v) && Double.TryParse(part2OfString, out part2v))
                            {
                                insertToDic(terms, part1OfString + "-" + part2OfString, isHeader, docName);
                                i = i + 3;
                                continue;
                            }
                        }
                    }
                    if (currentTerm=="United"&& i < initialArrayOfDoc.Length - 1)
                    {
                        if (initialArrayOfDoc[i + 1].TrimEnd(',', '.', '-',';', ' ', '\n', ';', ':', '"', '(', ')', '[', ']', '{', '}', '*') == "States")
                        {
                            insertToDic(terms,"u.s", isHeader, docName);
                            i = i + 1;
                            continue;
                        }
                    }
                    else
                    {
                        ///now check if term is part of a date
                        if (dates.ContainsKey(currentTerm.ToUpper())&&i<initialArrayOfDoc.Length-1)
                        {
                            string nextTerm = initialArrayOfDoc[i + 1];
                            bool hasComma = false;
                            if (nextTerm[nextTerm.Length - 1] == ',')
                            {
                                nextTerm = nextTerm.TrimEnd(',');
                                hasComma = true;
                            }
                                int valueOfDay;
                                if (int.TryParse(nextTerm, out valueOfDay))
                                {
                                    if (valueOfDay > 0 && valueOfDay <= 31)
                                    {
                                        string month = currentTerm;
                                        currentTerm = "";
                                        string sValueOfDay = "";
                                        sValueOfDay+=valueOfDay;
                                        if (valueOfDay < 10)
                                            sValueOfDay = "0" + sValueOfDay;
                                        currentTerm += dates[month.ToUpper()] + "-" + sValueOfDay;
                                        //the doc is not long enough
                                        int valueOfyear;                                     
                                        //check if MM-DD-YYYY
                                        if (i < initialArrayOfDoc.Length - 2&&hasComma &&int.TryParse(initialArrayOfDoc[i + 2], out valueOfyear)&& valueOfyear>999&& valueOfyear<=9999)
                                        {
                                            currentTerm = valueOfyear + "-" + currentTerm;                                              
                                            i = i + 2;
                                        }
                                        else
                                        {
                                        //insert MM-DD
                                            insertToDic(terms, currentTerm, isHeader, docName);
                                            i = i + 1;
                                            continue;
                                        }
                                    }
                                    //check if mm-yyyy
                                    else
                                    {
                                        int valueOfYear = valueOfDay;
                                        if(valueOfDay > 999 && valueOfDay <= 9999)
                                        {
                                            string month = currentTerm;
                                            currentTerm = "";
                                            currentTerm = valueOfYear + "-" + dates[month.ToUpper()];
                                            insertToDic(terms, currentTerm, isHeader, docName);
                                            i = i + 1;
                                         continue;
                                        }
                                    }
                                }
                            }
                    }
                }
                else
                {
                    shouldContinue = startWithNumber(terms, initialArrayOfDoc, currentTerm,ref i, isHeader, docName);
                    if (shouldContinue)
                        continue;
                }
                if(currentTerm!="")
                    insertToDic(terms, currentTerm.Trim(',','.','-').ToLower(), isHeader, docName);
            }
            return terms;
        }
        //check what to do 
        private bool startWithNumber(Dictionary<string, TermInfoInDoc> terms, string[] initialArrayOfDoc, string currentTerm, ref int i, bool isHeader, string docName)
        {
            double value;
            bool shouldContinue = false;
            bool foundNext = false;
            if (currentTerm.Length > 2)
            {
                int valueOfDay;
                //check if ends with th
                if (i < initialArrayOfDoc.Length - 1 && currentTerm.Substring(currentTerm.Length - 2) == "th" && int.TryParse(currentTerm.Substring(0, currentTerm.Length - 2), out valueOfDay) && valueOfDay >= 1 && valueOfDay <= 31)
                {
                    string day = valueOfDay < 10 ? "0" + valueOfDay.ToString() : valueOfDay.ToString();
                    shouldContinue = checkIfNextIsDate(terms, day, initialArrayOfDoc, ref i, isHeader, docName);
                    if (shouldContinue)
                        return true;
                }
                //check if has ending "bn"- billion
                if (currentTerm.Substring(currentTerm.Length - 2) == "bn")
                {
                    //double value;
                    double.TryParse(currentTerm.Substring(0, currentTerm.Length - 2), out value);
                    //should check the rest of doc
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, value * 1000.0 + "M", isHeader, docName);
                        return false;
                    }
                    string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, "bn", isHeader, docName);
                    insertToDic(terms, toInsert, isHeader, docName);
                    return foundNext;
                }
            }
                //check if clean number
                if (double.TryParse(currentTerm, out value))
                {
                    //check if part of a date
                    if (value >= 1 && value <= 31)
                    {
                        shouldContinue = checkIfNextIsDate(terms, value.ToString(), initialArrayOfDoc, ref i, isHeader, docName);
                        if (shouldContinue)
                            return true;
                    }
                    //real number clean
                     foundNext = false;
                    if (i >= initialArrayOfDoc.Length - 1)
                         {
                          if (value >= 1000000)
                            {
                             insertToDic(terms, value/1000000.0+"M", isHeader, docName);
                             return false;
                            }
                          else
                          return false;
                         }                       
                    string next = checkNextTerms(initialArrayOfDoc, terms, ref i, value,ref foundNext, "", isHeader, docName);
                    if (foundNext)
                        insertToDic(terms, next, isHeader, docName);
                    return foundNext;
                }
            //term that contains digits...
            //check if dollar
            if (currentTerm[0] == '$')
            {
                string flag = "$";
                //double value;
                if (Double.TryParse(currentTerm.Substring(1), out value))
                {
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, value + " Dollars", isHeader, docName);
                        return true;
                    }                      
                    string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, flag, isHeader, docName);
                    insertToDic(terms, toInsert, isHeader, docName);
                    return foundNext;
                }
                return false;
            }
            //check if has percentage
            if (currentTerm[currentTerm.Length - 1] == '%')
            {
                if (Double.TryParse(currentTerm.Substring(0, currentTerm.Length - 1), out value))
                {
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, currentTerm, isHeader, docName);
                        return true;
                    }
                }
            }
                //check if has ending m for million.
                if (currentTerm[currentTerm.Length - 1] == 'm')
                    {
                        //double value;
                        if (Double.TryParse(currentTerm.TrimEnd('m'), out value))
                        {
                            if (i >= initialArrayOfDoc.Length - 1)
                                 {
                                 insertToDic(terms, value + "M", isHeader, docName);
                                 return true;
                                 }
                        string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, "m", isHeader, docName);
                        insertToDic(terms, toInsert, isHeader, docName);
                        return foundNext;
                        }
                    }         
            return false;
        }

        private string checkNextTerms(string[] initialArrayOfDoc, Dictionary<string, TermInfoInDoc> terms, ref int currentIndex, double number, ref bool foundNext, string flag, bool isHeader, string docName)
        {
            int index = currentIndex + 1;
            string ans = "";
            if (flag == "m" || flag == "bn")
            {
                if (initialArrayOfDoc[index].ToLower() == "dollars")
                {
                    foundNext = true;
                    currentIndex = currentIndex + 1;
                    number = (flag == "m") ? number : number * 1000.0;
                    ans = number + "" + "M Dollars";
                    return ans;
                }
            }
            string nextTerm = initialArrayOfDoc[index].ToLower();
            //check if there is a fraction after number
            double numerator = 1.0, denominator = 1.0;
            if (nextTerm.Contains("/"))
            {
                string[] fraction = nextTerm.Split('/');
                if (Double.TryParse(fraction[0], out numerator) && Double.TryParse(fraction[1], out denominator))
                {
                    ans = number + " " + nextTerm;
                    foundNext = true;
                    currentIndex = currentIndex + 1;
                    if (index < initialArrayOfDoc.Length - 1)
                    {
                        index = index + 1;
                        nextTerm = initialArrayOfDoc[index].ToLower();
                    }
                    else
                    {
                        return ans;
                    }
                }
            }
            double howMuchToMultiply = 1.0;
            switch (nextTerm)
            {
                case "dollars":
                    foundNext = true;
                    currentIndex = currentIndex + 1;
                    if (ans == "")
                    {
                        if (number >= 1000000)
                        {
                            number = number / 1000000.0;
                            ans = number + "M";
                        }
                        else
                            ans = number.ToString();
                    }
                    return ans + " Dollars";
                    break;
                case "gmt":
                    if(initialArrayOfDoc[currentIndex].Length==4&&initialArrayOfDoc[currentIndex+1]=="GMT"&&number%100<=60&&number/100<=24)
                    {
                        foundNext = true;
                        ans = initialArrayOfDoc[currentIndex].Substring(0, 2) + ":" + initialArrayOfDoc[currentIndex].Substring(2);
                        currentIndex = currentIndex + 1;
                        return ans;
                    }
                    break;
                case "percent":
                    foundNext = true;
                    currentIndex = currentIndex + 1;
                    if (ans == "")
                    {
                        if (number >= 1000000)
                        {
                            number = number / 1000000.0;
                            ans = number + "M";
                        }
                        else
                            ans = number.ToString();
                    }
                    return ans + "%";
                    break;
                case "percentage":
                    foundNext = true;
                    currentIndex = currentIndex + 1;
                    if (ans == "")
                    {
                        if (number >= 1000000)
                        {
                            number = number / 1000000.0;
                            ans = number + "M";
                        }
                        else
                            ans = number.ToString();
                    }
                    return ans + "%";
                    break;
                case "million":
                    currentIndex = currentIndex + 1;
                    foundNext = true;
                    howMuchToMultiply = 1;
                    break;
                case "billion":
                    currentIndex = currentIndex + 1;
                    foundNext = true;
                    howMuchToMultiply = 1000;
                    break;
                case "trillion":
                    currentIndex = currentIndex + 1;
                    foundNext = true;
                    howMuchToMultiply = 1000000;
                    break;
            }
            if (flag == "$")
            {
                if (foundNext)
                    return (number * howMuchToMultiply) + "M Dollars";
                else
                {
                    if (number >= 1000000)
                    {
                        foundNext = true;
                        return number/1000000.0 + "M Dollars";
                    }
                    else
                    {
                        return number + " Dollars";
                    }
                }
            }
            if (foundNext)
            {
                index = (currentIndex);
                if (ans == "")
                     {
                    ans = number * howMuchToMultiply + "M";
                     }
                else
                    {
                    double fraction = numerator / denominator;
                    number = number + fraction;
                    ans = number * howMuchToMultiply + "M";
                    }
                if (index < initialArrayOfDoc.Length - 1)
                {
                    if (initialArrayOfDoc[index].ToLower() == "dollars")
                    {
                        currentIndex = currentIndex + 1;
                        return ans + " Dollars";
                    }
                    if (initialArrayOfDoc[currentIndex+1].ToUpper() == "U.S" && initialArrayOfDoc[currentIndex + 2].ToLower() == "dollars")
                    {
                        currentIndex = currentIndex + 2;
                        return ans + " Dollars";
                    }
                }
            }
                if ((number >= 1000000))
                {
                foundNext = true;
                number = number / 1000000.0;
                    ans = number + "M";
                }
                return ans;
        }

        private bool checkIfNextIsDate(Dictionary<string, TermInfoInDoc> terms, string day, string[] initialArrayOfDoc, ref int i, bool isHeader, string docName)
        {
            if (i+1 >= initialArrayOfDoc.Length)
            {
                return false;
            }
            if (dates.ContainsKey(initialArrayOfDoc[i + 1].ToUpper())){
                if(i+1 < initialArrayOfDoc.Length - 1)
                {
                    int valueOfYear;
                    //check if the next next value is a number
                    if(int.TryParse(initialArrayOfDoc[i+2],out valueOfYear))
                    {
                        //check if DD-mm-yy
                        if (valueOfYear >= 0 && valueOfYear <= 99)
                        {
                            string yearPrefix;
                            yearPrefix = (valueOfYear >= 0 && valueOfYear <= 20) ? "20" :  "19";
                            string termToAdd = yearPrefix + "" + valueOfYear + "-" + dates[initialArrayOfDoc[i + 1].ToUpper()] + "-" + day;
                            insertToDic(terms, termToAdd, isHeader, docName);
                            i = i + 2;
                            return true;                    
                        }
                        else
                        {
                            //check if DD-mm-YYYY
                            if (valueOfYear>=1000&&valueOfYear<=9999)
                            {
                                string termToAdd = valueOfYear + "-" + dates[initialArrayOfDoc[i + 1].ToUpper()] + "-" + day;
                                insertToDic(terms, termToAdd, isHeader, docName);
                                i = i + 2;
                                return true;
                            }
                        }
                    }
                    else
                         //case of DD month
                     {
                        string termToAdd =dates[initialArrayOfDoc[i + 1].ToUpper()] + "-" + day;
                        insertToDic(terms, termToAdd, isHeader, docName);
                        i = i + 1;
                         return true;
                      }
                }         
            }
            return false;
        }

        private void insertToDic(Dictionary<string,TermInfoInDoc> terms ,string term, bool isHeader, string docName)
        {
            if (term == "<f>" || term == "</f>" || term == "<f" || term =="p=106>"  || term == "'" || term == "" || term == " " || term == "|" || term == null || term == "$")
                return;
            if (term.All(c => c == '?' || c == '!' || c == '$' || c == '#'))
                return;
            if (term.Contains('?'))
                term = term.Replace("?", "").Trim(); ;
            if (term.Contains('!'))
                term = term.Replace("!", "").Trim();
            term = term.Trim('|', '|', '`', '/', '-', '\'', '_');
            if (shouldStem)
            {
                term = stemmer.stemTerm(term);
            }
            term = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(term));
            
            // case there are stop words to check 
            if (stopWords != null && stopWords.Contains(term))
            {
                return;
            }
            // case there are NOT stop words
            else if (term != "" )
            {
                if (currentTerm == ""&&!term.Any(char.IsDigit))
                {
                    currentTerm = term;
                }
                else
                {
                    if (!term.Any(char.IsDigit)){
                    if (frequencies.ContainsKey(currentTerm))
                    {
                        if (frequencies[currentTerm].ContainsKey(term))
                        {
                            frequencies[currentTerm][term]++;
                        }
                        else
                        {
                            frequencies[currentTerm].Add(term, 1);
                        }
                    }
                    else
                    {
                        frequencies.Add(currentTerm, new Dictionary<string, int>());
                    }
                    currentTerm = term;
                    }
                }
                if (terms.ContainsKey(term))
                {
                    terms[term].Tf++;
                }
                else
                {
                    terms.Add(term, new TermInfoInDoc(1, docName != null ? docName.Trim() : null, isHeader));
                }
             }
        }

        private string getStrBetweenTags(string value, string startTag, string endTag)
        {
            if (value.Contains(startTag) && value.Contains(endTag))
            {
                int index = value.IndexOf(startTag) + startTag.Length;
                return value.Substring(index, value.IndexOf(endTag) - index);
            }
            else
                return null;
        }

        private string getLanguage(string value, string startTag, string endTag)
        {
            if (value.Contains(startTag) && value.Contains(endTag))
            {
                int index = value.IndexOf(startTag) + startTag.Length;
                string valueFromStartTag = value.Substring(index);
                return valueFromStartTag.Substring(0, valueFromStartTag.IndexOf(endTag)).Trim();
            }
            else
                return null;
        }

        private int findMaxTf(Dictionary<string, TermInfoInDoc> terms)
        {
            int max_tf = 0;
            foreach(KeyValuePair<string, TermInfoInDoc> entry in terms)
            {
                if (entry.Value.Tf > max_tf)
                    max_tf = entry.Value.Tf;
            }
            return max_tf;    
        }

        private string fixLanguge(string language)
        {
            string ans = language;
            if (ans != null)
            {
                if (ans.Contains(" "))
                    ans = language.Substring(0, language.IndexOf(" "));
                if (ans.Contains(","))
                    ans = language.Substring(0, language.IndexOf(","));
                if (ans.Any(char.IsDigit))
                {
                    ans = null;
                }
            }
            return ans;
        }
    }
}