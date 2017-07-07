using BattleServerLib;
using ServerFrameWork;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BattleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                SingleServer(args);
            }
            if (args.Length == 4)
            {
                Servers(args);
            }
            //Api api = new Api();
        }

        static void SingleServer(string[] args)
        {
            AbstractBaseServer api = new Api();
            try
            {
                api.ServerName = "BattleServer";
                api.Init(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} init failed:{1}", api.ServerName, e.ToString());
                api.Exit();
                return;
            }

            Thread thread = new Thread(api.MainLoop);
            thread.Start();

            Console.WriteLine("{0} OnReady..", api.ApiTag.GetServerTagString());

            while (thread.IsAlive)
            {
                api.ProcessInput();
                Thread.Sleep(1000);
            }

            api.Exit();
            Console.WriteLine("{0} Exit..", api.ServerName);
        }


        static void Servers(string[] a)
        {
            int nCount = int.Parse(a[3]);
            List<Thread> threadList = new List<Thread>();
            if (nCount >0)
            {
                for (int i = 0; i < nCount; i++)
                {
                    ServerTag Tag = new ServerTag();
                    Tag.AreaId = ushort.Parse(a[0]);
                    Tag.ServerId = ushort.Parse(a[1]); ;
                    Tag.SubId =(ushort)(i+1);

                    Thread thread = new Thread(new ParameterizedThreadStart(ThreadMethod));
                    thread.Start(Tag);
                    threadList.Add(thread);
                }
            }
        }
        public static void ThreadMethod(Object obj)
        {
            ServerTag Tag = (ServerTag)obj;
            string[] args = { Tag.AreaId.ToString(),Tag.ServerId.ToString(),Tag.SubId.ToString()};
            SingleServer(args);
        }

    }
}
