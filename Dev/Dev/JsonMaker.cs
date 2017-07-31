using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonMaker
{
    class JsonMaker
    {

        private JSONObject root = new JSONObject(null);
        public void clear()
        {
            root.clear();
        }


        private JSONObject find(string objectName, bool autoCreateTree, JSONObject currentParent)
        {
            //quebra o nome em um array
            objectName = objectName.Replace("]", "").Replace("[", ".");
            string currentName = objectName;
            string childsNames = "";
            JSONObject childOnParent;

            if (objectName.IndexOf('.') > -1)
            {
                currentName = objectName.Substring(0, objectName.IndexOf('.'));
                childsNames = objectName.Substring(objectName.IndexOf('.') + 1);
            }

            if (!(currentParent.__getChilds().ContainsKey(currentName)))
            {
                if (autoCreateTree)
                    currentParent.__getChilds()[currentName] = new JSONObject(currentParent);
                else
                    return null;
            }

            childOnParent = currentParent.__getChilds()[currentName];


            if (childsNames == "")
            {
                return childOnParent;
            }
            else
            {
                return this.find(childsNames, autoCreateTree, childOnParent);
            }
        }

        private void _set(string objectName, string value)
        {

            if (isAJson(value))
            {
                this.parseJson(value, objectName);
                return;
            }

            JSONObject temp = this.find(objectName, true, this.root);

            /*if (value[0] == '\"')
                value = value.Substring(1, value.Length - 2);*/

            //value = value.Replace("\"", "\\\"");

            temp.setSingleValue(value);

        }

        private void del(JSONObject node)
        {
            var childs = node.__getChilds();
            while (childs.Count > 0)
            {
                del(childs.ElementAt(0).Value);
            }
            childs.Clear();

            var parentNodes = node.parent.__getChilds();
            for (int cont = 0; cont < parentNodes.Count; cont++)
            {
                if (parentNodes.ElementAt(cont).Value == node)
                {
                    //if parent is an array, pull the elements forward backwards
                    if (node.parent.isArray())
                    {
                        for (int cont2 = cont; cont2 < parentNodes.Count - 1; cont2++)
                            parentNodes[parentNodes.ElementAt(cont2).Key] = parentNodes[parentNodes.ElementAt(cont2 + 1).Key];

                        parentNodes.Remove(parentNodes.Last().Key);
                    }
                    else
                    {
                        parentNodes.Remove(parentNodes.ElementAt(cont).Key);
                    }
                    break;
                }
            }
        }

        Semaphore interfaceSemaphore = new Semaphore(1, 1);
        public void del(string objectName)
        {
            interfaceSemaphore.WaitOne();
            JSONObject temp = this.find(objectName, false, this.root);
            if (temp != null)
                del(temp);

            interfaceSemaphore.Release();


        }

        public void clearChilds(string objectName)
        {
            JSONObject temp = this.find(objectName, false, this.root);
            if (temp != null)
            {
                var childs = temp.__getChilds();
                while (childs.Count > 0)
                {
                    del(childs.ElementAt(0).Value);
                }
            }
            


        }

        public void set(string objectName, string value)
        {
            interfaceSemaphore.WaitOne();

            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + value);
            interfaceSemaphore.Release();

        }

        public void set(string objectName, JsonMaker toImport)
        {
            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + toImport.ToJson());

        }

        public string ToJson(bool quotesOnNames = true)
        {
            interfaceSemaphore.WaitOne();
            string result = root.ToJson(quotesOnNames);
            interfaceSemaphore.Release();
            return result;
        }

        public bool contains(string objectName)
        {
            interfaceSemaphore.WaitOne();
            bool result = this.find(objectName, false, this.root) != null;
            interfaceSemaphore.Release();
            return result;
        }

        public string get(string objectName, bool quotesOnNames = true)
        {
            interfaceSemaphore.WaitOne();
            JSONObject temp = this.find(objectName, false, this.root);
            interfaceSemaphore.Release();
            if (temp != null)
                return temp.ToJson(quotesOnNames);
            else
                return "null";

        }

        private List<string> getObjectsNames(JSONObject currentItem = null)
        {
            List<string> retorno = new List<string>();

            if (currentItem == null)
                currentItem = this.root;


            string parentName = "";

            List<string> childsNames;

            for (int cont = 0; cont < currentItem.__getChilds().Count; cont++)
            {


                childsNames = getObjectsNames(currentItem.__getChilds().ElementAt(cont).Value);

                //adiciona os filhos ao resultado
                //verifica se o nome atual atende ao filtro
                foreach (var att in childsNames)
                {
                    parentName += currentItem.__getChilds().ElementAt(cont).Key;

                    string nAtt = att;
                    if (nAtt != "")
                        nAtt = parentName + '.' + nAtt;

                    retorno.Add(nAtt);
                }
                retorno.Add(currentItem.__getChilds().ElementAt(cont).Key);
            }
            return retorno;

        }

        public List<string> getObjectsNames(string objectName = "")
        {
            List<string> retorno = this.getObjectsNames(this.find(objectName, false, this.root));

            return retorno;
        }
		
		private List<string> getChildsNames(JSONObject currentItem = null)
        {
            List<string> retorno = new List<string>();

            if (currentItem == null)
                currentItem = this.root;
            
            for (int cont = 0; cont < currentItem.__getChilds().Count; cont++)
            {
                retorno.Add(currentItem.__getChilds().ElementAt(cont).Key);
            }
            return retorno;
        }

        public List<string> getChildsNames(string objectName = "")
        {
            List<string> retorno = this.getChildsNames(this.find(objectName, false, this.root));

            return retorno;
        }

        #region json parser

        public void fromJson(string json)
        {
            this.parseJson(json);

        }

        public void fromString(string json)
        {
            this.parseJson(json);

        }

        public void parseJson(string json, string parentName = "")
        {
            //limpa o json, removendo coisas desnecessárias como espaços em branco e tabs
            json = clearJsonString(json);
            string name = "";

            string value = json;

            //verifica se o json é uma par chave<-> valor. Se for pega o nome
            if (json.Contains(':'))
            {
                name = "";
                int index = 0;
                while (json[index] != ':')
                {
                    if ("\"_ABCDEFGHIJKLMNOPQRSTUVXYWZabcdefghijklmnop.qrstuvxywz0123456789[] ".Contains(json[index]))
                        name += json[index];
                    else
                    {
                        name = "";
                        break;
                    }
                    index++;
                }

                //se achou o nome, então tira o nome do json, deixando as duas informações em duas variáveis serparadas
                if (name != "")
                    value = json.Substring(json.IndexOf(':') + 1);

            }

            //remove aspas do nome, caso houverem
            name = name.Replace("\"", "");


            //se tiver um '{' ou um '[', então processa independentemente cacda um de seus valroes
            List<string> childs = new List<string>();
            if ((value != "") && (value[0] == '['))
            {
                childs = getJsonFields(value);
                for (int cont = 0; cont < childs.Count; cont++)
                    childs[cont] = cont + ":" + childs[cont];
            }
            else if ((value != "") && (value[0] == '{'))
                childs = getJsonFields(value);
            else
                childs.Add(value);



            //parapara o nome do objeto
            if ((parentName != "") && (name != ""))
                name = '.' + name;


            name = parentName + name;

            //se for um array, cria um novo array


            var tempName = name;
            foreach (var att in childs)
            {
                //se for uma string, remove as aspas do inicio e do final
                //
                var toInsert = att;

                //adiciona o objeto à lista
                this._set(tempName, toInsert);
            }
        }

        private List<string> getJsonFields(string json)
        {
            int open = 0;
            List<string> fields = new List<string>();
            StringBuilder temp = new StringBuilder();
            bool quotes = false;

            for (int cont = 1; cont < json.Length - 1; cont++)
            {
                if (json[cont] == ',')
                {
                    if ((open == 0) && (!quotes))
                    {
                        fields.Add(temp.ToString());
                        temp.Clear();
                    }
                    else
                        temp.Append(json[cont]);
                }

                else
                {
                    if ((json[cont] == '{') || (json[cont] == '['))
                        open++;
                    else if ((json[cont] == '}') || (json[cont] == ']'))
                        open--;
                    else if (json[cont] == '"')
                    {
                        if (json[cont - 1] != '\\')
                            quotes = !quotes;
                    }

                    temp.Append(json[cont]);
                }


            }
            if (temp.Length > 0)
                fields.Add(temp.ToString());

            return fields;

        }

        private string clearJsonString(string json)
        {
            StringBuilder result = new StringBuilder();

            bool quotes = false;

            foreach (char att in json)
            {
                if (att == '\"')
                    quotes = !quotes;

                if (!quotes)
                {
                    if (!"\r\n\t ".Contains(att))
                        result.Append(att);
                }
                else
                {
                    result.Append(att);
                }
            }
            
            try
            {
                return result.ToString();
            }
            catch
            {
                return json;
            }
        }

        private bool isAJson(string json, bool objects = true, bool arrays = true)
        {
            bool quotes = false;

            char oldAtt = (char)0;
            int cont = 0;
            json = json.TrimStart();

            foreach (char att in json)
            {
                cont++;
                if ((att == '\"') && (oldAtt != '\\'))
                    quotes = !quotes;

                if (!quotes)
                {
                    if (att == ':')
                        return true;
                    if ((objects) && ("{}".Contains(att)) && (cont == 0))
                        return true;
                    else if ((arrays) && ("[]".Contains(att) && (cont == 0)))
                        return true;
                }
                oldAtt = att;
            }

            return false;
        }

        #endregion


        public string getString(string name, string defaultValue = "")
        {
            string result = this.get(name);
            if ((result.Length > 0) && (result[0] == '"'))
                result = result.Substring(1);
            if ((result.Length > 0) && (result[result.Length - 1] == '"'))
                result = result.Substring(0, result.Length - 1);

            result = result.Replace("\\\\", "\\").Replace("\\\"", "\"");

            if (result != "")
                return result;
            else
                return defaultValue;

        }

        public void setString(string name, string value)
        {
            if (value == null)
                value = "";
            value = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
            this.set(name, '"' + value + '"');
        }

        public int getInt(string name, int defaultValue = 0)
        {
            string temp = getOnly(this.get(name), "0123456789-");
            if (temp != "")
                return int.Parse(temp);
            else return defaultValue;
        }

        public void setInt(string name, int value)
        {
            this.set(name, value.ToString());
        }

        public Int64 getInt64(string name, Int64 defaultValue = 0)
        {
            string temp = getOnly(this.get(name), "0123456789-");
            if (temp != "")
                return Int64.Parse(temp);
            else return defaultValue;
        }

        public void setInt64(string name, Int64 value)
        {
            this.set(name, value.ToString());
        }

        public bool getBoolean(string name, bool defaultValue = false)
        {
            string temp = this.get(name);
            if (temp != "")
            {
                if (temp.ToLower() == "true")
                    return true;
                else
                    return false;
            }
            else return defaultValue;
        }

        public void setBoolean(string name, bool value)
        {
            this.set(name, value.ToString().ToLower());
        }

        public DateTime getDateTime(string name)
        {
            string temp = getOnly(this.get(name), "0123456789/: TU");
            if (temp != "")
                return DateTime.Parse(temp);
            else
                return new DateTime(0);

        }

        public void setDateTime(string name, DateTime value)
        {
            this.set(name, '"' + value.ToString() + '"');
        }

        public double getDouble(string name, double defaultValue = 0)
        {
            string temp = getOnly(this.get(name).Replace('.', ','), "0123456789-,");
            if (temp != null)
                return double.Parse(temp);
            else return defaultValue;
        }

        public void setDouble(string name, double value)
        {
            this.set(name, value.ToString().Replace(',', '.'));
        }

        private string getOnly(string text, string chars)
        {
            StringBuilder ret = new StringBuilder();
            foreach (var att in text)
                if (chars.Contains(att))
                    ret.Append(att);
            return ret.ToString();
        }
    }
}