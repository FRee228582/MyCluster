using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DataProperty
{
    internal class Parser
    {
        private string _filename;
        internal DataList Parse(string filename,string text = null)
        {
            this._filename = filename;

            var doc = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    doc.Load(filename);
                }
                else
                {
                    doc.LoadXml(text);
                }
            }
            catch (Exception e)
            {
                LOG.ErrorLine(e);
                return null;
            }

            var dateList = ParseIdspace(doc);

            //if (!ParseHeader(dateList,doc))
            //{
            //    return null;
            //}
            if (!ParseData(dateList,doc))
            {
                return null;
            }
            return dateList;
        }

        private bool ParseData(DataList dateList, XmlDocument doc)
        {
            var xmlData = doc.GetElementsByTagName("class");

            int index = 0;
            foreach (XmlNode xmlDatum in xmlData)
            {
                index++;
                var data = new Data();

                int id = index;
                var idAttribute = xmlDatum.Attributes["id"];
                if (idAttribute !=null)
                {
                    string idString = idAttribute.Value;
                    id = int.Parse(idString);
                    xmlDatum.Attributes.Remove(idAttribute);
                }
                data.SetId(id);

                //Name should be unique too,only if it exists
                var nameAttribute = xmlDatum.Attributes["name"];
                if (nameAttribute !=null && nameAttribute.Value.Length!=0)
                {
                    var name = nameAttribute.Value;
                    data.SetName(name);
                    xmlDatum.Attributes.Remove(xmlDatum.Attributes["name"]);
                }

                var groupAttribute = xmlDatum.Attributes["name"];
                if (groupAttribute != null && groupAttribute.Value.Length != 0)
                {
                    var group = groupAttribute.Value;
                    data.SetName(group);
                    xmlDatum.Attributes.Remove(xmlDatum.Attributes["group"]);
                }

                ParseAttributes(data, xmlDatum.Attributes);
                dateList.AddData(data);
            }
            return true;
        }

        private void ParseAttributes(Data data, XmlAttributeCollection xmlAttributeCollection)
        {
            foreach (XmlAttribute attr in xmlAttributeCollection)
            {
                object outValue;
                var type = ParseValue(attr.Value, out outValue);
                var property = new Property(type,attr.Name,outValue);
                data.SetPropetry(property);
            }
        }

        internal ValueType ParseValue(string input, out object outValue)
        {
            ValueType ret;
            int outInt;
            float outFloat;

            if (int.TryParse(input,out outInt))
            {
                ret = ValueType.INT;
                outValue = outInt;
            }
            else if (float.TryParse(input, out outFloat))
            {
                ret = ValueType.FLOAT;
                outValue = outFloat;
            }
            else
            {
                ret = ValueType.STRING;
                outValue = input;
            }
            return ret;
        }

        private bool ParseHeader(DataList dateList, XmlDocument doc)
        {
            var headers = doc.GetElementsByTagName("header");
            if (headers.Count==0)
            {
                
            }
            else if (headers.Count ==1)
            {
                //var header = dateList.Header;
                //var headerTag = headers.Item(0);
                //ParseAttributes(header, headerTag.Attributes);
            }
            else
            {
                LOG.ErrorLine("File " + _filename + " has more than one header tag");
            }
            return true;
        }

        private DataList ParseIdspace(XmlDocument doc)
        {
            var dataListId = doc.DocumentElement.GetAttribute("id");
            var dataList = DataListManager.Inst.GetDataList(dataListId);
            if (dataList == null)
            {
                dataList = new DataList();
                dataList.Init(dataListId);
            }
            return dataList;
        }
    }
}
