using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class Document
    {
        private string docName;
        private int max_tf;
        private int numOfUniqueTerms;
        private string language;
        private string date;

        public string DocName
        {
            get { return docName; }
            set { docName = value; }
        }
        public int Max_tf
        {
            get { return max_tf; }
            set { max_tf = value; }
        }
        public int NumOfUniqueTerms
        {
            get { return numOfUniqueTerms; }
            set { numOfUniqueTerms = value; }
        }
        public string Language
        {
            get { return language; }
            set { language = value; }
        }
        public string Date
        {
            get { return date; }
            set { date = value; }
        }
    }
}
