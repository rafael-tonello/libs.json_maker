using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMaker
{
    class JSONSingleValueObject : JSONObject
    {
        private enum SOType { Null, String, Int, Double, Boolean }
        string value;
        SOType type = SOType.Null;


        public JSONSingleValueObject(JSONObject pParent, string startValue = null) : base(pParent)
        {
            this.set(startValue);
        }

        public void set(string value)
        {
            int sucess = 0;
            double sucess2 = 0;

            //trye as null
            if ((value == null) || (value == "null"))
                this.type = SOType.Null;
            else
            {
                //try as boolean
                this.value = value;
                if ((value == "true") || (value == "false"))
                    this.type = SOType.Boolean;
                else
                {
                    //try as int
                    if (int.TryParse(value, out sucess))
                        type = SOType.Int;
                    else
                    {
                        //try as double
                        if (double.TryParse(value, out sucess2))
                            type = SOType.Double;
                        else
                        {
                            //is a string
                            type = SOType.String;
                        }
                    }
                }
            }
        }

        new public void clear()
        {
            this.value = "";
        }

        public override string ToJson()
        {
            if (this.type == SOType.Null)
                return "null";
            else if (this.type == SOType.Boolean)
                return ((this.value.ToLower() == "true") || (this.value == "1")) ? "true" : "false";
            else if (this.type == SOType.String)
                return '"' + this.value + '"';
            else
                return this.value;
        }
    }
}
