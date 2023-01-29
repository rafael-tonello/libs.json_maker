#ifndef JSONMAKER_H
#define JSONMAKER_H

#include <iostream>
#include <string>
#include <strings.h>
#include <sstream>
#include <vector>
#include <map>
#include <algorithm>


//include used int trim functions
#include <functional>
#include <cctype>
#include <locale>

using namespace std;

namespace JsonMaker{
	enum SOType { Null, String, DateTime, Int, Double, Boolean, __Object, __Array, Undefined };
	enum JsonType { Memory, File};
	class IJSONObject
    {
		protected:
			//properties


			//methods
			SOType __determineSoType(string value);
			string serializeSingleValue();

            string className = "IJSONObject";
            

        public:
            virtual ~IJSONObject(){};


			//public properties
            //used by parser only

            IJSONObject *parser_parent;
            string parser_name;
			    
            
            /*void setParser_Parent(IJSONObject* nParent){};
            void setParser_Name(string name);
            IJSONObject* getParser_Parent();
            string getParser_Name();*/
            

			bool isArray();
			void forceType(SOType forcedType);
            SOType getJSONType();
			string ToJson(bool quotesOnNames, bool format = false, int level = 0);

			//public methods

            //This method must be used to clear used data. But atention: this method is called only in the root node.
            //So, if the child nodes needs to be cleared too, you need call their 'clear' methods be your own.
            //Clear method is called in preparation to a deletation of the entire json data, differently from method
            //'del' which is called in preparation to clear only a single node.
            virtual void clear() = 0;

            //this methos is called right after the object creation
			virtual void Initialize(string relativeName) =  0;

            //this method is called when a single child node must be deleted. This method can call the clear function of 
            //the child node before delete them
            virtual void del(string name) = 0;

            //should return the name passed in the Initialize function
			virtual string getRelativeName() = 0;

            //adds a new child to the current node
            virtual void setChild(string name, IJSONObject *child) = 0;

            //set a single value of the node (this value can be a bool, integer, string ...)
            virtual void setSingleValue(string value) = 0;

            //return the single value of the node (the value setted with setSingleValue method)
            virtual string getSingleValue() = 0;

            //This function is very important to the JSON library when it needs to create new nodes.
            //Should returna  new instance of the class. 
            virtual IJSONObject* createNewInstance() = 0;

            //just return true or fals to indicade if the node contains a certain child
			virtual bool __containsChild(string name, bool caseSensitive = false) = 0;

            //returns a child
            virtual IJSONObject* __getChild(string name, bool caseSensitive = false) = 0;

            //return a list with the names of all childs of the node
			virtual vector<string> __getChildsNames() = 0;

            //should store some data that is needed by the JSON library
            virtual void __storeInternalProp(string name,string value) = 0;

            //return properties saved with __storeInternalProp
            virtual string __getInternalProp(string name, string defaultValue) = 0;

            //save comments readed in the JSON file (comments are a feature of the JSON maker, and is not supported by the JSON specs)
            virtual void addComment(string comment) = 0;

            //return the list of comments added with 'addComment' function
            virtual vector<string> getComments() = 0;


    };

    class InMemoryJsonObject: public IJSONObject
    {
        protected:

            vector<string> comments;

            map<string, IJSONObject*> childs;
			string singleValue;
			string relativeName;
            map<string, string> tags;


        public:
            InMemoryJsonObject(){this->className = "InMemoryJsonObject";}
			void clear();
			void Initialize(string relativeName);
			void del(string name);
			string getRelativeName();
            void setChild(string name, IJSONObject *child);
            void setSingleValue(string value);
            string getSingleValue();
            IJSONObject* createNewInstance();
			bool __containsChild(string name, bool caseSensitive = false);
			IJSONObject* __getChild(string name, bool caseSensitive = false);
			vector<string> __getChildsNames();

            void __storeInternalProp(string name,string value);
            string __getInternalProp(string name, string defaultValue);

            void addComment(string comment);
            vector<string> getComments();

            //InMemoryJsonObject(InMemoryJsonObject *pParent, string relativeName);
			//IJSONObject* get(string name);


    };


    class JSON
    {

        private:

            int commentNextNameCount = 0;
			JsonType jsonType = JsonType::Memory;
            IJSONObject *root;
            IJSONObject *modelObject;
            bool caseSensitiveToFind;

			void internalInitialize(IJSONObject *_modelObject = NULL);


            IJSONObject *find(string objectName, bool autoCreateTree, IJSONObject *currentParent, SOType forceType = SOType::Undefined);
            void _set(string objectName, string value);
            void del(IJSONObject *node);
            vector<string> getObjectsNames(IJSONObject *currentItem = NULL);
            vector<string> getChildsNames(IJSONObject *currentItem = NULL);
            vector<string> getJsonFields(string json);
            string clearJsonString(string json);
            bool isAJson(string json, bool objects = true, bool arrays = true);
            string getNextCommentChildName();

        public:
            
			JSON(bool caseSensitiveToFind, IJSONObject *_modelObject = NULL);
			JSON(string JsonString, bool caseSensitiveToFind, IJSONObject *_modelObject = NULL);
            JSON(string jsonString):JSON(jsonString, true, NULL){};
            JSON():JSON(true, NULL){};
			~JSON();

            JSON(const JSON &cp2)
            {
                if (dynamic_cast<InMemoryJsonObject*>(cp2.modelObject))
			        this->internalInitialize(new InMemoryJsonObject());
                else
                    this->internalInitialize();
                
                string data = ((JSON&)cp2).ToJson();
                this->parseJson(data);
            }

            JSON& operator =(const JSON& other)
            {
                if (dynamic_cast<InMemoryJsonObject*>(other.modelObject))
			        this->internalInitialize(new InMemoryJsonObject());
                else
                    this->internalInitialize();

                string data = ((JSON&)other).ToJson();
                this->parseJson(data);

                return *this;
            }

            /// <summary>
            /// Removes an object +from JSON three
            /// </summary>
            /// <param name="objectName">The object name</param>
            void del(string objectName);

            void clearChilds(string objectName);

            /// <summary>
            /// Set or creates an property with an json string
            /// </summary>
            /// <param name="objectName">The json object name</param>
            /// <param name="value">The json string </param>
            void set(string objectName, string value, SOType forceType = SOType::Undefined);

            /// <summary>
            /// Insert a new json in current json three
            /// </summary>
            /// <param name="objectName">Name of the object</param>
            /// <param name="toImport">Json to be imported</param>
            void set(string objectName, JSON *toImport);

            /// <summary>
            /// Serialize the Json three
            /// </summary>
            /// <param name="quotesOnNames">User '"' in name of objects</param>
            /// <returns></returns>
            string ToJson(bool format = false);

            string ToString();

            operator string(){ return ToString();}
            string operatorCharTmp;
            operator char*(){
                operatorCharTmp = ToString();
                return (char*)operatorCharTmp.c_str();
            }
            

            /// <summary>
            /// Return true if the an object is in json three
            /// </summary>
            /// <param name="objectName">The object name</param>
            /// <returns></returns>
            bool contains(string objectName);

            /// <summary>
            /// returns the value of an json object as a json string (Serialize an object)
            /// </summary>
            /// <param name="objectName">The object name</param>
            /// <param name="quotesOnNames">User '"' in names</param>
            /// <returns></returns>
            string get(string objectName, bool format = false, bool quotesOnNames = true, string valueOnNotFound = "undefined");

			IJSONObject* getRaw(string objectName);

            /// <summary>
            /// Return all names of the json three of an object
            /// </summary>
            /// <param name="objectName">The name of object</param>
            /// <returns></returns>
            vector<string> getObjectsNames(string objectName = "");

            /// <summary>
            /// Return the childNames of an json object
            /// </summary>
            /// <param name="objectName">The name of object</param>
            /// <returns></returns>
            vector<string> getChildsNames(string objectName = "");


            void fromJson(string json, bool tryParseInvalidJson = false);
			void fromString(string json, bool tryParseInvalidJson = false);

            void parseJson(string json, string parentName = "", bool tryParseInvalidJson = false, SOType forceType = SOType::Undefined);


			SOType getJSONType(string objectName);

            /// <summary>
            /// Get a json property as string
            /// </summary>
            /// <param name="name">Object name of the property</param>
            /// <param name="defaultValue">Value to be returned when the property is not found</param>
            /// <returns></returns>
            string getString(string name, string defaultValue = "");

            /// <summary>
            /// Set or create a property as string
            /// </summary>
            /// <param name="name">The property object name </param>
            /// <param name="value">The value</param>
            void setString(string name, string value, bool tryRedefineType = false);

            /// <summary>
            /// Get a json property as int
            /// </summary>
            /// <param name="name">Object name of the property</param>
            /// <param name="defaultValue">Value to be returned when the property is not found</param>
            /// <returns></returns>
            int getInt(string name, int defaultValue = 0);

            /// <summary>
            /// Set or create a property as int
            /// </summary>
            /// <param name="name">The property object name </param>
            /// <param name="value">The value</param>
            void setInt(string name, int value);

            /// <summary>
            /// Get a json property as Int64
            /// </summary>
            /// <param name="name">Object name of the property</param>
            /// <param name="defaultValue">Value to be returned when the property is not found</param>
            /// <returns></returns>
            long getLong(string name, long defaultValue = 0);

            /// <summary>
            /// Set or create a property as int64
            /// </summary>
            /// <param name="name">The property object name </param>
            /// <param name="value">The value</param>
            void setLong(string name, long value);

            /// <summary>
            /// Get a json property as boolean
            /// </summary>
            /// <param name="name">Object name of the property</param>
            /// <param name="defaultValue">Value to be returned when the property is not found</param>
            /// <returns></returns>
            bool getBoolean(string name, bool defaultValue = false);

            /// <summary>
            /// Set or create a property as boolean
            /// </summary>
            /// <param name="name">The property object name </param>
            /// <param name="value">The value</param>
            void setBoolean(string name, bool value);

            /// <summary>
            /// Get a json property as double. To get a custom DateTime, please use getString
            /// </summary>
            /// <param name="name">Object name of the property</param>
            /// <param name="defaultValue">Value to be returned when the property is not found</param>
            /// <returns></returns>
            double getDouble(string name, double defaultValue = 0);

            /// <summary>
            /// Set or create a property as Double
            /// </summary>
            /// <param name="name">The property object name </param>
            /// <param name="value">The value</param>
            void setDouble(string name, double value);

            /// <summary>
            /// Return the childs count of an object (like arrays or objects)
            /// </summary>
            /// <param name="objectName">The name of the object</param>
            /// <returns></returns>
            size_t getArrayLength(string objectName = "");

            void clear();
            void Dispose();
    };

    string getOnly(string source, string allowed);

    std::string ReplaceString(std::string subject, const std::string& search, const std::string& replace);

    map<string, IJSONObject*>::const_iterator getChildByIndex (map<string, IJSONObject*> *maptosearch, int index, bool *sucess);

    // trim from start (in place)
    //static inline void ltrim(std::string &s);

    // trim from end (in place)
    inline void rtrim(std::string &s);

    // trim from both ends (in place)
    inline void trim(std::string &s);

    // trim from start (copying)
    inline std::string ltrim_copy(std::string s);

    // trim from end (copying)
    inline std::string rtrim_copy(std::string s);

    // trim from both ends (copying)
    inline std::string trim_copy(std::string s);

    string __unescapeString(string data);
}


#endif // JSONMAKER_H
