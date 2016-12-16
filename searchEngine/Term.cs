using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    class Term
    {
        private int df;
        private int tf;
        private string docName;
        private bool isTitle;

        public int Df
        {
            get { return df; }
            set { df = value; }
        }
        public int Tf
        {
            get { return tf; }
            set { tf = value; }
        }
        public string DocName
        {
            get { return docName; }
            set { docName = value; }
        }
        public bool IsTitle
        {
            get { return isTitle; }
            set { isTitle = value; }
        }
    }

}
