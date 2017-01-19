using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    [Serializable]
    public class Document
    {
        private string docName;
        private int max_tf;
        private int numOfUniqueTerms;
        private string language;
        private string date;
        private int documentLength;
        private double magnitudeForCosSim;

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
        public double MagnitudeForCosSim
        {
            get { return magnitudeForCosSim; }
            set { magnitudeForCosSim = value; }
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
        public int DocumentLength
        {
            get { return documentLength; }
            set { documentLength = value; }
        }

        public string getDocumentName()
        {
            return docName;
        }
    }
}
