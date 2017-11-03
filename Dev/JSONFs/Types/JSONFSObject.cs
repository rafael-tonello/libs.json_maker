using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class JSONFSObject
{

    public enum SOType { Null, String, Int, Double, Boolean }
    public string baseName;


    private  string getKey(string key, string defValue = "")
    {
        if (Directory.Exists(baseName + "\\.DATA"))
        {
            if (File.Exists(baseName + "\\.DATA\\" + key.ToUpper()))
            {
                return File.ReadAllText(baseName + "\\.DATA\\" + key.ToUpper());
            }
        }

        return defValue;
    }

    private void setKey(string key, string value)
    {
        createDirectoryPath(baseName + "\\.DATA");
        
        File.WriteAllText(baseName + "\\.DATA\\" + key.ToUpper(), value);

    }
    public string getSingleName()
    {
        if (baseName.Contains('\\'))
            return baseName.Substring(baseName.LastIndexOf('\\') + 1);
        else
            return baseName;
    }

    public SOType type_GET()
    {
        string str = getKey("type", SOType.Null.ToString());

        return (SOType)(Enum.Parse(typeof(SOType), str));

    }

    public string singleValue_GET()
    {
        return getKey("singleValue", "");
    }
    public JSONFSObject(string baseName)
    {
        this.baseName = baseName;
        
    }

    private int childs_Count()
    {
        int ret = 0;
        ret = int.Parse(getKey("childs.count", "0"));

        return ret;
    }

    private JSONFSObject childs_Get(int index)
    {
        string name = getKey("childs.indexes." + index, "");
        if (name != "")
        {
            JSONFSObject ret = new JSONFSObject(this.baseName + "\\" + name);
            return ret;
        }
        else
            return null;
    }

    private JSONFSObject childs_Get(string name)
    {
        JSONFSObject ret = null;
        if (getKey("childs.names." + name, "") != "")
        {
            ret = new JSONFSObject(baseName + "\\" + name);
        }
            

        return ret;
    }

    public JSONFSObject parent_Get()
    {
        JSONFSObject ret = new JSONFSObject(baseName.Substring(0, baseName.LastIndexOf('\\')));
        return ret;
    }

    public void childs_remove(string name)
    {

    }

    public virtual void setChild(string name, JSONFSObject child)
    {
        int index = this.childs_Count();

        setKey("childs.count", (index + 1).ToString());
        setKey("childs.indexes." +index, name);
        setKey("childs.names." +name, index.ToString());
    }

    public virtual void delete(string name)
    {
        childs_remove(name);
    }

    public virtual JSONFSObject get(string name)
    {
        return this.childs_Get(name);
    }

    public void clear()
    {
        
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

        setKey("singleValue", thisValue);
        setKey("type", thisType.ToString());
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

    public Dictionary<string, JSONFSObject> __getChilds()
    {
        int cont = childs_Count();
        Dictionary<string, JSONFSObject> ret = new Dictionary<string, JSONFSObject>();
        for (int c = 0; c < cont; c++)
        {
            //string curr = ctrl.getAp().getConf(this.baseName + ".childs.indexes." + c, "");

            var t = this.childs_Get(c);
            if (t != null)
                ret.Add(t.getSingleName(), t);

        }
        return ret;
    }

    private void createDirectoryPath(string path)
    {
        string[] names = path.Replace('/', '\\').Split('\\');
        string currDirectory =  "";
        foreach (var curr in names)
        {
            if (currDirectory != "")
                currDirectory += "\\";
            currDirectory += curr;

            if (!Directory.Exists(currDirectory))
                Directory.CreateDirectory(currDirectory);
        }
    }
}
