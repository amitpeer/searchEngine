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

        public Parse()
        {
            dates = new Dictionary<string, string>()
            {
                {"JANUARY","01" },
                 {"JAN","01" },
                 {"FEBUARY","02" },
                {"FEB","02" },
                {"MARCH","03" },
                {"APRIL","04" },
                {"APR","04" },
                {"MAY","05" },
                {"JUNE","06" },
                {"JUN","06" },
                {"JULY","07" },
                {"JUL","07" },
                {"AUGUST","08" },
                {"AUG","08" },
                {"SEPTEMBER","09" },
                {"SEP","09" },
                {"OCTOBER","10" },
                {"OCT","10" },
                {"NOVEMBER","11" },
                {"NOV","11" },
                {"DECEMBER","12" },
                {"DEC","12" },
            };

            }
        public Dictionary<string,int> parseDocument(string doc)
        {
           bool shouldContinue = false;
            char[] delimiters = { ' ', '\n', ';', ':', '"', '(', ')', '[', ']', '{', '}', '*' };
           string[] initialArrayOfDoc= doc.Trim(delimiters).Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, int> terms = new Dictionary<string, int>();
            for(int i = 0; i < initialArrayOfDoc.Length; i++)
            {
                string currentTerm = initialArrayOfDoc[i];
                //doesnt contain a digit then
                if (!currentTerm.Any(char.IsDigit))
                {
                    if (currentTerm.ToUpper() == "BETWEEN")
                    {

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
                                            insertToDic(terms, currentTerm);
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
                                        insertToDic(terms, currentTerm);
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
                    shouldContinue = startWithNumber(terms, initialArrayOfDoc, currentTerm,ref i);
                    if (shouldContinue)
                        continue;
                }
                if(currentTerm!="")
                    insertToDic(terms, currentTerm.Trim(',','.','-'));
            }
            return terms;
        }
        //check what to do 
        private bool startWithNumber(Dictionary<string, int> terms, string[] initialArrayOfDoc, string currentTerm,ref int i)
        {
            bool shouldContinue = false;
            if (currentTerm.Length > 2)
            {
                int valueOfDay;
                //check if ends with th
                if (i<initialArrayOfDoc.Length-1&&currentTerm.Substring(currentTerm.Length - 2) == "th"&& int.TryParse(currentTerm.Substring(0,currentTerm.Length - 2), out valueOfDay)&& valueOfDay>=1&& valueOfDay<=31)
                {
                    string day = valueOfDay<10 ? "0" + valueOfDay.ToString() : valueOfDay.ToString();
                    shouldContinue=checkIfNextIsDate(terms, day, initialArrayOfDoc, ref i);
                    if (shouldContinue)
                        return true;
                }
            }
            else
            {
                int value;
                //check if number
                if(int.TryParse(currentTerm, out value))
                    {
                    if(value >= 1 && value <= 31)
                    {
                        shouldContinue = checkIfNextIsDate(terms, value.ToString(), initialArrayOfDoc, ref i);
                        if (shouldContinue)
                            return true;
                    }
                    //real number



                    }
            }
            return false;
        }
        private bool checkIfNextIsDate(Dictionary<string, int> terms, string day, string[] initialArrayOfDoc, ref int i)
        {
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
                            insertToDic(terms, termToAdd);
                            i = i + 2;
                            return true;                    
                        }
                        else
                        {
                            //check if DD-mm-YYYY
                            if (valueOfYear>=1000&&valueOfYear<=9999)
                            {
                                string termToAdd = valueOfYear + "-" + dates[initialArrayOfDoc[i + 1].ToUpper()] + "-" + day;
                                insertToDic(terms, termToAdd);
                                i = i + 2;
                                return true;
                            }
                        }
                    }
                    else
                         //case of DD month
                     {
                        string termToAdd =dates[initialArrayOfDoc[i + 1].ToUpper()] + "-" + day;
                        insertToDic(terms, termToAdd);
                        i = i + 1;
                         return true;
                      }
                }
              
            }
            return false;
        }

        private void insertToDic(Dictionary<string,int> terms ,string term)
        {
            if (terms.ContainsKey(term))
                terms[term]++;
            else
                terms.Add(term, 1);
        }
        }
    }

