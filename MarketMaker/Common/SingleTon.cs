using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Common
{
    public class SingleTon<T> where T : class, new()
    {
        static public T GetInstance()
        {
            if (null == m_inst)
            {
                lock (typeof(T))
                {
                    if (null == m_inst)
                    {
                        m_inst = new T();
                    }
                }
            }
            return m_inst;
        }
        protected SingleTon()
        {
        }
        static protected T m_inst = null;
    }
}
