using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (value[0] == '\"')
                value = value.Substring(1, value.Length - 2);

            value = value.Replace("\"", "\\\"");

            temp.setSingleValue(value);
            
        }

        public void add(string objectName, string value)
        {
            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + value);

        }

        public void add(string objectName, JsonMaker toImport)
        {
            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + toImport.ToJson());

        }

        public string ToJson(bool quotesOnNames = true)
        {
            return root.ToJson(quotesOnNames);
        }

        public string ToString(bool quotesOnNames = true)
        {
            return root.ToJson(quotesOnNames);
        }

        public bool contains(string objectName)
        {
            return this.find(objectName, false, this.root) != null;
        }

        public string get(string objectName, bool quotesOnNames = true)
        {
            JSONObject temp = this.find(objectName, false, this.root);
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
            }
            return retorno;

        }

        public List<string> getObjectsNames(string objectName = "")
        {

            List<string> retorno = this.getObjectsNames(this.root);

            //remove os items que não atendem ao filtro
            if (objectName != "")
            {
                for (int cont = retorno.Count - 1; cont >= 0; cont--)
                {
                    if (!(retorno[cont].IndexOf(objectName) == 0))
                        retorno.RemoveAt(cont);
                }
            }

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

        private void parseJson(string json, string parentName = "")
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
                    if ("\"_ABCDEFGHIJKLMNOPQRSTUVXYWZabcdefghijklmnop.qrstuvxywz0123456789[]".Contains(json[index]))
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
            if (value[0] == '[')
            {
                childs = getJsonFields(value);
                for (int cont = 0; cont < childs.Count; cont++)
                    childs[cont] = cont + ":" + childs[cont];
            }
            else if (value[0] == '{')
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

            //result = result.Replace("\r", "\\r").Replace("\n", "\\n");
            return result.ToString();
        }

        private bool isAJson(string json, bool objects = true, bool arrays = true)
        {
            bool quotes = false;
            

            foreach (char att in json)
            {
                if ((att == '\"') || (att == '\''))
                    quotes = !quotes;

                if (!quotes)
                {
                    if (att == ':')
                        return true;
                    if ((objects) && ("{}".Contains(att)))
                        return true;
                    else if ((arrays) && ("[]".Contains(att)))
                        return true;
                }
            }

            return false;
        }

        #endregion


        public string getString(string name)
        {
            return this.get(name);
        }

        public int getInt(string name)
        {
            return int.Parse(getOnly(this.get(name), "0123456789-"));
        }

        public Int64 getInt64(string name)
        {
            return Int64.Parse(getOnly(this.get(name), "0123456789-"));
        }

        public bool getBoolean(string name)
        {
            if (this.get(name).ToLower() == "true")
                return true;
            else
                return false;
        }

        public DateTime getDateTime(string name)
        {
            return DateTime.Parse(getOnly(this.get(name), "0123456789/: TU"));
        }

        public double getDouble(string name)
        {
            return double.Parse(getOnly(this.get(name).Replace('.', ','), "0123456789-,"));
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
