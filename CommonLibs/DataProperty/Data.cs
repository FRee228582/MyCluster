using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataProperty
{
    public class Data
    {
        private DataList _ownerDataList;

        private int _id;

        public int Id
        {
            get { return _id; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        private string group;
        public string Group
        {
            get { return group; }
        }

    

        private readonly Dictionary<string, Property> _properties;

        public Data()
        {
            _properties = new Dictionary<string, Property>();
        }

        internal void SetOwner(DataList ownerList)
        {
            _ownerDataList = ownerList;
        }

        internal void SetId(int id)
        {
            _id = id;
        }

        internal void SetName(string name)
        {
            _name = name;
        }

        internal bool SetPropetry(Property value)
        {
            if (_properties.ContainsKey(value.Key))
            {
                Logger.LOG.ErrorLine("idspace '{0}' - class '{1}' has more than one property named '{2}'.", _ownerDataList.Id, Id, value.Key);    
            }
            _properties.Add(value.Key, value);
            return true;
        }

        private Property Get(string v)
        {
            Property ret;
            _properties.TryGetValue(v,out ret);
            return ret;
        }

        public string GetString(string v)
        {
            Property prob = Get(v);
            if (prob == null)
            {
                return "";
            }
            return prob.GetString();
        }

  
    }
}
