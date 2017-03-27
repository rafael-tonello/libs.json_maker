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


        private JSONObject find(string objectName, bool autoCreateTree)
        {
            //quebra o nome em um array
            objectName = objectName.Replace("[", ".[");
            List<string> parts = objectName.Split('.').ToList();
            JSONObject currentParent = this.root;

            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];

                //verifica se o nome atual está na lista atual
                //verifica se se refere a um array
                JSONObject parent = null;
                if (currentName.Contains('['))
                {

                    int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));

                    //força o pai a ser um array
                    if (!(currentParent is JSONArray))
                    {
                        if (autoCreateTree)
                            currentParent.parent.replace(currentParent, new JSONArray(currentParent.parent));
                        else
                            return null;
                    }

                    parent = ((JSONArray)(currentParent)).get(indice);

                    if (parent == null)
                    {
                        if (autoCreateTree)
                        {
                            parent = new JSONSingleValueObject(currentParent);
                            ((JSONArray)(currentParent)).set(parent);
                        }
                        else
                            return null;
                    }

                }
                else
                {

                    //força o pai a ser um JSONObject
                    if ((currentParent is JSONArray) || (currentParent is JSONSingleValueObject))
                    {
                        if (autoCreateTree)
                        {
                            JSONObject toReplace = new JSONObject(currentParent.parent);
                            currentParent.parent.replace(currentParent, toReplace);
                            currentParent = toReplace;
                        }

                        else
                            return null;
                    }

                    parent = ((JSONObject)(currentParent)).get(currentName);
                    //se o elemento não estiver na lista atual, adiciona um filho a ela
                    if (parent == null)
                    {
                        if (autoCreateTree)
                        {
                            parent = new JSONSingleValueObject(currentParent);
                            ((JSONObject)(currentParent)).set(currentName, parent);
                        }
                        else
                            return null;
                    }

                }
                //define a lista de filhos di elemento encontrado como sendo a lista atual.
                //verifica se já está no final dos nomes
                if (cont == parts.Count - 1)
                {
                    return parent;
                }
                currentParent = parent;
            }
            return null;
        }

        private void _set(string objectName, string value, bool replaceSpecialChars = true)
        {


            //if (value.Contains('{') || value.Contains('[') || value.Contains(":{") || value.Contains(":[") || value.Contains(":\""))

            if (isAJson(value))
            {
                this.parseJson(value, objectName);
                return;
            }

            if (replaceSpecialChars)
            {
                if (!isAJson(value))
                {
                    if ((value.Length > 0) && (value[0] == '\"'))
                        value = value.Substring(1, value.Length - 2);
                }
                //value = value.Replace("\"", "\\\"");
            }
            JSONObject temp = this.find(objectName, true);
            if (!(temp is JSONSingleValueObject))
            {
                JSONSingleValueObject temp2 = new JSONSingleValueObject(temp.parent);
                temp.parent.replace(temp, temp2);
                temp = temp2;
            }
            ((JSONSingleValueObject)(temp)).set(value);
            
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

        public string ToJson()
        {
            return root.ToJson();
        }

        public bool contains(string objectName)
        {
            return this.find(objectName, false) != null;
        }

        public string get(string objectName)
        {
            JSONObject temp = this.find(objectName, false);
            if (temp != null)
                return temp.ToJson();
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
                    if (currentItem is JSONArray)
                        parentName += "[" + cont + "]";
                    else
                        parentName += currentItem.__getChilds().ElementAt(cont).Key;

                    string nAtt = att;
                    if (nAtt != "")
                        nAtt = parentName + '.' + nAtt;
                    
                    retorno.Add(nAtt);
                }
            }
            //if (value.Contains('{') || value.Contains('[') || value.Contains(":{") || value.Contains(":[") || value.Contains(":\""))
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
                //if (!isAJson(toInsert))
                if ((att[0] != '{') && (att[0] != '['))
                {
                    if ((toInsert.Length > 0) && (toInsert[0] == '\"'))
                        toInsert = toInsert.Substring(1);
                    if ((toInsert.Length > 0) && (toInsert[toInsert.Length - 1] == '\"'))
                        toInsert = toInsert.Substring(0, toInsert.Length - 1);
                }

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

            if ((json[0] != '{') && (json[0] != '[') && (json[0] != '\"'))
                quotes = true;
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

            if ((json.Length > 0) && (json[0] != '{') && (json[0] != '[') && (json[0] != '\"'))
                quotes = true;

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
