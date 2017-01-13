using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class TermInfoInDoc
    {
        private int tf;
        private string docName;
        private bool isTitle;

        public TermInfoInDoc() { }
        public TermInfoInDoc(int _tf, string docName, bool isTitle)
        {
            tf = _tf;
            this.docName = docName;
            this.isTitle = isTitle;
        }

        public int Tf
        {
            get { return tf; }
            set { tf = value; }
        }
        public bool IsTitle
        {
            get { return isTitle; }
            set { isTitle = value; }
        }
        public string DocName
        {
            get { return docName; }
            set { docName = value; }
        }

        public bool equals(TermInfoInDoc other)
        {
            return other.docName == docName && other.isTitle == isTitle && other.tf == tf;
        }
    }

}
