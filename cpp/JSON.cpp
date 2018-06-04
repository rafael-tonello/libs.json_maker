#ifndef JSON_H
    #include "JSON.h"
#endif

using namespace std;

namespace JsonMaker{

    string getOnly(string source, string allowed)
    {
        stringstream ret;
        for (const auto& curr : source)
        {
            if (allowed.find(curr) != string::npos)
                ret << curr;
        }

        return ret.str();
    }

    std::string ReplaceString(std::string subject, const std::string& search, const std::string& replace) {
        size_t pos = 0;
        while ((pos = subject.find(search, pos)) != std::string::npos) {
            subject.replace(pos, search.length(), replace);
            pos += replace.length();
        }
        return subject;
    }

    map<string, JSONObject*>::const_iterator getChildByIndex (map<string, JSONObject*> *maptosearch, int index){
        map<string, JSONObject*>::const_iterator end = (*maptosearch).end(); 

        int counter = 0;
        for (map<string, JSONObject*>::const_iterator it = (*maptosearch).begin(); it != end; ++it) {
            counter++;

            if (counter == index)
                return it;
        }
    }

    // trim from start (in place)
    static inline void ltrim(std::string &s) {
        s.erase(s.begin(), std::find_if(s.begin(), s.end(),
                std::not1(std::ptr_fun<int, int>(std::isspace))));
    }

    // trim from end (in place)
    static inline void rtrim(std::string &s) {
        s.erase(std::find_if(s.rbegin(), s.rend(),
                std::not1(std::ptr_fun<int, int>(std::isspace))).base(), s.end());
    }

    // trim from both ends (in place)
    static inline void trim(std::string &s) {
        ltrim(s);
        rtrim(s);
    }

    // trim from start (copying)
    static inline std::string ltrim_copy(std::string s) {
        ltrim(s);
        return s;
    }

    // trim from end (copying)
    static inline std::string rtrim_copy(std::string s) {
        rtrim(s);
        return s;
    }

    // trim from both ends (copying)
    static inline std::string trim_copy(std::string s) {
        trim(s);
        return s;
    }

    string __unescapeString(string data)
    {
        //result = result.Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
        string nValue = "";
        int cont;
        for (cont = 0; cont < data.size() - 1; cont++)
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
        if (cont < data.size())
            nValue = nValue + data[cont];

        return nValue;
    }

    string JSONObject::serializeSingleValue()
    {
        std::locale loc;
        if (this->type == SOType::Null)
            return "null";
        else if (this->type == SOType::Boolean)
            return ((std::tolower(this->singleValue, loc) == "true") || (this->singleValue == "1")) ? "true" : "false";
        else if (this->type == SOType::String)
        {
            if ((this->singleValue.size() > 0) && (this->singleValue[0] != '"'))
                return '"' + this->singleValue + '"';
            else
                return this->singleValue;
        }
        else if (this->type == SOType::Double)
        {
            std::replace( this->singleValue.begin(), this->singleValue.end(), ',', '.'); // replace all 'x' to 'y'
            return this->singleValue;
        }
        else
            return this->singleValue;
    }




    JSONObject::JSONObject(JSONObject *pParent)
    {
        this->parent = pParent;
    }

    void JSONObject::setChild(string name, JSONObject *child)
    {
        this->childs[name] = child;
    }

    void JSONObject::del(string name)
    {
        this->childs.erase(name);
    }

    JSONObject* JSONObject::JSONObject::get(string name)
    {
        if (this->childs.find(name) != this->childs.end())
            return childs[name];
        else
            return NULL;
    }

    void JSONObject::clear()
    {
        for (const auto& curr : this->childs) {
            curr.second->clear();
        }
        this->childs.clear();
    }

    string JSONObject::ToJson(bool quotesOnNames)
    {
        stringstream result;

        if (this->childs.size() > 0)
        {
            bool array = this->isArray();
            if (array)
                result << "[";
            else
                result << "{";

            for (int cont = 0; cont < this->childs.size(); cont++)
            {
                auto current = getChildByIndex(&(this->childs), cont);
                if (array)
                {
                    if (current->second != NULL)
                        result << current->second->ToJson(quotesOnNames);
                }
                else
                {
                    if (quotesOnNames)
                        result << '"' + current->first + "\":" + current->second->ToJson(quotesOnNames);
                    else
                        result << current->first + ":" + current->second->ToJson(quotesOnNames);
                }

                if (cont < this->childs.size() - 1)
                    result << ',';
            }

            if (array)
                result << "]";
            else
                result << "}";

            return result.str();
        }
        else
        {
            return serializeSingleValue();
        }
    }

    

    void JSONObject::setSingleValue(string value)
    {
        int sucess = 0;
        double sucess2 = 0;

        //trye as null
        if ((value == "null") || (value == ""))
            this->type = SOType::Null;
        else
        {
            //try as boolean
            this->singleValue = value;

            if ((value == "true") || (value == "false"))
                this->type = SOType::Boolean;
            else
            {
                //try as int
                if (getOnly(value, "0123456789") == value)
                    this->type = SOType::Int;
                else
                {
                    //try as double
                    if (getOnly(value, "0123456789.") == value)
                        this->type = SOType::Double;
                    else
                    {
                        //is a string
                        this->type = SOType::String;
                    }
                }
            }
        }
    }

    bool JSONObject::isArray()
    {
        int temp = 0;

        int cont = 0;
        while (cont < this->childs.size())
        {
            auto curr = getChildByIndex(&(this->childs), cont);
            if (getOnly(curr->first, "0123456789") != curr->first)
                return false;
            cont++;
        }
        return true;
    }

    map<string, JSONObject*> *JSONObject::__getChilds()
    {
        return &(this->childs);
    }

    void JSON::clear()
    {
        root->clear();
    }

        
    JSONObject *JSON::find(string objectName, bool autoCreateTree, JSONObject *currentParent)
    {
        //quebra o nome em um array
        objectName = ReplaceString(objectName, "]", "");
        objectName = ReplaceString(objectName, "[", ".");

        std::string currentName = objectName;
        std::string childsNames = "";
        JSONObject *childOnParent;

        if (objectName.find(".") > string::npos)
        {
            currentName = objectName.substr(0, objectName.find("."));
            childsNames = objectName.substr(objectName.find(".") + 1);
        }

        if (currentParent->__getChilds()->find(currentName) == currentParent->__getChilds()->end())
        {
            if (autoCreateTree)
                (*(currentParent->__getChilds()))[currentName] = new JSONObject(currentParent);
            else
                return NULL;
        }

        childOnParent = (*(currentParent->__getChilds()))[currentName];


        if (childsNames == "")
        {
            return childOnParent;
        }
        else
        {
            return this->find(childsNames, autoCreateTree, childOnParent);
        }
    }

    void JSON::_set(string objectName, string value)
    {

        if (this->isAJson(value))
        {
            this->parseJson(value, objectName);
            return;
        }

        JSONObject temp = this->find(objectName, true, this->root);

        /*if (value[0] == '\"')
            value = value.Substring(1, value.Length - 2);*/

        //value = value.Replace("\"", "\\\"");

        temp.setSingleValue(value);

    }

    void JSON::del(JSONObject *node)
    {
        auto childs = node->__getChilds();
        while (childs->size() > 0)
        {
            del(getChildByIndex(childs, 0)->second);
        }
        childs->clear();

        auto parentNodes = node->parent->__getChilds();
        for (int cont = 0; cont < parentNodes->size(); cont++)
        {
            if (getChildByIndex(parentNodes, cont)->second == node)
            {
                //if parent is an array, pull the elements forward backwards
                if (node->parent->isArray())
                {
                    for (int cont2 = cont; cont2 < parentNodes->size() - 1; cont2++)
                        (*parentNodes)[getChildByIndex(parentNodes, cont2)->first] = (*parentNodes)[getChildByIndex(parentNodes, cont2 + 1)->first];

                    parentNodes->erase(parentNodes->rend()->first);
                }
                else
                {
                    parentNodes->erase(parentNodes->rend()->first);
                }
                break;
            }
        }
    }

    vector<string> JSON::getObjectsNames(JSONObject *currentItem = NULL)
    {
        vector<string> retorno;

        if (currentItem == NULL)
            currentItem = this->root;


        string parentName = "";

        vector<string> childsNames;

        for (int cont = 0; cont < currentItem->__getChilds()->size(); cont++)
        {

            childsNames = getObjectsNames(getChildByIndex(currentItem->__getChilds(), cont)->second);


            parentName = getChildByIndex(currentItem->__getChilds(), cont)->first;
            //adiciona os filhos ao resultado
            //verifica se o nome atual atende ao filtro
            for (const auto& att : childsNames)
            {
                string nAtt = att;
                if (nAtt != "")
                    nAtt = parentName + '.' + nAtt;

                retorno.push_back(nAtt);
            }
            retorno.push_back(getChildByIndex(currentItem->__getChilds(), cont)->first);
        }
        return retorno;

    }

    vector<string> JSON::getChildsNames(JSONObject *currentItem = NULL)
    {
        vector<string> retorno;

        if (currentItem == NULL)
            currentItem = this->root;

        for (int cont = 0; cont < currentItem->__getChilds()->size(); cont++)
        {
            retorno.push_back(getChildByIndex(currentItem->__getChilds(), cont)->first);
        }
        return retorno;
    }

    vector<string> JSON::getJsonFields(string json)
    {
        int open = 0;
        vector<string> fields;
        stringstream temp;
        bool quotes = false;

        for (int cont = 1; cont < json.size() - 1; cont++)
        {
            if (json[cont] == ',')
            {
                if ((open == 0) && (!quotes))
                {
                    fields.push_back(temp.str());
                    temp.clear();
                }
                else
                    //if ((quotes) || (temp.Length == 0) || (!"}]".Contains(temp[temp.Length - 1])))
                    temp << json[cont];
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
                temp << json[cont];
            }


        }
        
        //get size of temp
        temp.seekg(0, ios::end);
        int size = temp.tellg();
        if (size > 0)
            fields.push_back(temp.str());

        return fields;

    }

    string JSON::clearJsonString(string json)
    {
        stringstream result;

        bool quotes = false;
        char oldOldAtt = ' ';
        char oldAtt = ' ';
        string specialchars1 = "\r\n\t\0 ";
        for (const auto& att : json)
        {
            if ((att == '\"') && ((oldAtt != '\\') || (oldOldAtt == '\\')))
                quotes = !quotes;

            if (!quotes)
            {
                if (specialchars1.find(att) == string::npos)
                    result << att;
            }
            else
            {
                result << att;
            }

            oldOldAtt = oldAtt;
            oldAtt = att;
        }

        return result.str();
    }

    bool JSON::isAJson(string json, bool objects = true, bool arrays = true)
    {
        bool quotes = false;

        char oldAtt = (char)0;
        int cont = 0;
        json = ltrim_copy(json);

        string objectkeys = "{}";
        string vectorkeys = "[]";
        for (const auto& att : json)
        {
            cont++;
            if ((att == '\"') && (oldAtt != '\\'))
                quotes = !quotes;

            if (!quotes)
            {
                if (att == ':')
                    return true;
                if ((objects) && (objectkeys.find(att) != string::npos) && (cont == 0))
                    return true;
                else if ((arrays) && (vectorkeys.find(att) != string::npos) && (cont == 0))
                    return true;
            }
            oldAtt = att;
        }

        return false;
    }

    JSON::JSON() { }
    JSON::JSON(string JsonString)
    {
        this->parseJson(JsonString);
    }

    /// <summary>
    /// Removes an object from JSON three
    /// </summary>
    /// <param name="objectName">The object name</param>
    void JSON::del(string objectName)
    {
        JSONObject *temp = this->find(objectName, false, this->root);
        if (temp != NULL)
            del(temp);
    }

    void JSON::clearChilds(string objectName)
    {
        JSONObject *temp = this->find(objectName, false, this->root);
        if (temp != NULL)
        {
            auto childs = temp->__getChilds();
            while (childs->size() > 0)
            {
                del(getChildByIndex(childs, 0)->second);
            }
        }
    }

    /// <summary>
    /// Set or creates an property with an json string
    /// </summary>
    /// <param name="objectName">The json object name</param>
    /// <param name="value">The json string </param>
    void JSON::set(string objectName, string value)
    {

        if (objectName != "")
            objectName = objectName + ":";
        this->parseJson(objectName + value);

    }

    /// <summary>
    /// Insert a new json in current json three
    /// </summary>
    /// <param name="objectName">Name of the object</param>
    /// <param name="toImport">Json to be imported</param>
    void JSON::set(string objectName, JSON *toImport)
    {
        if (objectName != "")
            objectName = objectName + ":";
        this->parseJson(objectName + toImport->ToJson());

    }

    /// <summary>
    /// Serialize the Json three
    /// </summary>
    /// <param name="quotesOnNames">User '"' in name of objects</param>
    /// <returns></returns>
    string JSON::ToJson(bool quotesOnNames = true)
    {
        std::string result = root->ToJson(quotesOnNames);
        return result;
    }

    string JSON::ToString()
    {
        return this->ToJson();
    }

    /// <summary>
    /// Return true if the an object is in json three
    /// </summary>
    /// <param name="objectName">The object name</param>
    /// <returns></returns>
    bool JSON::contains(string objectName)
    {
        bool result = this->find(objectName, false, this->root) != NULL;
        return result;
    }

    /// <summary>
    /// returns the value of an json object as a json string (Serialize an object)
    /// </summary>
    /// <param name="objectName">The object name</param>
    /// <param name="quotesOnNames">User '"' in names</param>
    /// <returns></returns>
    string JSON::get(string objectName, bool quotesOnNames = true)
    {
        JSONObject *temp = this->find(objectName, false, this->root);
        if (temp != NULL)
            return temp->ToJson(quotesOnNames);
        else
            return "null";

    }

    

    /// <summary>
    /// Return all names of the json three of an object
    /// </summary>
    /// <param name="objectName">The name of object</param>
    /// <returns></returns>
    vector<string> JSON::getObjectsNames(string objectName = "")
    {
        if (objectName == "")
        {
            JSONObject *nullo = NULL;
            return this->getObjectsNames(nullo);
        }
        else
        {
            auto finded = this->find(objectName, false, this->root);
            vector<string> retorno;
            if (finded != NULL)
                retorno = this->getObjectsNames(finded);

            return retorno;
        }
    }

    /// <summary>
    /// Return the childNames of an json object
    /// </summary>
    /// <param name="objectName">The name of object</param>
    /// <returns></returns>
    vector<string> JSON::getChildsNames(string objectName = "")
    {
        if (objectName == "")
        {
            JSONObject *nullo = NULL;
            return this->getChildsNames(nullo);
        }
        else
        {
            auto finded = this->find(objectName, false, this->root);
            vector<string> retorno;
            if (finded != NULL)
                retorno = this->getChildsNames(finded);
            return retorno;
        }

    }


    void JSON::fromJson(string json)
    {
        this->parseJson(json);
    }

    void JSON::fromString(string json)
    {
        this->parseJson(json);

    }

    void JSON::parseJson(string json, string parentName = "")
    {
        //limpa o json, removendo coisas desnecessárias como espaços em branco e tabs
        json = clearJsonString(json);
        string name = "";

        string value = json;

        //verifica se o json é uma par chave<-> valor. Se for pega o nome
        if (json.find(":") != string::npos)
        {
            name = "";
            int index = 0;
            string allowdNameChars = "\"_ABCDEFGHIJKLMNOPQRSTUVXYWZabcdefghijklmnop.qrstuvxywz0123456789[] ";
            while (json[index] != ':')
            {
                if (allowdNameChars.find(json[index]) != string::npos)
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
                value = json.substr(json.find(":") + 1);

        }

        //remove aspas do nome, caso houverem
        name = ReplaceString(name, "\"", "");


        //se tiver um '{' ou um '[', então processa independentemente cacda um de seus valroes
        vector<string> childs;
        if ((value != "") && (value[0] == '['))
        {
            childs = getJsonFields(value);
            for (int cont = 0; cont < childs.size(); cont++)
                childs[cont] = cont + ":" + childs[cont];
        }
        else if ((value != "") && (value[0] == '{'))
            childs = getJsonFields(value);
        else
            childs.push_back(value);



        //parapara o nome do objeto
        if ((parentName != "") && (name != ""))
            name = '.' + name;


        name = parentName + name;

        //se for um array, cria um novo array


        auto tempName = name;
        for (const auto& att : childs)
        {
            //se for uma string, remove as aspas do inicio e do final
            //
            auto toInsert = att;

            //adiciona o objeto à lista
            if (toInsert != json)
                this->_set(tempName, toInsert);
        }
    }

    
    /// <summary>
    /// Get a json property as string
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    string JSON::getString(string name, string defaultValue = "")
    {
        string result = this->get(name);
        if ((result.size() > 0) && (result[0] == '"'))
            result = result.substr(1);
        if ((result.size() > 0) && (result[result.size() - 1] == '"'))
            result = result.substr(0, result.size() - 1);

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
    void JSON::setString(string name, string value)
    {

        value = ReplaceString(value, "\\", "\\\\");
        value = ReplaceString(value, "\"", "\\\"");
        value = ReplaceString(value, "\r", "\\r");
        value = ReplaceString(value, "\n", "\\n");
        value = ReplaceString(value, "\t", "\\t");
        this->set(name, '"' + value + '"');
    }

    /// <summary>
    /// Get a json property as int
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    int JSON::getInt(string name, int defaultValue = 0)
    {
        string temp = getOnly(this->get(name), "0123456789-");
        if (temp != "")
            return std::stoi(temp);
        else return defaultValue;
    }

    /// <summary>
    /// Set or create a property as int
    /// </summary>
    /// <param name="name">The property object name </param>
    /// <param name="value">The value</param>
    void JSON::setInt(string name, int value)
    {
        this->set(name, std::to_string(value));
    }

    /// <summary>
    /// Get a json property as Int64
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    long JSON::getLong(string name, long defaultValue = 0)
    {
        string temp = getOnly(this->get(name), "0123456789-");
        if (temp != "")
            return std::stol(temp);
        else return defaultValue;
    }

    /// <summary>
    /// Set or create a property as int64
    /// </summary>
    /// <param name="name">The property object name </param>
    /// <param name="value">The value</param>
    void JSON::setLong(string name, long value)
    {
        this->set(name, std::to_string(value));
    }

    /// <summary>
    /// Get a json property as boolean
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    bool JSON::getBoolean(string name, bool defaultValue = false)
    {
        std::locale loc;
        string temp = this->get(name);
        if (temp != "")
        {
            if (std::tolower(temp, loc) == "true")
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
    void JSON::setBoolean(string name, bool value)
    {
        std::locale loc;
        this->set(name, std::tolower(std::to_string(value), loc));
    }

    /// <summary>
    /// Get a json property as double. To get a custom DateTime, please use getString
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    double JSON::getDouble(string name, double defaultValue = 0)
    {
        //string temp = getOnly(this->get(name).Replace('.', ','), "0123456789-,");
        string temp = getOnly(this->get(name), "0123456789-,.");
        if (temp != "")
            return std::stod(temp);
        else return defaultValue;
    }

    /// <summary>
    /// Set or create a property as Double
    /// </summary>
    /// <param name="name">The property object name </param>
    /// <param name="value">The value</param>
    void JSON::setDouble(string name, double value)
    {
        string temp = std::to_string(value);
        temp = ReplaceString(temp, ",", ".");
        this->set(name, temp);
    }

    /// <summary>
    /// Return the childs count of an object (like arrays or objects)
    /// </summary>
    /// <param name="objectName">The name of the object</param>
    /// <returns></returns>
    int JSON::getArrayLength(string objectName = "")
    {
        auto finded = this->find(objectName, false, this->root);

        if (finded != NULL)
            return finded->__getChilds()->size();
        return 0;
    }

    void JSON::Dispose()
    {
        this->clear();
    }
}