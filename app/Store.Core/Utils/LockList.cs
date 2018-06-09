using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Store.Core.Utils
{
    [Serializable()]
    public class LockList
    {
        private Dictionary<int, long> _list = new Dictionary<int, long>();
        private static long MAX_TICKS = 100;

        public bool addId(int id) {
            lock (_list) {
                //Удаляем старые ID
                foreach (var key in _list.Keys) {
                    long val = 0;
                    _list.TryGetValue(key, out val);
                    if (val + MAX_TICKS > DateTime.Now.Ticks)
                    {
                        _list.Remove(key);
                    }
                }

                if (!_list.ContainsKey(id))
                {
                    _list.Add(id, DateTime.Now.Ticks);
                    return true;
                }
                else
                {
                    long val = 0;
                    _list.TryGetValue(id, out val);
                    if (val + MAX_TICKS > DateTime.Now.Ticks)
                    {
                        _list.Remove(id);
                        _list.Add(id, DateTime.Now.Ticks);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
        }

        public void removeId(int id)
        {
            lock (_list)
            {
                if (_list.ContainsKey(id))
                {
                    _list.Remove(id);
                }
            }
        }
    }
}
