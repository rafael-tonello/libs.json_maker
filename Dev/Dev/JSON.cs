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

        private JSONObject root = new JSONObject(null);
        public void clear()
        {
            root.clear();
        }

        public JSON() { }
        public JSON(string JsonString)
        {
            this.parseJson(JsonString);
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

        /// <summary>
        /// Removes an object from JSON three
        /// </summary>
        /// <param name="objectName">The object name</param>
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
            JSONObject temp = this.find(objectName, false, this.root);
            interfaceSemaphore.Release();
            if (temp != null)
                return temp.ToJson(quotesOnNames, format);
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


                parentName = currentItem.__getChilds().ElementAt(cont).Key;
                //adiciona os filhos ao resultado
                //verifica se o nome atual atende ao filtro
                foreach (var att in childsNames)
                {


                    string nAtt = att;
                    if (nAtt != "")
                        nAtt = parentName + '.' + nAtt;

                    retorno.Add(nAtt);
                }
                retorno.Add(currentItem.__getChilds().ElementAt(cont).Key);
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
                JSONObject nullo = null;
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

        /// <summary>
        /// Return the childNames of an json object
        /// </summary>
        /// <param name="objectName">The name of object</param>
        /// <returns></returns>
        public List<string> getChildsNames(string objectName = "")
        {
            if (objectName == "")
            {
                JSONObject nullo = null;
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

                    if (json[cont] == '"')
                    {
                        if ((json[cont - 1] != '\\') || (json[cont - 2] == '\\'))
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
            foreach (char att in json)
            {
                if ((att == '\"') && ((oldAtt != '\\') || (oldOldAtt == '\\')))
                    quotes = !quotes;

                if (!quotes)
                {
                    if (!"\r\n\t\0 ".Contains(att))
                        result.Append(att);
                }
                else
                {
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

        public JSONObject.SOType getJSONType(string objectName)
        {
            interfaceSemaphore.WaitOne();
            JSONObject temp = this.find(objectName, false, this.root);
            interfaceSemaphore.Release();
            if (temp != null)
            {
                return temp.getJSONType();
            }
            else
                return JSONObject.SOType.Null;
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
                return finded.__getChilds().Count();
            return 0;
        }

        private string getOnly(string text, string chars)
        {
            StringBuilder ret = new StringBuilder();
            foreach (var att in text)
                if (chars.Contains(att))
                    ret.Append(att);return ret.ToString();
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


        #region exclusive to CSharp (Uses reflection). Serializer and Unserializer for public properties
        public static JSON SerializeObject(Object obj, int maxLevel = int.MaxValue)
        {
            return _SerializeObject(obj, maxLevel, 1);

        }
        private static JSON _SerializeObject(Object obj, int maxLevel = int.MaxValue, int currLevel = 1)
        {
            if (currLevel > maxLevel)
                return null;
            JSON ret = new JSON();
            ret.setString("Type", obj.GetType().ToString());

            var teste = obj.GetType().GetMembers();

            foreach (var prop in teste)
            {
                if (prop.MemberType == MemberTypes.Method)
                {
                    if (prop.Name.StartsWith("get_"))
                    {
                        string propName = prop.Name.Substring(prop.Name.IndexOf('_') + 1);

                        //(MethodInfo)prop).
                        object propValue;
                        try
                        {
                            if (((MethodInfo)prop).GetParameters().Length == 0)
                            {
                                propValue = ((MethodInfo)prop).Invoke(obj, new object[] { });
                                if (propValue != null)
                                    _addToJson(ret, propName, propValue, maxLevel, currLevel);
                            }
                        }
                        catch (Exception e) { string a = e.Message; }

                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// This
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rebuildChildren"></param>
        public static object UnSerializeObject(JSON data, object destObject = null, bool rebuildChildren = true)
        {

            //reconstituite the original object
            object result = null;
            if (destObject != null)
                result = destObject;
            else if (data.contains("Type"))
                result = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(data.getString("Type"));

            //if result can't be instanciate, checks if JOSN specifies a CSharp primitive Type
            if (result == null)
            {
                string typestr = data.getString("Type");
                //try restore primitive object (from dictionaries and lists)
                if (typestr == "System.Boolean")
                    return bool.Parse(data.getString("Value"));
                else if (typestr == "System.Double")
                    return double.Parse(data.getString("Value"));
                else if (typestr == "System.Int")
                    return int.Parse(data.getString("Value"));
                else if (typestr.EndsWith("DateTime"))
                    return DateTime.Parse(data.getString("Value"));
                else if (typestr == "System.String")
                    return data.getString("Value");
                else
                    //return null;

                    //if the assembly don't contais the current type, use an expando object
                    result = new ExpandoObject();
            }

            Dictionary<string, MemberInfo> props = new Dictionary<string, MemberInfo>();

            //in expando object cases, all propertyes in JSON may be imported
            if (result is ExpandoObject)
            {
                List<string> propsInJson = data.getChildsNames("");
                foreach (var c in propsInJson)
                    props.Add(c, null);
            }
            //in internal types, just properties of this types are allowed
            else
            {
                var selfMethods = result.GetType().GetMembers();
                //scrols the methods, looking for "set_"
                foreach (var c in selfMethods)
                {
                    if (c.Name.StartsWith("set_"))
                    {
                        string propName = c.Name.Substring(c.Name.IndexOf('_') + 1);
                        props.Add(propName, c);

                    }
                }
            }


            //scrolls through properties to be imported
            foreach (var curr in props)
            {
                //takes the name of perty and, in cases of internal types, the method to be used to set a property
                string propName = curr.Key;
                MethodInfo c = curr.Value != null ? (MethodInfo)curr.Value : null;

                //looks in DATA to see if this contains a property named 'propName'
                if (data.contains(propName))
                {
                    var JSType = data.getJSONType(propName);
                    //checks if is a type
                    if ((rebuildChildren) && (data.contains(propName + ".Type")))
                    {
                        string typeStr = data.getString(propName + ".Type");

                        //check if the type is a List
                        if (typeStr.Contains("System.Collections.Generic.List"))
                        {
                            //take the string type from JSON
                            string listTypeString = typeStr.Substring(typeStr.IndexOf('[') + 1);
                            listTypeString = listTypeString.Substring(0, listTypeString.IndexOf(']'));
                            Type listItemsType = Type.GetType(listTypeString);


                            //crate a new list, with the type scpecified in the JSON
                            Type listType = typeof(List<>).MakeGenericType(listItemsType);
                            object list = Activator.CreateInstance(listType);

                            //add JSON items to the list
                            int itemsCount = data.getArrayLength(propName + ".Items");
                            for (int cItems = 0; cItems < itemsCount; cItems++)
                            {
                                string kName = propName + ".Items[" + cItems + "]";
                                object currItem = UnSerializeObject(new JSON(data.get(kName)));

                                list.GetType().GetMethod("Add").Invoke(list, new object[] { currItem });
                            }
                            if (c != null)
                                ((MethodInfo)c).Invoke(result, new object[] { list });
                            else if (result is ExpandoObject)
                                ((IDictionary<string, object>)result)[propName] = list;
                        }
                        //checks if the type is a dictionary
                        else if (typeStr.Contains("System.Collections.Generic.Dictionary"))
                        {
                            //takes the types names (strings) of key and value from JSON
                            string dicTypesString = typeStr.Substring(typeStr.IndexOf('[') + 1);
                            dicTypesString = dicTypesString.Substring(0, dicTypesString.IndexOf(']'));
                            string[] dicTypesStringsArray = dicTypesString.Split(',');
                            Type dicKeyType = Type.GetType(dicTypesStringsArray[0]);
                            Type dicValueType = Type.GetType(dicTypesStringsArray[1]);

                            //create a new Dictionary, with the types specifieds in the JSON
                            //crate a new list, with the type scpecified in the JSON
                            Type dicType = typeof(Dictionary<,>).MakeGenericType(new Type[] { dicKeyType, dicValueType });
                            object dictionary = Activator.CreateInstance(dicType);

                            //add items to dictionary
                            int itemsLength = data.getArrayLength(propName + ".Items");
                            for (int countItems = 0; countItems < itemsLength; countItems++)
                            {
                                //takes the key from json
                                string temp = data.get(propName + ".Items[" + countItems + "].Key");
                                temp = data.getString(propName + ".Items[" + countItems + "].Key");

                                string currKeyData = data.getString(propName + ".Items[" + countItems + "].Key");
                                object currKey = UnSerializeObject(new JSON(currKeyData));
                                //takes the value from json
                                temp = data.get(propName + ".Items[" + countItems + "].Value");
                                temp = data.getString(propName + ".Items[" + countItems + "].Value");

                                string currValueData = data.getString(propName + ".Items[" + countItems + "].Value");
                                object currValue = UnSerializeObject(new JSON(currValueData));


                                //add the item to Dictionary
                                object Add = dictionary.GetType().GetMethod("Add").Invoke(dictionary, new object[] { currKey, currValue });

                            }
                            if (c != null)
                                ((MethodInfo)c).Invoke(result, new object[] { dictionary });
                            else if (result is ExpandoObject)
                                ((IDictionary<string, object>)result)[propName] = dictionary;

                        }
                        else
                        {
                            var obj = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(typeStr);
                            obj = UnSerializeObject(new JSON(data.get(propName)), obj, rebuildChildren);
                            if (obj != null)
                            {
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { obj });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = obj;
                            }
                        }

                    }
                    else
                    {
                        switch (JSType)
                        {
                            case JSONObject.SOType.Boolean:
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { data.getBoolean(propName) });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = data.getBoolean(propName);
                                break;
                            case JSONObject.SOType.Double:
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { data.getDouble(propName) });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = data.getDouble(propName);

                                break;
                            case JSONObject.SOType.Int:
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { data.getInt(propName) });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = data.getInt(propName);
                                break;
                            case JSONObject.SOType.DateTime:
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { data.getDateTime(propName) });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = data.getDateTime(propName);
                                break;
                            case JSONObject.SOType.String:
                                if (c != null)
                                    ((MethodInfo)c).Invoke(result, new object[] { data.getString(propName) });
                                else if (result is ExpandoObject)
                                    ((IDictionary<string, object>)result)[propName] = data.getString(propName);
                                break;
                            case JSONObject.SOType.__Array:
                            case JSONObject.SOType.__Object:
                                ((IDictionary<string, object>)result)[propName] = UnSerializeObject(new JSON(data.get(propName)));
                                break;
                        }
                    }
                }

            }

            return result;
        }

        public static void _addToJson(JSON json, string propName, object propValue, int maxLevel = int.MaxValue, int currLevel = 1)
        {
            if (currLevel > maxLevel)
                return;

            string typeStr = propValue.GetType().ToString();
            if (propValue is int)
                json.setInt(propName, (int)propValue);
            else if (propValue is double)
                json.setDouble(propName, (double)propValue);
            else if (propValue is bool)
                json.setBoolean(propName, (bool)propValue);
            else if (propValue is DateTime)
                json.setDateTime_UtcFormat(propName, (DateTime)propValue);
            else if (propValue is string)
            {
                json.setString(propName, (string)propValue);
            }
            else if (typeStr.Contains("System.Collections.Generic.Dictionary"))
            {
                if (propValue.GetType().GetMethod("get_Keys") == null) return;
                if (propValue.GetType().GetMethod("get_Item") == null) return;

                json.setString(propName + ".Type", propValue.GetType().ToString());

                object keys = propValue.GetType().GetMethod("get_Keys").Invoke(propValue, new object[] { });
                if (keys.GetType().GetMethod("GetEnumerator") == null) return;

                object enumerator = keys.GetType().GetMethod("GetEnumerator").Invoke(keys, new object[] { });
                if (enumerator.GetType().GetMethod("get_Current") == null) return;

                int count = 0;
                while ((bool)enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, new object[] { }))
                {
                    object current = enumerator.GetType().GetMethod("get_Current").Invoke(enumerator, new object[] { });
                    object ret3 = propValue.GetType().GetMethod("get_Item").Invoke(propValue, new object[] { current });

                    _addToJson(json, propName + ".Items[" + count + "].Key.Type", current.GetType().ToString(), maxLevel, currLevel + 1);
                    _addToJson(json, propName + ".Items[" + count + "].Key.Value", current, maxLevel, currLevel + 1);
                    _addToJson(json, propName + ".Items[" + count + "].Value.Type", ret3.GetType().ToString(), maxLevel, currLevel + 1);
                    _addToJson(json, propName + ".Items[" + count + "].Value.Value", ret3, maxLevel, currLevel + 1);

                    count++;
                }
            }
            else if (typeStr.Contains("System.Collections.Generic.List"))
            {
                if (propValue.GetType().GetMethod("get_Item") == null) return;

                json.setString(propName + ".Type", propValue.GetType().ToString());
                if (propValue.GetType().GetMethod("get_Count") == null) return;
                int listCount = (int)propValue.GetType().GetMethod("get_Count").Invoke(propValue, new object[] { });

                object teste = propValue.GetType().GetMethods();
                if (propValue.GetType().GetMethod("GetEnumerator") == null) return;
                object ret = propValue.GetType().GetMethod("GetEnumerator").Invoke(propValue, new object[] { });

                for (int count = 0; count < listCount; count++)
                {
                    object ret3 = propValue.GetType().GetMethod("get_Item").Invoke(propValue, new object[] { count });

                    _addToJson(json, propName + ".Items[" + count + "].Type", ret3.GetType().ToString(), maxLevel, currLevel + 1);
                    _addToJson(json, propName + ".Items[" + count + "].Value", ret3, maxLevel, currLevel + 1);
                }
            }
            else if (propValue is ExpandoObject)
            {
                IDictionary<string, object> dic = ((IDictionary<string, object>)propValue);
                if (currLevel <= maxLevel)
                {
                    foreach (var currItem in dic)
                    {
                        var serializedData = _SerializeObject(currItem.Value, maxLevel, currLevel + 1);
                        if (serializedData != null)
                            json.set(propName, serializedData);
                        else
                            json.set(propName, "null");
                    }
                }
            }
            else if (!typeStr.Contains("System.Collections.Generic"))
            {

                if (currLevel <= maxLevel)
                {
                    var serializedData = _SerializeObject(propValue, maxLevel, currLevel + 1);
                    if (serializedData != null)
                        json.set(propName, serializedData);
                    else
                        json.set(propName, "null");
                }
            }
        }

        #endregion

    }
}

