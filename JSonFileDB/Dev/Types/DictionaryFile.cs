using Common.ApStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Types
{
    class DictionaryFile
    {
        ApStorage ap;

        public DictionaryFile(string filename)
        {
            ap = new ApStorage(false, filename, true);
        }

        public virtual void set(string name, string value)
        {
            int count = int.Parse(ap.getConf("count", "0"));
            ap.setConf("names." + name, "items." + count);
            ap.setConf("items." + count, value);
            count++;
            ap.setConf("count", count.ToString());
        }

        public virtual string getChild(string name)
        {
            string itemName = ap.getConf("names." + name, "");
            if (itemName != "")
            {
                return ap.getConf(itemName, "");
            }
            else
                return "";

        }

        public virtual string getChild(int index)
        {
            return ap.getConf("items." + index, "");

        }

        public virtual int getChildsCount()
        {
            return int.Parse(ap.getConf("count", "0"));

        }
    }
}
