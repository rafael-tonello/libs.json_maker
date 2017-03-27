using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs
{
    class JsonMaker
    {
		/*
			Autor: Rafal Tonello
			email: tonello.rafinha@gmail.com
		*/
        class ObjectItem
        {
            public string name;
            public string value;
            public bool isArray = false;
            public ObjectItem parent;
            public List<ObjectItem> childs = new List<ObjectItem>();

            public ObjectItem(ObjectItem pParent)
            {
                this.parent = pParent;
            }

            public void clear()
            {
                this.name = "";
                this.value = "";
                foreach (var att in this.childs)
                    att.clear();
                this.childs.Clear();

            }
            public string ToJson(bool exportSelfName = true)
            {

                string namePrefix = this.name != "" ? "\"" + this.name + "\":" : "";

                if (!exportSelfName)
                    namePrefix = "";
                //se for um objeto simples, retorna apenas o valor string
                if (this.childs.Count == 0)
                {
                    string correctValue = value;
                    //veririca se não é numero (inteiro ou float
                    if
                        ((!(containsOnly(correctValue, "0123456789.")) && //numero
                        !(containsOnly(correctValue, "0123456789-."))) ||
                        value == "")//float
                    {
                        //verifica se é um "true", "false", ou "null"
                        if ((correctValue.ToLower() == "true") || (correctValue.ToLower() == "false") || (correctValue.ToLower() == "null"))
                            correctValue = correctValue.ToLower();
                        else //adiciona aspas
                            correctValue = '\"' + correctValue + '\"';


                    }

                    return namePrefix + correctValue;
                }
                else
                {
                    StringBuilder ret = new StringBuilder();
                    ret.Append(namePrefix);
                    ret.Append(this.isArray ? "[" : "{");

                    //percore os filhos
                    for (int cont = 0; cont < this.childs.Count; cont++)
                    {
                        //ser for um array, garante que o filho não tenha nome
                        if (isArray)
                            this.childs[cont].name = "";
                        //adiciona o filho à exportação
                        ret.Append(childs[cont].ToJson());

                        //se não for o último elemento, adiciona uma virgula
                        if (cont < this.childs.Count - 1)
                            ret.Append(',');
                    }
                    ret.Append(this.isArray ? "]" : "}");
                    return ret.ToString();
                }
            }

            private bool containsOnly(string value, string chars)
            {
                int cont = 0;
                while (cont < value.Length)
                {
                    if (!chars.Contains(value[cont]))
                        return false;
                    cont++;
                }
                return true;
            }
        }

        private ObjectItem root = new ObjectItem(null) { name = "" };
        public void clear()
        {
            root.clear();
        }


        //return the nextChildName
        private string _getNewArrayPos(string objectName)
        {
            List<string> parts = objectName.Replace("[", ".[").Split('.').ToList();
            ObjectItem currentParent = this.root;

            string retname = "";
            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];
                ObjectItem parent = null;
                if (currentName.Contains('['))
                {

                    int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));

                    //caso não existam filhos até o indice especificado, cria estes filhos
                    for (int cont2 = currentParent.childs.Count; cont2 <= indice; cont2++)
                    {
                        currentParent.childs.Add(new ObjectItem(currentParent) { name = "", value = "null" });
                    }

                    parent = currentParent.childs[indice];

                    currentParent.isArray = true;
                }
                else
                {

                    parent = currentParent.childs.Find(delegate (ObjectItem att) { return att.name == currentName; });

                    //se o elemento não estiver na lista atual, adiciona um filho a ela
                    if (parent == null)
                    {
                        parent = new ObjectItem(currentParent)
                        {
                            name = currentName,
                            isArray = false
                        };
                        currentParent.childs.Add(parent);
                    }
                }

                //verifica se já está no final dos nomes
                if (cont == parts.Count - 1)
                {
                    parent.isArray = true;
                    return objectName + "[" + (parent.childs.Count).ToString() + "]";
                }

                currentParent = parent;
            }
            return "";
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
            //quebra o nome em um array
            objectName = objectName.Replace("[", ".[");
            List<string> parts = objectName.Split('.').ToList();
            ObjectItem currentParent = this.root;

            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];

                //verifica se o nome atual está na lista atual
                //verifica se se refere a um array
                ObjectItem parent = null;
                if (currentName.Contains('['))
                {

                    int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));

                    //caso não existam filhos até o indice especificado, cria estes filhos
                    for (int cont2 = currentParent.childs.Count; cont2 <= indice; cont2++)
                    {
                        currentParent.childs.Add(new ObjectItem(currentParent) { name = "", value = "null" });
                    }

                    parent = currentParent.childs[indice];

                    currentParent.isArray = true;
                }
                else
                {
                    parent = currentParent.childs.Find(delegate (ObjectItem att) { return att.name == currentName; });

                    //se o elemento não estiver na lista atual, adiciona um filho a ela
                    if (parent == null)
                    {
                        parent = new ObjectItem(currentParent)
                        {
                            name = currentName,
                            isArray = false

                        };

                        //
                        currentParent.childs.Add(parent);
                    }

                    //verifica se já está no final dos nomes
                }
                if (cont == parts.Count - 1)
                {
                    parent.value = value;
                }



                //define a lista de filhos di elemento encontrado como sendo a lista atual.
                currentParent = parent;
            }
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
            //quebra o nome em um array
            objectName = objectName.Replace("[", ".[");
            List<string> parts = objectName.Split('.').ToList();
            ObjectItem currentParent = this.root;

            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];

                //verifica se o nome atual está na lista atual
                //verifica se se refere a um array
                ObjectItem parent = null;
                if (currentName.Contains('['))
                {

                    int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));

                    if (indice < currentParent.childs.Count)
                        parent = currentParent.childs[indice];
                    else
                        break;

                }
                else
                {
                    parent = currentParent.childs.Find(delegate (ObjectItem att) { return att.name == currentName; });

                    //se o elemento não estiver na lista atual, adiciona um filho a ela
                    if (parent == null)
                        return false;

                    //verifica se já está no final dos nomes
                }
                if (cont == parts.Count - 1)
                {
                    return true;
                }
            }
            return false;
        }

        public string get(string objectName)
        {
            //quebra o nome em um array
            objectName = objectName.Replace("[", ".[");
            List<string> parts = objectName.Split('.').ToList();
            ObjectItem currentParent = this.root;

            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];

                //verifica se o nome atual está na lista atual
                //verifica se se refere a um array
                ObjectItem parent = null;
                if (currentName.Contains('['))
                {

                    int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));

                    if (indice < currentParent.childs.Count)
                        parent = currentParent.childs[indice];
                    else
                        break;

                }
                else
                {
                    parent = currentParent.childs.Find(delegate (ObjectItem att) { return att.name == currentName; });

                    //se o elemento não estiver na lista atual, adiciona um filho a ela
                    if (parent == null)
                        break;

                    //verifica se já está no final dos nomes
                }
                if (cont == parts.Count - 1)
                {

                    string ret = parent.ToJson(false);
                    //remove aspasta do inicio e do final
                    if (ret[0] == '\"')
                        ret = ret.Substring(1, ret.Length - 2);

                    return ret;
                }

                //define a lista de filhos di elemento encontrado como sendo a lista atual.
                currentParent = parent;
            }
            return "null";
        }

        private List<string> getObjectsNames(ObjectItem currentItem = null)
        {
            List<string> retorno = new List<string>();

            if (currentItem == null)
                currentItem = this.root;



            if (currentItem.childs.Count == 0)
            {
                retorno.Add(currentItem.name);
            }
            else
            {
                string parentName = currentItem.name;
                List<string> childsNames;
                for (int cont = 0; cont < currentItem.childs.Count; cont++)
                {
                    if (currentItem.isArray)
                        currentItem.childs[cont].name = "";

                    childsNames = getObjectsNames(currentItem.childs[cont]);

                    //adiciona os filhos ao resultado
                    //verifica se o nome atual atende ao filtro
                    foreach (var att in childsNames)
                    {
                        string childNameAtt = currentItem.name;
                        if (currentItem.isArray)
                            childNameAtt = currentItem.name + "[" + cont + "]";

                        if ((att != "") && (childNameAtt != "") && (att[0] != '['))
                            childNameAtt += ".";

                        childNameAtt += att;
                        retorno.Add(childNameAtt);
                    }
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
            bool isArray = false;
            if (value[0] == '[')
            {
                childs = getJsonFields(value);
                isArray = true;
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

                if (isArray)
                    tempName = _getNewArrayPos(name);

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
                if (att == '\"')
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
