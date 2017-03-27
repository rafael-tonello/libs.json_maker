using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMaker
{
    class JSONObject
    {
        protected Dictionary<string, JSONObject> childs = new Dictionary<string, JSONObject>();
        public JSONObject parent;

        public JSONObject(JSONObject pParent)
        {
            this.parent = pParent;
        }
        public virtual void set(string name, JSONObject child)
        {
            childs[name] = child;
        }

        public virtual void delete(string name)
        {
            childs.Remove(name);

        }

        public virtual JSONObject get(string name)
        {
            if (childs.ContainsKey(name))
                return childs[name];
            else
                return null;
        }

        public void clear()
        {
            foreach (var current in childs.Values)
                current.clear();
            childs.Clear();
        }

        public virtual string ToJson()
        {
            StringBuilder result = new StringBuilder();
            result.Append("{");
            for (int cont = 0; cont < this.childs.Count; cont++)
            {
                var current = this.childs.ElementAt(cont);
                result.Append(current.Key + ":" + current.Value.ToJson());

                if (cont < this.childs.Count - 1)
                {
                    result.Append(',');
                }
            }
            result.Append("}");
            return result.ToString();
        }

        public void replace(JSONObject oldChild, JSONObject newChild)
        {
            for (int cont = 0; cont < this.childs.Count; cont++)
            {
                var curr = this.childs.ElementAt(cont);
                if (curr.Value == oldChild)
                    childs[curr.Key] = newChild;
            }

        }

        public Dictionary<string, JSONObject> __getChilds()
        {
            return __getChilds();
        }
    }
}
