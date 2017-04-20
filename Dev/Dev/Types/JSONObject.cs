using System.Collections.Generic;
using System.Linq;
using System.Text;

class JSONObject
{
    protected Dictionary<string, JSONObject> childs = new Dictionary<string, JSONObject>();
    public JSONObject parent;


    private enum SOType { Null, String, Int, Double, Boolean }
    private SOType type = SOType.Null;
    private string singleValue;

    public JSONObject(JSONObject pParent)
    {
        this.parent = pParent;
    }
    public virtual void setChild(string name, JSONObject child)
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

    public virtual string ToJson(bool quotesOnNames)
    {
        StringBuilder result = new StringBuilder();
        if (this.childs.Count > 0)
        {
            bool array = this.isArray();
            if (array)
                result.Append("[");
            else
                result.Append("{");
            for (int cont = 0; cont < this.childs.Count; cont++)
            {
                var current = this.childs.ElementAt(cont);
                if (array)
                    result.Append(current.Value.ToJson(quotesOnNames));
                else
                {
                    if (quotesOnNames)
                        result.Append('"' + current.Key + "\":" + current.Value.ToJson(quotesOnNames));
                    else
                        result.Append(current.Key + ":" + current.Value.ToJson(quotesOnNames));
                }

                if (cont < this.childs.Count - 1)
                {
                    result.Append(',');
                }
            }
            if (array)
                result.Append("]");
            else
                result.Append("}");
            return result.ToString();
        }
        else
            return serializeSingleValue();
    }

    private string serializeSingleValue()
    {
        if (this.type == SOType.Null)
            return "null";
        else if (this.type == SOType.Boolean)
            return ((this.singleValue.ToLower() == "true") || (this.singleValue == "1")) ? "true" : "false";
        else if (this.type == SOType.String)
        {
            if ((this.singleValue.Length > 0) && (this.singleValue[0] != '"'))
                return '"' + this.singleValue + '"';
            else
                return this.singleValue;
        }
        else
            return this.singleValue;


    }

    public void setSingleValue(string value)
    {
        int sucess = 0;
        double sucess2 = 0;

        //trye as null
        if ((value == null) || (value == "null"))
            this.type = SOType.Null;
        else
        {
            //try as boolean
            this.singleValue = value;

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

    private bool isArray()
    {
        int temp = 0;

        int cont = 0;
        while (cont < this.childs.Count)
        {
            if (!int.TryParse(this.childs.ElementAt(cont).Key, out temp))
                return false;
            cont++;
        }
        return true;
    }

    public Dictionary<string, JSONObject> __getChilds()
    {
        return childs;
    }
}
