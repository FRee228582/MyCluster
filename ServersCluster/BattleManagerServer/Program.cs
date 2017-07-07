﻿using BattleManagerServerLib;
using ServerFrameWork;
using System;
using System.Threading;

namespace BattleManagerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractBaseServer api = new Api();
            try
            {
                api.ServerName = "BattleManagerServer";
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
    }
}
