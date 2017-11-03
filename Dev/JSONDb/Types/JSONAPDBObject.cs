using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class JSONOAPDBbject
{

    public enum SOType { Null, String, Int, Double, Boolean }
    JsonMaker.JsonMakerAPDB ctrl;
    public string baseName;

    public string getSingleName()
    {
        if (baseName.Contains('.'))
            return baseName.Substring(baseName.LastIndexOf('.') + 1);
        else
            return baseName;
    }

    public SOType type_GET()
    {
        string str = ctrl.getAp().getConf(this.baseName + ".type", SOType.Null.ToString());

        return (SOType)(Enum.Parse(typeof(SOType), str));

    }

    public string singleValue_GET()
    {
        return ctrl.getAp().getConf(this.baseName + ".singleValue", "");
    }
    public JSONOAPDBbject(JsonMaker.JsonMakerAPDB ctrl, string baseName)
    {
        this.ctrl = ctrl;
        this.baseName = baseName;
        
    }

    private int childs_Count()
    {
        return int.Parse(ctrl.getAp().getConf(this.baseName + ".childs.count", "0"));
    }

    private JSONOAPDBbject childs_Get(int index)
    {
        string name = ctrl.getAp().getConf(this.baseName + ".childs.indexes." + index, "");
        if (name != "")
        {
            JSONOAPDBbject ret = new JSONOAPDBbject(ctrl, this.baseName + "."+ name);
            return ret;
        }
        else
            return null;
    }

    private JSONOAPDBbject childs_Get(string name)
    {
        JSONOAPDBbject ret = new JSONOAPDBbject(ctrl, baseName + "."+name);

        return ret;
    }

    public JSONOAPDBbject parent_Get()
    {
        JSONOAPDBbject ret = new JSONOAPDBbject(ctrl, baseName.Substring(0, baseName.LastIndexOf('.')));
        return ret;
    }

    public void childs_remove(string name)
    {

    }

    public virtual void setChild(string name, JSONOAPDBbject child)
    {
        int index = this.childs_Count();

        ctrl.getAp().setConf(this.baseName + ".childs.count", (index + 1).ToString());
        ctrl.getAp().setConf(this.baseName + ".childs.indexes."+index, name);
        ctrl.getAp().setConf(this.baseName + ".childs.names."+name, index.ToString());


        ctrl.getAp().setConf(this.baseName + "." + name + ".indexOnParent", index.ToString());
        ctrl.getAp().setConf(this.baseName + "." + name + ".singleValue", "");
        ctrl.getAp().setConf(this.baseName + "." + name + ".type", SOType.Null.ToString());
    }

    public virtual void delete(string name)
    {
        childs_remove(name);
    }

    public virtual JSONOAPDBbject get(string name)
    {
        return this.childs_Get(name);
    }

    public void clear()
    {
        ctrl.getAp().setConf(this.baseName + ".childs.count", "0");
    }

    public virtual string ToJson(bool quotesOnNames)
    {
        StringBuilder result = new StringBuilder();
        int childs_count = this.childs_Count();
        if (childs_count > 0)
        {
            bool array = this.isArray();
            if (array)
                result.Append("[");
            else
                result.Append("{");
            for (int cont = 0; cont < childs_count; cont++)
            {
                var current = this.childs_Get(cont);
                if (array)
                    result.Append(current.ToJson(quotesOnNames));
                else
                {
                    if (quotesOnNames)
                        result.Append('"' + current.getSingleName() + "\":" + current.ToJson(quotesOnNames));
                    else
                        result.Append(current.getSingleName() + ":" + current.ToJson(quotesOnNames));
                }

                if (cont < childs_count - 1)
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
        SOType thisType = this.type_GET();
        string thisValue = this.singleValue_GET();

        if (thisType == SOType.Null)
            return "null";
        else if (thisType == SOType.Boolean)
            return ((thisValue.ToLower() == "true") || (thisValue == "1")) ? "true" : "false";
        else if (thisType == SOType.String)
        {
            if ((thisValue.Length > 0) && (thisValue[0] != '"'))
                return '"' + thisValue + '"';
            else
                return thisValue;
        }
        else if (thisType == SOType.Double)
        {
            return thisValue.Replace(',', '.');
        }
        else
            return thisValue;
    }

    public void setSingleValue(string value)
    {
        int sucess = 0;
        double sucess2 = 0;

        SOType thisType = SOType.Null;
        string thisValue = "";

        //trye as null
        if ((value == null) || (value == "null") || (value == ""))
            thisType = SOType.Null;
        else
        {
            //try as boolean
            thisValue = value;

            if ((value == "true") || (value == "false"))
                thisType = SOType.Boolean;
            else
            {
                //try as int
                if (int.TryParse(value, out sucess))
                    thisType = SOType.Int;
                else
                {
                    //try as double
                    if (double.TryParse(value, out sucess2))
                        thisType = SOType.Double;
                    else
                    {
                        //is a string
                        thisType = SOType.String;
                    }
                }
            }
        }

        ctrl.getAp().setConf(this.baseName + ".singleValue", thisValue);
        ctrl.getAp().setConf(this.baseName + ".type", thisType.ToString());
    }

    public bool isArray()
    {
        int temp = 0;

        int cont = 0;
        int childs_count = this.childs_Count();
        while (cont < childs_count)
        {
            if (!int.TryParse(this.childs_Get(cont).getSingleName(), out temp))
                return false;
            cont++;
        }
        return true;
    }

    public Dictionary<string, JSONOAPDBbject> __getChilds()
    {
        int cont = childs_Count();
        Dictionary<string, JSONOAPDBbject> ret = new Dictionary<string, JSONOAPDBbject>();
        for (int c = 0; c < cont; c++)
        {
            //string curr = ctrl.getAp().getConf(this.baseName + ".childs.indexes." + c, "");

            var t = this.childs_Get(c);
            if (t != null)
                ret.Add(t.getSingleName(), t);

        }
        return ret;
    }
}
