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
        public TermInfoInDoc(int _tf, string _docName, bool _isTitle)
        {
            tf = _tf;
            docName = _docName;
            isTitle = _isTitle;
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
    }

}
