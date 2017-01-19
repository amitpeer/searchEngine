using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine.SearchExecution.Indexing
{
    class FrequencyDicToSave
    {
        private Dictionary<string, List<string>> frequencyDic;

        public FrequencyDicToSave() { }

        public FrequencyDicToSave(Dictionary<string, List<string>> frequencyDic)
        {
            this.frequencyDic = frequencyDic;
        }

        public Dictionary<string, List<string>> FrequencyDic { get { return frequencyDic; } set { frequencyDic = value; } }
    }
}
