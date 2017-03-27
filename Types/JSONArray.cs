using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMaker
{
    class JSONArray : JSONObject
    {
        protected int arraysPos = 0;

        public JSONArray(JSONObject pParent) : base(pParent)
        {
            

        }
        public void set(JSONObject child)
        {
            base.set(this.arraysPos.ToString(), child);
            arraysPos++;
        }
        
        public  void delete(int index)
        {
            base.delete(index.ToString());
        }
        public void set(int index, JSONObject child)
        {
            for (int cont = 0; cont <= index; cont++)
            {
                if (!this.childs.ContainsKey(cont.ToString()))
                {
                    base.set(cont.ToString(), new JSONSingleValueObject(this, null));

                }
            }


            base.set(index.ToString(), child);
        }

        public JSONObject get(int index)
        {
            if (index < this.childs.Count)
                return base.get(index.ToString());
            else
                return null;

        }

        new public void clear()
        {
            base.clear();
        }

        new public string ToJson()
        {
            StringBuilder result = new StringBuilder();
            result.Append("[");
            for (int cont = 0; cont < this.childs.Count; cont++)
            {
                var current = this.childs.ElementAt(cont).Value;
                result.Append(current.ToJson());

                if (cont < this.childs.Count - 1)
                {
                    result.Append(',');
                }
            }
            result.Append("]");
            return result.ToString();
        }
    }
}
