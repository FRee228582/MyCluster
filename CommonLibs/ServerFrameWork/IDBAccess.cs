using DBUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrameWork
{
    interface IDBAccess
    {
        void InitDB();
        void UpdateProcessDB();
    }
}
