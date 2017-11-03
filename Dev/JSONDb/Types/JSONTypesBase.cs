using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMaker
{
    class JSONTypesBase
    {
        public JSONTypesBase parent;

        public JSONTypesBase(JSONTypesBase pParent)
        {
            this.parent = pParent;
        }

        public virtual void clear() { }
        public virtual string ToJson() { return ""; }
    }
}
