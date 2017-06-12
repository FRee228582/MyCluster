using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtility
{
    public class SystemTime
    {
        /// <summary>
        /// 
        /// </summary>
        private DateTime _prev;
        public void Init()
        {
            _prev = DateTime.Now;
        }
        public TimeSpan Update(out bool isPassDay)
        {
            var now = DateTime.Now;
            if (now.Day!=_prev.Day)
            {
                isPassDay = true;
            }
            else
            {
                isPassDay = false;
            }
            var delta = now - _prev;
            _prev = now;
            return delta;
        }
        public TimeSpan Update()
        {
            var now = DateTime.Now;
            var delta = now - _prev;
            _prev = now;
            return delta;
        }
        /// <summary>
        /// 从调用Update到现在的时间 
        /// </summary>
        /// <returns></returns>
        public double GetThis()
        {
            var now = DateTime.Now;
            var delta = now - _prev;
            return delta.TotalMilliseconds;
        }
        static public bool CheckRefresh(int[] refresh,DateTime lastRefresh)
        {
            if (lastRefresh.Date<DateTime.Today)
            {
                return true;
            }
            else
            {
                foreach (var item in refresh)
                {
                    if (lastRefresh.Hour<item&&DateTime.Now.Hour>item)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
    }
}
