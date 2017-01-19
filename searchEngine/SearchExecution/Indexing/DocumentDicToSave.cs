using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace searchEngine.Indexing
{
    class DocumentDicToSave
    {
        private Dictionary<string, Document> documentsDic;

        public DocumentDicToSave() { }

        public DocumentDicToSave(Dictionary<string, Document> documentsDic)
        {
            this.documentsDic = documentsDic;
        }

        public Dictionary<string, Document> DocumentsDic { get { return documentsDic; } set { documentsDic = value; } }
    }
    class FrequencyDicToSave
    {
        private Dictionary<string, List<string>> FrequencyDic;

        public FrequencyDicToSave() { }

        public FrequencyDicToSave(Dictionary<string, List<string>> FrequencyDic)
        {
            this.FrequencyDic = FrequencyDic;
        }

        public Dictionary<string, List<string>> DocumentsDic { get { return FrequencyDic; } set { FrequencyDic = value; } }
    }


}
