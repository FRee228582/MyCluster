using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOCPLib.IOCP
{
    public class AsyncUserTokenPool
    {
        private Stack<AsyncUserToken> m_pool;

        public AsyncUserTokenPool(int capacity)
        {
            m_pool = new Stack<AsyncUserToken>(capacity);
        }

        public void Push(AsyncUserToken item)
        {
            if (item == null)
            {
                throw new ArgumentException("Items added to a AsyncSocketUserToken cannot be null");
            }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        public AsyncUserToken Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        public int Count
        {
            get { return m_pool.Count; }
        }
    }

    public class AsyncUserTokenList : Object
    {
        private List<AsyncUserToken> m_list;

        public AsyncUserTokenList()
        {
            m_list = new List<AsyncUserToken>();
        }

        public void Add(AsyncUserToken userToken)
        {
            lock(m_list)
            {
                m_list.Add(userToken);
            }
        }

        public void Remove(AsyncUserToken userToken)
        {
            lock (m_list)
            {
                m_list.Remove(userToken);
            }
        }

        public void CopyList(ref AsyncUserToken[] array)
        {
            lock (m_list)
            {
                array = new AsyncUserToken[m_list.Count];
                m_list.CopyTo(array);
            }
        }

        public int GetLength()
        {
            lock (m_list)
            {
                return m_list.Count;
            }
        }

        public void Update()
        {
            lock (m_list)
            {
                foreach (var item in m_list)
                {
                    if (item.InvokeElement != null)
                    {
                        item.InvokeElement.Update();
                    }
                }
            }
        }
    }
}
