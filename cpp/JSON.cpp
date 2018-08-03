#ifndef JSON_H
    #include "JSON.h"
#endif

using namespace std;

namespace JsonMaker{

	//utils functions
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

    map<string, IJSONObject*>::const_iterator getChildByIndex (map<string, IJSONObject*> *maptosearch, int index, bool *sucess){
        map<string, IJSONObject*>::const_iterator end = (*maptosearch).end(); 

        int counter = 0;
        for (map<string, IJSONObject*>::const_iterator it = (*maptosearch).begin(); it != end; ++it) {

            if (counter == index)
            {
                *sucess = true;
                return it;
            }
            counter++;
        }

        *sucess = false;
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
}

namespace JsonMaker{
	
	//IJSONObject

    string IJSONObject::ToJson(bool quotesOnNames, bool format, int level)
    {
        stringstream result;
        
		
		vector<string> childsNames = this->__getChildsNames();
		
        if (childsNames.size() > 0)
        {
            bool array = this->isArray();
            if (array)
                result << "[";
            else
                result << "{";

            
            if (format)
                result << "\r\n";

            level++;

            for (int cont = 0; cont < childsNames.size(); cont++)
            {
                if (format)
                    for (int a = 0; a < level; a++)
                        result << "    ";
				
                auto current = this->__getChild(childsNames[cont]);
                if (array)
				{
					if ((current != NULL))
						result << current->ToJson(quotesOnNames, format, level);
				}
				else
				{
					if (quotesOnNames)
						result << '"' + childsNames[cont] + "\":" + current->ToJson(quotesOnNames, format, level);
					else
						result << childsNames[cont] + ":" + current->ToJson(quotesOnNames, format, level);
				}

                if (cont < childsNames.size() - 1)
                {
                    result << ",";
                    if (format)
                        result << "\r\n";
                }
            }

            level --;
            if (format)
            {
                result << "\r\n";
                for (int a = 0; a < level; a++)
                    result << "    ";
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

    bool IJSONObject::isArray()
    {
        int temp = 0;
        bool sucess;


		vector<string> childsNames = this->__getChildsNames();
		if (childsNames.size() == 0)
			return false;
		
        int cont = 0;
        while (cont < childsNames.size())
        {
            if (getOnly(childsNames[cont], "0123456789") != childsNames[cont])
                return false;
            cont++;
        }
        return true;
    }

	SOType IJSONObject::__determineSoType(string value)
	{
        //trye as null
        if ((value == "null") || (value == ""))
            return SOType::Null;
        else
        {
            //try as boolean
            std::transform(value.begin(), value.end(), value.begin(), ::tolower);

            if ((value == "true") || (value == "false"))
                return SOType::Boolean;
            else
            {
                //try as int
                if (getOnly(value, "0123456789") == value)
                    return SOType::Int;
                else
                {
					//try as datetime
					if (getOnly(value, "0123456789:/-+TtZz") == value)
						return SOType::DateTime;
                    //try as double
                    else{
						if (getOnly(value, "0123456789.") == value)
							return SOType::Double;
						else
						{
							//is a string
							return SOType::String;
						}
					}
                }
            }
        }
		
	}
}

namespace JsonMaker{
	//InMemoryJsonObject
	
	InMemoryJsonObject::InMemoryJsonObject(InMemoryJsonObject *pParent, string relativeName)
    {
		this->relativeName = relativeName;
        this->parent = pParent;
    }
	
	void InMemoryJsonObject::setChild(string name, IJSONObject *child)
    {
        this->childs[name] = child;
    }
	
	void InMemoryJsonObject::del(string name)
    {
        this->childs.erase(name);
    }
	
	void InMemoryJsonObject::clear()
    {
        for (const auto& curr : this->childs) {
            curr.second->clear();
            //delete[] curr->second;
        }
        this->singleValue.clear();
        this->childs.clear();
    }
	
    string InMemoryJsonObject::serializeSingleValue()
    {
        if (this->type == SOType::Null)
            return "null";
        else if (this->type == SOType::Boolean)
		{
			string temp = this->singleValue;
			std::transform(temp.begin(), temp.end(), temp.begin(), ::tolower);
			
            temp =  ((temp == "true") || (temp == "1")) ? "true" : "false";
			return temp;
		}
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
    
	SOType InMemoryJsonObject::getJSONType()
    {
        if (this->childs.size() > 0)
		{
			if (this->isArray())
				return SOType::__Array;
			else
				return SOType::__Object;
		}
		return this->type;
    }
	
    void InMemoryJsonObject::setSingleValue(string value)
    {
        this->type = this->__determineSoType(value);

		if (this->type != SOType::Null)
			this->singleValue = value;
    }
	
	vector<string> InMemoryJsonObject::__getChildsNames()
	{
		vector<string> result;
		bool sucess;
		for (int cont = 0; cont < this->childs.size(); cont++)
		{
			auto current = getChildByIndex(&(this->childs), cont, &sucess);
			if (sucess)
				result.push_back(current->first);
		}
		
		return result;
	}
	
	IJSONObject* InMemoryJsonObject::__getChild(string name)
    {
        if (this->childs.find(name) != this->childs.end())
            return childs[name];
        else
            return NULL;
    }
	
	bool InMemoryJsonObject::__containsChild(string name)
	{
		if (this->childs.find(name) != this->childs.end())
            return true;
        else
            return false;
	}
	
	string InMemoryJsonObject::getRelativeName()
	{
		return this->relativeName;
	}
	
	bool InMemoryJsonObject::isDeletable()
	{
		return false;
	}
}

namespace JsonMaker{
	
	//the json library
    void JSON::clear()
    {
        root->clear();
    }
	
	void JSON::internalInitialize(JsonType type, void* arguments)
	{
		this->jsonType = type;
		this->JsonObjectArguments = arguments;
		if (type == JsonType::Memory)
			root = new InMemoryJsonObject(NULL, "");
		//else
			//root = new FileAystemJsonObject(null, "", arguments);
	}
	
	JSON::JSON(JsonType type = JsonType::Memory, void * arguments) 
	{ 
		this->internalInitialize(type, arguments);
	}
    JSON::JSON(string JsonString, JsonType type , void* arguments)
    {
		this->internalInitialize(type, arguments);
        this->parseJson(JsonString);
    }

        
    IJSONObject *JSON::find(string objectName, bool autoCreateTree, IJSONObject *currentParent)
    {
        //quebra o nome em um array
        objectName = ReplaceString(objectName, "]", "");
        objectName = ReplaceString(objectName, "[", ".");

        std::string currentName = objectName;
        std::string childsNames = "";
        IJSONObject *childOnParent;

        if (objectName.find(".") != string::npos)
        {
            currentName = objectName.substr(0, objectName.find("."));
            childsNames = objectName.substr(objectName.find(".") + 1);
        }
		
		if (!currentParent->__containsChild(currentName))
        {
            if (autoCreateTree)
            {
				IJSONObject *tempObj;
				if (this->jsonType == JsonType::Memory)
					tempObj = new InMemoryJsonObject((InMemoryJsonObject*)currentParent, currentParent->getRelativeName() + "." + currentName);
				//else
					//tempObj = new FileSystemJsonObject((FileSystemJsonObject*)currentParent, currentParent->getRelativeName() + "." +currentName, (string)this->JsonObjectArguments);
				
				currentParent->setChild(currentName, tempObj);
            }
            else
                return NULL;
        }

        childOnParent = currentParent->__getChild(currentName);


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

        IJSONObject* temp = this->find(objectName, true, this->root);


        /*if (value[0] == '\"')
            value = value.Substring(1, value.Length - 2);*/

        //value = value.Replace("\"", "\\\"");
        temp->setSingleValue(value);

    }

    void JSON::del(IJSONObject *node)
    {
        /*auto childsNames = node->__getChildsNames();
        for (const auto& c : childsNames)
        {
            del(node->__getChild(c);
        }
        childs->clear();*/

        /*auto parentNodes = node->parent->__getChilds();
        for (int cont = 0; cont < parentNodes->size(); cont++)
        {
            auto find = getChildByIndex(parentNodes, cont, &sucess);
            if (sucess && find->second == node)
            {
                //if parent is an array, pull the elements forward backwards
                if (node->parent->isArray())
                {
                    for (int cont2 = cont; cont2 < parentNodes->size() - 1; cont2++)
                        (*parentNodes)[getChildByIndex(parentNodes, cont2, &sucess)->first] = (*parentNodes)[getChildByIndex(parentNodes, cont2 + 1, &sucess)->first];

                    parentNodes->erase(parentNodes->rend()->first);
                }
                else
                {
                    parentNodes->erase(parentNodes->rend()->first);
                }
                break;
            }
        }*/
    }

    vector<string> JSON::getObjectsNames(IJSONObject *currentItem)
    {
        vector<string> retorno;

        if (currentItem == NULL)
            currentItem = this->root;


        string parentName = "";

        vector<string> childsNames;
        bool sucess;

		auto childsNamesList = currentItem->__getChildsNames();
        for (int cont = 0; cont < childsNamesList.size(); cont++)
        {

            childsNames = getObjectsNames(currentItem->__getChild(childsNamesList[cont]));


            parentName = childsNamesList[cont];
            //adiciona os filhos ao resultado
            //verifica se o nome atual atende ao filtro
            for (const auto& att : childsNames)
            {
                string nAtt = att;
                if (nAtt != "")
                    nAtt = parentName + '.' + nAtt;

                retorno.push_back(nAtt);
            }
            retorno.push_back(childsNamesList[cont]);
        }
        return retorno;

    }

    vector<string> JSON::getChildsNames(IJSONObject *currentItem)
    {
        vector<string> retorno;
        bool sucess;

        if (currentItem == NULL)
            currentItem = this->root;
		
		auto childsNames = currentItem->__getChildsNames();

        for (int cont = 0; cont < childsNames.size(); cont++)
        {
            retorno.push_back(childsNames[cont]);
        }
        return retorno;
    }

    vector<string> JSON::getJsonFields(string json)
    {
        int open = 0;
        vector<string> fields;
        stringstream temp;
        bool quotes = false;
		bool skeepNext = false;

        for (int cont = 1; cont < json.size() - 1; cont++)
        {
			if (skeepNext)
			{
				temp << json[cont];
				skeepNext = false;
				continue;
			}
			
            if (json[cont] == ',')
            {
                if ((open == 0) && (!quotes))
                {
                    fields.push_back(temp.str());
                    //clear the stringstream
					temp.str(std::string());
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
				else if (json[cont] =='\\')
				{
					skeepNext = true;
				}

                if (json[cont] == '"')
                {
                    //if ((json[cont - 1] != '\\') || (json[cont - 2] == '\\'))
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
		bool skeepNext = false;
        string specialchars1 = "\r\n\t\0 ";
        for (const auto& att : json)
        {
			if (skeepNext)
			{
				result << att;	
				skeepNext = false;
				continue;
			}
			
            if (att == '\"')
                quotes = !quotes;

            if (!quotes)
            {
                if (specialchars1.find(att) == string::npos)
                    result << att;
            }
            else
            {
				if (att == '\\')
				{
					skeepNext = true;
				}
                result << att;
            }
        }

        return result.str();
    }

    bool JSON::isAJson(string json, bool objects, bool arrays)
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

    /// <summary>
    /// Removes an object from JSON three
    /// </summary>
    /// <param name="objectName">The object name</param>
    void JSON::del(string objectName)
    {
        IJSONObject *temp = this->find(objectName, false, this->root);
        if (temp != NULL)
            del(temp);
    }

    void JSON::clearChilds(string objectName)
    {
        IJSONObject *temp = this->find(objectName, false, this->root);
        bool sucess;
        if (temp != NULL)
        {
            //auto childs = temp->__getChilds();
			auto names = temp->__getChildsNames();
            for (const auto& c : names)
            {
                del(temp->__getChild(c));
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
    string JSON::ToJson(bool format)
    {
        std::string result = this->root->ToJson(true, format);
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
    string JSON::get(string objectName, bool format, bool quotesOnNames)
    {
        IJSONObject *temp = this->find(objectName, false, this->root);
        if (temp != NULL)
            return temp->ToJson(quotesOnNames, format);
        else
            return "null";

    }

    

    /// <summary>
    /// Return all names of the json three of an object
    /// </summary>
    /// <param name="objectName">The name of object</param>
    /// <returns></returns>
    vector<string> JSON::getObjectsNames(string objectName)
    {
        if (objectName == "")
        {
            IJSONObject *nullo = NULL;
            return this->getObjectsNames(nullo);
        }
        else
        {
            IJSONObject* finded = this->find(objectName, false, this->root);
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
    vector<string> JSON::getChildsNames(string objectName)
    {
        if (objectName == "")
        {
            IJSONObject *nullo = NULL;
            return this->getChildsNames(nullo);
        }
        else
        {
            IJSONObject* finded = this->find(objectName, false, this->root);
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

	
    void JSON::parseJson(string json, string parentName)
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
	
	SOType JSON::getJSONType(string objectName)
	{
		IJSONObject *temp = this->find(objectName, false, this->root);
		if (temp != NULL)
		{
			return temp->getJSONType();
		}
		else
			return SOType::Null;
	}

    
    /// <summary>
    /// Get a json property as string
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    string JSON::getString(string name, string defaultValue)
    {
        string result = this->get(name);


        if ((result.size() > 0) && (result[0] == '"'))
            result = result.substr(1);
        if ((result.size() > 0) && (result[result.size() - 1] == '"'))
            result = result.substr(0, result.size() - 1);

        if (result.size() > 0)
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
    int JSON::getInt(string name, int defaultValue)
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
    long JSON::getLong(string name, long defaultValue)
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
    bool JSON::getBoolean(string name, bool defaultValue)
    {
        string temp = this->get(name);
        if (temp != "")
        {
			std::transform(temp.begin(), temp.end(), temp.begin(), ::tolower);
            if (temp == "true")
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
		std::transform(name.begin(), name.end(), name.begin(), ::tolower);
		
        this->set(name, name);
    }

    /// <summary>
    /// Get a json property as double. To get a custom DateTime, please use getString
    /// </summary>
    /// <param name="name">Object name of the property</param>
    /// <param name="defaultValue">Value to be returned when the property is not found</param>
    /// <returns></returns>
    double JSON::getDouble(string name, double defaultValue)
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
    int JSON::getArrayLength(string objectName)
    {
        IJSONObject* finded = this->find(objectName, false, this->root);

        if (finded != NULL)
            return finded->__getChildsNames().size();
        return 0;
    }

    void JSON::Dispose()
    {
        this->clear();
    }
	
}