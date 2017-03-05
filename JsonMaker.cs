using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs
{
    class JsonMaker
    {
        class ObjectItem
        {
            public string name;
            public string value;
            public bool isArray = false;
            ObjectItem parent;
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
            public string ToJson()
            {

                string namePrefix = this.name != "" ? "\"" + this.name + "\":" : "";
                //se for um objeto simples, retorna apenas o valor string
                if (this.childs.Count == 0)
                {
                    string correctValue = value;
                    //veririca se não é numero (inteiro ou float
                    if (!(containsOnly(correctValue, "0123456789.")) && //numero
                        !(containsOnly(correctValue, "0123456789-.")))//float
                    {
                        //verifica se é um "true"
                        if ((correctValue.ToLower() == "true") || (correctValue.ToLower() == "false"))
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

        private ObjectItem root = new ObjectItem(null) { name = ""};
        public void clear()
        {
            root.clear();
        }

        private void _add(string objectName, string value, bool replaceSpecialChars = true)
        {


            //if (value.Contains('{') || value.Contains('[') || value.Contains(":{") || value.Contains(":[") || value.Contains(":\""))
            if (isAJson(value))
            {
                this.parseJson(value, objectName);
                return;
            }

            if (replaceSpecialChars)
            {
                value = value.Replace("\"", "\\\"");
            }
            //quebra o nome em um array
            objectName = objectName.Replace("[", ".[");
            List<string> parts = objectName.Split('.').ToList();
            ObjectItem currentParent = this.root;

            //percore os nomes
            for (int cont = 0; cont < parts.Count; cont++)
            {
                string currentName = parts[cont];

                
                bool isArray = false;
                //verifica se o nome atual está na lista atual
                //verifica se se refere a um array
                ObjectItem parent = null;
                if (currentName.Contains('['))
                {
                    isArray = true;

                    if (currentName == "[]")
                    {
                        //cria um novo elemento na lista pai
                        parent = new ObjectItem(currentParent)
                        {
                            name = currentName,
                            isArray = false

                        };

                        //
                        currentParent.childs.Add(parent);

                    }
                    else
                    {
                        int indice = int.Parse(currentName.Substring(1, currentName.Length - 2));
                        parent = currentParent.childs[indice];
                    }

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
                }

                //verifica se já está no final dos nomes
                if (cont == parts.Count - 1)
                {
                    
                    //altera o valor do parent
                    parent.value = value;

                }



                //define a lista de filhos di elemento encontrado como sendo a lista atual.
                currentParent = parent;
            }
        }

        public void add(string objectName, string value, bool replaceSpecialChars = true)
        {
            _add(objectName, value, replaceSpecialChars);

        }

        public void add(string objectName, JsonMaker toImport)
        {
            this._add(objectName, toImport.ToJson(), false);

        }

        public string ToJson()
        {

            return root.ToJson();
        }

        public bool contains(string name)
        {
            return false;

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
                    if ("\"_ABCDEFGHIJKLMNOPQRSTUVXYWZabcdefghijklmnopqrstuvxywz0123456789".Contains(json[index]))
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
                childs.Add(value.Replace("\"", ""));



            //parapara o nome do objeto
            if ((parentName != "") && (name != ""))
                name = '.' + name;

            if (isArray)
                name += "[]";

            name = parentName + name;
            foreach (var att in childs)
            {
                //se for uma string, remove as aspas do inicio e do final
                //
                var toInsert = att;
                //if (!isAJson(toInsert))
                if ((att[0] != '{') && (att[0] != '['))
                {
                    
                    toInsert = toInsert.Replace("\\\"", "\"");
                }

                //adiciona o objeto à lista
                this._add(name, toInsert);
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

            if ((json[0] != '{') && (json[0] != '[') && (json[0] != '\"'))
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

    }
}
