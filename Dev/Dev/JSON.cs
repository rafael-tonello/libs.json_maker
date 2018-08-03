using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonMaker
{
    public class JSON : IDisposable
    {

        public enum JsonType { Memory, File}

        private IJSONObject root;

        object JsonObjectArguments;


        public void clear()
        {
            root.clear();
        }

        private void internalInitialize(JsonType type, object arguments)
        {
            JsonObjectArguments = arguments;
            if (type == JsonType.Memory)
                root = new InMemoryJsonObject(null, "");
            else
                root = new FileSystemJsonObject(null, "", (string)JsonObjectArguments);
        }

        public JSON(JsonType type = JsonType.Memory, object arguments = null) { this.internalInitialize(type, arguments); }
        public JSON(string JsonString, JsonType type = JsonType.Memory, object arguments = null)
        {
            this.internalInitialize(type, arguments);
            this.parseJson(JsonString);
        }
        
        private IJSONObject find(string objectName, bool autoCreateTree, IJSONObject currentParent)
        {
            
            //quebra o nome em um array
            objectName = objectName.Replace("]", "").Replace("[", ".");
            string currentName = objectName;
            string childsNames = "";
            IJSONObject childOnParent;

            if (objectName.IndexOf('.') > -1)
            {
                currentName = objectName.Substring(0, objectName.IndexOf('.'));
                childsNames = objectName.Substring(objectName.IndexOf('.') + 1);
            }

            if (!(currentParent.__containsChild(currentName)))
            {
                if (autoCreateTree)
                {
                    IJSONObject tempObj;
                    if (currentParent is InMemoryJsonObject)
                        tempObj = new InMemoryJsonObject((InMemoryJsonObject)currentParent, currentParent.getRelativeName() + "." + currentName);
                    else
                        tempObj = new FileSystemJsonObject((FileSystemJsonObject)currentParent, currentParent.getRelativeName() + "." + currentName, (string)JsonObjectArguments);

                    currentParent.setChild(currentName, tempObj);
                }
                else
                {   
                    return null;
                }
            }


            childOnParent = currentParent.__getChild(currentName);


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

            IJSONObject temp = this.find(objectName, true, this.root);

            /*if (value[0] == '\"')
                value = value.Substring(1, value.Length - 2);*/

            //value = value.Replace("\"", "\\\"");

            temp.setSingleValue(value);

        }

        private void del(IJSONObject node)
        {
            node.clear();
            var childs = node.__getChildsNames();
            foreach (var c in childs)
            {
                del(node.__getChild(c));
            }
            childs.Clear();

            /*
            var parentNodesNames = node.parent.__getChildsNames();
            for (int cont = 0; cont < parentNodesNames.Count; cont++)
            {
                if (parentNodesNames[cont] == node)
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
            }*/
        }

        Semaphore interfaceSemaphore = new Semaphore(1, 1);

        /// <summary>
        /// Removes an object from JSON three
        /// </summary>
        /// <param name="objectName">The object name</param>
        public void del(string objectName)
        {
            interfaceSemaphore.WaitOne();
            IJSONObject temp = this.find(objectName, false, this.root);
            if (temp != null)
                del(temp);

            interfaceSemaphore.Release();


        }

        public void clearChilds(string objectName)
        {
            IJSONObject temp = this.find(objectName, false, this.root);
            if (temp != null)
            {
                //var childs = temp.__getChilds();
                var names = temp.__getChildsNames();
                foreach (var c in names)
                {
                    del(temp.__getChild(c));
                }
            }



        }

        /// <summary>
        /// Set or creates an property with an json string
        /// </summary>
        /// <param name="objectName">The json object name</param>
        /// <param name="value">The json string </param>
        public void set(string objectName, string value)
        {
            interfaceSemaphore.WaitOne();

            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + value);
            interfaceSemaphore.Release();

        }

        /// <summary>
        /// Insert a new json in current json three
        /// </summary>
        /// <param name="objectName">Name of the object</param>
        /// <param name="toImport">Json to be imported</param>
        public void set(string objectName, JSON toImport)
        {
            if (objectName != "")
                objectName = objectName + ":";
            this.parseJson(objectName + toImport.ToJson());

        }

        /// <summary>
        /// Serialize the Json three
        /// </summary>
        /// <param name="quotesOnNames">User '"' in name of objects</param>
        /// <returns></returns>
        public string ToJson(bool format = true)
        {
            interfaceSemaphore.WaitOne();
            string result = root.ToJson(true, format);
            interfaceSemaphore.Release();
            return result;
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        /// Return true if the an object is in json three
        /// </summary>
        /// <param name="objectName">The object name</param>
        /// <returns></returns>
        public bool contains(string objectName)
        {
            interfaceSemaphore.WaitOne();
            bool result = this.find(objectName, false, this.root) != null;
            interfaceSemaphore.Release();
            return result;
        }

        /// <summary>
        /// returns the value of an json object as a json string (Serialize an object)
        /// </summary>
        /// <param name="objectName">The object name</param>
        /// <param name="quotesOnNames">User '"' in names</param>
        /// <returns></returns>
        public string get(string objectName, bool format = false, bool quotesOnNames = true)
        {
            interfaceSemaphore.WaitOne();
            IJSONObject temp = this.find(objectName, false, this.root);
            interfaceSemaphore.Release();
            if (temp != null)
                return temp.ToJson(quotesOnNames, format);
            else
                return "null";

        }

        private List<string> getObjectsNames(IJSONObject currentItem = null)
        {
            List<string> retorno = new List<string>();

            if (currentItem == null)
                currentItem = this.root;


            string parentName = "";

            List<string> childsNames;

            var chieldsNames= currentItem.__getChildsNames();
            for (int cont = 0; cont < chieldsNames.Count; cont++)
            {

                childsNames = getObjectsNames(currentItem.__getChild(chieldsNames[cont]));


                parentName = chieldsNames[cont];
                //adiciona os filhos ao resultado
                //verifica se o nome atual atende ao filtro
                foreach (var att in childsNames)
                {


                    string nAtt = att;
                    if (nAtt != "")
                        nAtt = parentName + '.' + nAtt;

                    retorno.Add(nAtt);
                }
                retorno.Add(chieldsNames[cont]);
            }
            return retorno;

        }

        /// <summary>
        /// Return all names of the json three of an object
        /// </summary>
        /// <param name="objectName">The name of object</param>
        /// <returns></returns>
        public List<string> getObjectsNames(string objectName = "")
        {
            if (objectName == "")
            {
                IJSONObject nullo = null;
                return this.getObjectsNames(nullo);
            }
            else
            {
                var finded = this.find(objectName, false, this.root);
                List<string> retorno = new List<string>();
                if (finded != null)
                    retorno = this.getObjectsNames(finded);

                return retorno;
            }
        }

        private List<string> getChildsNames(IJSONObject currentItem = null)
        {
            List<string> retorno = new List<string>();

            if (currentItem == null)
                currentItem = this.root;

            var chieldsNames = currentItem.__getChildsNames();
            for (int cont = 0; cont < chieldsNames.Count; cont++)
            {
                retorno.Add(chieldsNames[cont]);
            }
            return retorno;
        }

        /// <summary>
        /// Return the childNames of an json object
        /// </summary>
        /// <param name="objectName">The name of object</param>
        /// <returns></returns>
        public List<string> getChildsNames(string objectName = "")
        {
            if (objectName == "")
            {
                IJSONObject nullo = null;
                return this.getChildsNames(nullo);
            }
            else
            {
                var finded = this.find(objectName, false, this.root);
                List<string> retorno = new List<string>();
                if (finded != null)
                    retorno = this.getChildsNames(finded);
                return retorno;
            }

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
                if (toInsert != json)
                    this._set(tempName, toInsert);
            }
        }

        private List<string> getJsonFields(string json)
        {
            int open = 0;
            List<string> fields = new List<string>();
            StringBuilder temp = new StringBuilder();
            bool quotes = false;

            bool skeepNext = false;

            for (int cont = 1; cont < json.Length - 1; cont++)
            {
                if (skeepNext)
                {
                    temp.Append(json[cont]);
                    skeepNext = false;
                    continue;
                }

                if (json[cont] == ',')
                {
                    if ((open == 0) && (!quotes))
                    {
                        fields.Add(temp.ToString());
                        temp.Clear();
                    }
                    else
                        //if ((quotes) || (temp.Length == 0) || (!"}]".Contains(temp[temp.Length - 1])))
                        temp.Append(json[cont]);
                }

                else
                {
                    if (!quotes)
                    {
                        if ((json[cont] == '{') || (json[cont] == '['))
                            open++;
                        else if ((json[cont] == '}') || (json[cont] == ']'))
                            open--;
                    }
                    else if (json[cont] == '\\')
                    {
                        skeepNext = true;
                    }

                    if (json[cont] == '"')
                    {
                        //if ((json[cont - 1] != '\\') || (json[cont - 2] == '\\'))
                        quotes = !quotes;
                    }

                    // if ((quotes) || (temp.Length == 0) || (!"}]".Contains(temp[temp.Length - 1])))
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
            char oldOldAtt = ' ';
            char oldAtt = ' ';
            bool skeepNext = false;
            foreach (char att in json)
            {
                if (skeepNext)
                {
                    result.Append(att);
                    skeepNext = false;
                    continue;
                }

                if (att == '\"')
                    quotes = !quotes;

                if (!quotes)
                {
                    if (!"\r\n\t\0 ".Contains(att))
                        result.Append(att);
                }
                else
                {
                    if (att == '\\')
                    {
                        skeepNext = true;
                    }

                    result.Append(att);
                }

                oldOldAtt = oldAtt;
                oldAtt = att;
            }

            try
            {
                //var result2 = __unescapeString(result.ToString());
                //return result2;
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

        public SOType getJSONType(string objectName)
        {
            interfaceSemaphore.WaitOne();
            IJSONObject temp = this.find(objectName, false, this.root);
            interfaceSemaphore.Release();
            if (temp != null)
            {
                return temp.getJSONType();
            }
            else
                return SOType.Null;
        }

        /// <summary>
        /// Get a json property as string
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
        public string getString(string name, string defaultValue = "")
        {
            string result = this.get(name);
            if ((result.Length > 0) && (result[0] == '"'))
                result = result.Substring(1);
            if ((result.Length > 0) && (result[result.Length - 1] == '"'))
                result = result.Substring(0, result.Length - 1);

            result = __unescapeString(result);

            if ((result != "") && (result != "null"))
                return result;
            else
                return defaultValue;

        }

        /// <summary>
        /// Set or create a property as string
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setString(string name, string value)
        {
            if (value == null)
                value = "";
            value = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
            this.set(name, '"' + value + '"');
        }

        /// <summary>
        /// Get a json property as int
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
        public int getInt(string name, int defaultValue = 0)
        {
            string temp = getOnly(this.get(name), "0123456789-");
            if (temp != "")
                return int.Parse(temp);
            else return defaultValue;
        }

        /// <summary>
        /// Set or create a property as int
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setInt(string name, int value)
        {
            this.set(name, value.ToString());
        }

        /// <summary>
        /// Get a json property as Int64
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
        public Int64 getInt64(string name, Int64 defaultValue = 0)
        {
            string temp = getOnly(this.get(name), "0123456789-");
            if (temp != "")
                return Int64.Parse(temp);
            else return defaultValue;
        }

        /// <summary>
        /// Set or create a property as int64
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setInt64(string name, Int64 value)
        {
            this.set(name, value.ToString());
        }

        /// <summary>
        /// Get a json property as boolean
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
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

        /// <summary>
        /// Set or create a property as boolean
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setBoolean(string name, bool value)
        {
            this.set(name, value.ToString().ToLower());
        }

        /// <summary>
        /// Get a json property as DateTime
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
        public DateTime getDateTime(string name, string format = "")
        {
            string temp = getOnly(this.get(name), "0123456789/-: TU");
            if (temp != "")
                if (format != "")
                    return DateTime.ParseExact(temp, format, System.Globalization.CultureInfo.InvariantCulture);
                else
                    return DateTime.Parse(temp);
            else
                return new DateTime(0);

        }

        /// <summary>
        /// Set or create a property as DateTime. To set a custom DateTime format, please use setString
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setDateTime(string name, DateTime value, string format = "")
        {
            string newV = "";
            if (format != "")
                newV = value.ToString(format);
            else
                newV = value.ToString();
            this.set(name, '"' + newV + '"');
        }

        public void setDateTime_UtcFormat(string name, DateTime value, TimeSpan offset)
        {
            if (offset.Equals(TimeSpan.MinValue))
            {
                offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            }
            string timeZone = offset.ToString();
            timeZone = timeZone.Remove(timeZone.LastIndexOf(':'));
            if (timeZone[0] != '-')
                timeZone = "+" + timeZone;

            this.setDateTime(name, value, "yyyy-MM-ddTHH:mm:ss" + timeZone);
        }

        public void setDateTime_UtcFormat(string name, DateTime value)
        {
            setDateTime_UtcFormat(name, value, TimeSpan.MinValue);
        }

        /// <summary>
        /// Get a json property as double. To get a custom DateTime, please use getString
        /// </summary>
        /// <param name="name">Object name of the property</param>
        /// <param name="defaultValue">Value to be returned when the property is not found</param>
        /// <returns></returns>
        public double getDouble(string name, double defaultValue = 0)
        {
            string temp = getOnly(this.get(name).Replace('.', ','), "0123456789-,");
            if (temp != null)
                return double.Parse(temp);
            else return defaultValue;
        }

        /// <summary>
        /// Set or create a property as Double
        /// </summary>
        /// <param name="name">The property object name </param>
        /// <param name="value">The value</param>
        public void setDouble(string name, double value)
        {
            if (double.IsNaN(value))
                value = 0f;
            else if (double.IsInfinity(value))
                value = 0f;
            this.set(name, value.ToString().Replace(',', '.'));
        }

        /// <summary>
        /// Return the childs count of an object (like arrays or objects)
        /// </summary>
        /// <param name="objectName">The name of the object</param>
        /// <returns></returns>
        public int getArrayLength(string objectName = "")
        {
            var finded = this.find(objectName, false, this.root);

            if (finded != null)
                return finded.__getChildsNames().Count();
            return 0;
        }

        private string getOnly(string text, string chars)
        {
            StringBuilder ret = new StringBuilder();
            foreach (var att in text)
                if (chars.Contains(att))
                    ret.Append(att); return ret.ToString();
        }

        private string __unescapeString(string data)
        {
            //result = result.Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
            string nValue = "";
            int cont;
            for (cont = 0; cont < data.Length - 1; cont++)
            {
                if (data[cont] == '\\')
                {
                    if (data[cont + 1] == '\"')
                        nValue += '\"';
                    else if (data[cont + 1] == '\r')
                        nValue += '\r';
                    else if (data[cont + 1] == '\n')
                        nValue += '\n';
                    else if (data[cont + 1] == '\t')
                        nValue += '\t';
                    else if (data[cont + 1] == '\\')
                        nValue += '\\';
                    else
                        nValue += '?';

                    cont++;
                }
                else
                    nValue += data[cont];

                //cont++;


            }
            if (cont < data.Length)
                nValue = nValue + data[cont];

            return nValue;
        }

        public void Dispose()
        {
            this.clear();
        }

    }
}

