using DBUtility;
using System;


namespace ClusterManagerServerLib
{
    public partial class Api
    {
        void ProcessDBPostUpdate()
        {
            foreach (var dbPool in Db.DBLabelNameList)
            {
                foreach (var dbManager in dbPool.Value.DBMngLst)
                {
                    try
                    {
                        var queue = dbManager.GetPostUpdateQueue();
                        while (queue.Count!=0)
                        {
                            var query = queue.Dequeue();
                            query.PostUpdate();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
