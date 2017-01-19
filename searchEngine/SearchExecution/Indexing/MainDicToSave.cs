using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine.Indexing
{
    class MainDicToSave
    {
        private Dictionary<string, int[]> mainDic;

        public MainDicToSave() { }

        public MainDicToSave(Dictionary<string, int[]> mainDic)
        {
            this.mainDic = mainDic;
        }

        public Dictionary<string, int[]> MainDic { get { return mainDic; } set { mainDic = value; } }
    }
}
