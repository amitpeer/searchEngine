using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine
{
    public class TermInfoInDoc
    {
        private bool isTitle;
        private int tf;
        private string docName;

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
    }

}
