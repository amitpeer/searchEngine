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
            char[] delimiters = { ' ', '\n', ';', ':', '"', '(', ')', '[', ']', '{', '}', '*', '\r' };
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
        private bool startWithNumber(Dictionary<string, int> terms, string[] initialArrayOfDoc, string currentTerm, ref int i)
        {
            bool shouldContinue = false;
            bool foundNext = false;
            if (currentTerm.Length > 2)
            {
                int valueOfDay;
                //check if ends with th
                if (i < initialArrayOfDoc.Length - 1 && currentTerm.Substring(currentTerm.Length - 2) == "th" && int.TryParse(currentTerm.Substring(0, currentTerm.Length - 2), out valueOfDay) && valueOfDay >= 1 && valueOfDay <= 31)
                {
                    string day = valueOfDay < 10 ? "0" + valueOfDay.ToString() : valueOfDay.ToString();
                    shouldContinue = checkIfNextIsDate(terms, day, initialArrayOfDoc, ref i);
                    if (shouldContinue)
                        return true;
                }
            }
            else
            {
                double value;
                //check if clean number
                //bool foundNext=false;
                if (double.TryParse(currentTerm, out value))
                {
                    if (value >= 1 && value <= 31)
                    {
                        shouldContinue = checkIfNextIsDate(terms, value.ToString(), initialArrayOfDoc, ref i);
                        if (shouldContinue)
                            return true;
                    }
                    //real number clean
                     foundNext = false;
                    if (i >= initialArrayOfDoc.Length - 1)
                        return false;
                        string next = checkNextTerms(initialArrayOfDoc, terms, ref i, value,ref foundNext, "");
                    if (foundNext)
                    {
                        insertToDic(terms, next);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            //term that contains digits...
            //check if dollar

            if (currentTerm[0] == '$')
            {
                string flag = "$";
                double value;
                if (Double.TryParse(currentTerm.Substring(1), out value))
                {
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, value + " Dollars");
                        return true;
                    }                      
                    string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, flag);
                    insertToDic(terms, toInsert);
                    return foundNext;
                }
                return false;
            }
            //check if has ending m for million.
            if (currentTerm[currentTerm.Length - 1] == 'm')
            {
                double value;
                if (Double.TryParse(currentTerm.TrimEnd('m'), out value))
                {
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, value + "M");
                        return false;
                    }
                    string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, "m");
                    insertToDic(terms, toInsert);
                    return foundNext;
                }
            }
            //check if has ending "bn"- billion
            if (currentTerm.Length > 2)
            {
                if (currentTerm.Substring(currentTerm.Length - 2) == "bn")
                {
                    double value;
                    double.TryParse(currentTerm.Substring(0, currentTerm.Length - 2), out value);
                    //should check the rest of doc
                    if (i >= initialArrayOfDoc.Length - 1)
                    {
                        insertToDic(terms, value*1000.0+"M");
                        return false;
                    }                        
                    string toInsert = checkNextTerms(initialArrayOfDoc, terms, ref i, value, ref foundNext, "bn");
                    insertToDic(terms, toInsert);
                    return foundNext;
                }
            }
            return false;
        }
        private string checkNextTerms(string[] initialArrayOfDoc, Dictionary<string, int> terms, ref int currentIndex, double number, ref bool foundNext, string flag)
        {
            int index = currentIndex + 1;
            string ans = "";
            switch (flag)
            {
                case "bn":
                    if (initialArrayOfDoc[index].ToLower() == "dollars")
                    {
                        foundNext = true;
                        currentIndex = currentIndex + 1;
                        ans = number * 1000 + "" + " M Dollars";
                        return ans;
                    }
                    break;
                case "m":
                    if (initialArrayOfDoc[index].ToLower() == "dollars")
                    {
                        foundNext = true;
                        currentIndex = currentIndex + 1;
                        ans = number + "" + " M Dollars";
                        return ans;
                    }
                    break;

            }
            string nextTerm = initialArrayOfDoc[index].ToLower();
            //check if there is a fraction after number
            double numerator = 1.0, denominator = 1.0;
            if (nextTerm.Contains("/"))
            {
                string[] fraction = nextTerm.Split('/');
                if (Double.TryParse(fraction[0], out numerator) && Double.TryParse(fraction[1], out denominator))
                {
                    ans = number + "" + nextTerm;
                    if (index < initialArrayOfDoc.Length - 1)
                    {
                        foundNext = true;
                        index = index + 1;
                        currentIndex = currentIndex + 1;
                        nextTerm = initialArrayOfDoc[index].ToLower();
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
                        return number + "M Dollars";
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
                    if (index < initialArrayOfDoc.Length - 1 && initialArrayOfDoc[index].ToUpper() == "U.S" && initialArrayOfDoc[index + 1].ToLower() == "dollars")
                    {
                        currentIndex = currentIndex + 2;
                        return ans + " Dollars";

                    }
                }
            }
            return ans;
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

