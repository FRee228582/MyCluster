using CommonUtility;
using DataProperty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerFrameWork
{
    partial class AbstractServer : IConfigAccess
    {
        public void InitPath()
        {
            string pathString = string.Empty;
            string rootPath = string.Empty;
            if (string.IsNullOrEmpty(pathString))
            {
                DirectoryInfo path = new DirectoryInfo(Application.StartupPath);
                if (path.Parent.Exists)
                {
                    rootPath = path.Parent.FullName;
                }
                else
                {
                    Console.WriteLine("Path is error!Please check the input path!");
                }
            }
            else
            {
                rootPath = pathString;
            }
            PathMng.SetPath(rootPath);
        }
        public void InitData()
        {
            string[] files = Directory.GetFiles(PathMng.FullPathFromData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                DataListManager.Inst.Parse(file);
            }

        }


    }
}
