using IOCPLib.IOCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleManagerServerLib
{
    public partial class TickTock
    {
        int count = 0;
        private System.Timers.Timer timer;
        private AsyncSocketServer server;

        public TickTock(AsyncSocketServer server)
        {
            this.server = server;

            timer = new System.Timers.Timer(10000);
            //设置timer可用
            timer.Enabled = true;

            //设置timer
            //timer.Interval = 10000;

            //设置是否重复计时，如果该属性设为False,则只执行timer_Elapsed方法一次。
            timer.AutoReset = true;

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        public void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            server.GetQPS();
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
