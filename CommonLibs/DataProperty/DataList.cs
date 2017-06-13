using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DataProperty
{
    public class DataList:IEnumerable<KeyValuePair<int,Data>>
    {
        string _id;

        public string Id
        {
            get { return _id; }
        }

        private Dictionary<int, Data> _dataListById;
        public Dictionary<int, Data> DataListById
        {
            get { return _dataListById; }
        }

        private Dictionary<string, Data> _dataListByName;
        public Dictionary<string, Data> DataListByName
        {
            get { return _dataListByName; }
        }

        private Dictionary<string, List<Data>> _dataListByGroup;
        public Dictionary<string, List<Data>> DataListByGroup
        {
            get { return _dataListByGroup; }
        }

        internal DataList()
        {
            _dataListById = new Dictionary<int, Data>();
            _dataListByName = new Dictionary<string, Data>();
            _dataListByGroup = new Dictionary<string, List<Data>>();
            //header = new Data();
            //header.SetOwner(this);
        }

        public bool AddData(Data data)
        {
            if (_dataListById.ContainsKey(data.Id))
            {
                LOG.ErrorLine("idspace '{0}' has duplicated id '{1}'",Id,data.Id);
                return false;
            }
            else if (data.Name !=null&&_dataListByName.ContainsKey(data.Name))
            {
                LOG.ErrorLine("idspace '{0}' has duplicated id '{1}'", Id, data.Id);
                return false;
            }

            data.SetOwner(this);
            _dataListById.Add(data.Id, data);
            if (data.Name !=null)
            {
                _dataListByName.Add(data.Name, data);
            }
            if (data.Group!=null)
            {
                List<Data> groupList = null;
                if (_dataListByGroup.ContainsKey(data.Group))
                {
                    groupList = _dataListByGroup[data.Group];
                }
                else
                {
                    groupList = new List<Data>();
                    _dataListByGroup.Add(data.Group, groupList);
                }
                groupList.Add(data);
            }
            return true;
        }

        public Data GetDataById(int id)
        {
            Data data = null;
            _dataListById.TryGetValue(id, out data);
            return data;

        }

        public Data GetDataByName(string name)
        {
            Data data = null;
            _dataListByName.TryGetValue(name, out data);
            return data;
        }

        public List<Data> GetByGroup(string group)
        {
            List<Data> dataList = null;
            _dataListByGroup.TryGetValue(group, out dataList);
            return dataList;
        }

        internal void Init(string dataListId)
        {
            this._id = dataListId;
        }

        public IEnumerator<KeyValuePair<int, Data>> GetEnumerator()
        {
            return _dataListById.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataListById.GetEnumerator();
        }
    }
}
