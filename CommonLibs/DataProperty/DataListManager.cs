using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProperty
{
    public class DataListManager
    {
        private static DataListManager _inst = new DataListManager();
        public static DataListManager Inst
        {
            get { return _inst; }
        }

        private Parser _parser;

        private Dictionary<string, DataList> _dataLists;
        internal Dictionary<string, DataList> DataLists
        {
            get { return _dataLists; }
        }

        private DataListManager()
        {
            _parser = new Parser();
            _dataLists = new Dictionary<string, DataList>();
        }

        public bool Parse(string filename,string text = null)
        {
            DataList dataList = _parser.Parse(filename, text);
            if (dataList ==null)
            {
                return false;
            }
            else if (_dataLists.ContainsKey(dataList.Id))
            {
                return true;
            }
            else
            {
                _dataLists.Add(dataList.Id, dataList);
                return true;
            }
        }

        /// <summary>
        /// 获取List
        /// </summary>
        /// <param name="dataListId"></param>
        /// <returns></returns>
        public DataList GetDataList(string dataListId)
        {
            DataList dataList;
            _dataLists.TryGetValue(dataListId, out dataList);
            return dataList;
        }

    }
}
