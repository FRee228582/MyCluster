using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtility
{
    public class PathMng
    {
        static private string basePath = string.Empty;
        static private string dataPath = string.Empty;
        static private string clientDataPath = string.Empty;
        static private string shareDataPath = string.Empty;
        static private string toolPath = string.Empty;
        static private string serverPath = string.Empty;

        static public void SetPath(string inputPath)
        {
            basePath = inputPath;
            dataPath = PathCombine(inputPath, "Data");
            clientDataPath = PathCombine(inputPath, "Shared");
            shareDataPath = PathCombine(inputPath, "Client");
            toolPath = PathCombine(inputPath, "Tool");
            serverPath = PathCombine(inputPath, "Server");
        }

        private static string PathCombine(string inputPath, string folder)
        {
            string newPath = Path.Combine(inputPath, folder);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;
        }

        static public string FullPathFromBase(string subDir)
        {
            return PathCombine(basePath, subDir);
        }

        static public string FullPathFromData(string subDir)
        {
            return PathCombine(dataPath, subDir);
        }

        static public string FullPathFromClientData(string subDir)
        {
            return PathCombine(clientDataPath, subDir);
        }

        static public string FullPathFromSharedData(string subDir)
        {
            return PathCombine(shareDataPath, subDir);
        }

        static public string FullPathFromTool(string subDir)
        {
            return PathCombine(toolPath, subDir);
        }

        static public string FullPathFromServer(string subDir)
        {
            return PathCombine(serverPath, subDir);
        }
    }
}
