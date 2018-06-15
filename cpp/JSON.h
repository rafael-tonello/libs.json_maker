#define JSON_H

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
	enum SOType { Null, String, Int, Double, Boolean };
    class JSONObject
    {
        protected:
            
            map<string, JSONObject*> childs;
            SOType type = SOType::Null;

            string serializeSingleValue();
        
        public:
            string singleValue;
            JSONObject *parent;
            JSONObject(JSONObject *pParent);
            void setChild(string name, JSONObject *child);
            void del(string name);
            JSONObject* get(string name);
            void clear();
            string ToJson(bool quotesOnNames, bool format = false, int level = 0);
			SOType getJSONType();
            void setSingleValue(string value);
            bool isArray();
            map<string, JSONObject*> *__getChilds();
    };


    class JSON
    {

        private:
            JSONObject *root = new JSONObject(NULL);
            
            void clear();

            
            JSONObject *find(string objectName, bool autoCreateTree, JSONObject *currentParent);

            void _set(string objectName, string value);

            void del(JSONObject *node);

            vector<string> getObjectsNames(JSONObject *currentItem = NULL);

            vector<string> getChildsNames(JSONObject *currentItem = NULL);

            vector<string> getJsonFields(string json);

            string clearJsonString(string json);

            bool isAJson(string json, bool objects = true, bool arrays = true);

        public:
            JSON();
            JSON(string JsonString);

            /// <summary>
            /// Removes an object from JSON three
            /// </summary>
            /// <param name="objectName">The object name</param>
            void del(string objectName);

            void clearChilds(string objectName);

            /// <summary>
            /// Set or creates an property with an json string
            /// </summary>
            /// <param name="objectName">The json object name</param>
            /// <param name="value">The json string </param>
            void set(string objectName, string value);

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
            string get(string objectName, bool format = false, bool quotesOnNames = true);

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


            void fromJson(string json);

            void fromString(string json);

            void parseJson(string json, string parentName = "");


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
            void setString(string name, string value);

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
            int getArrayLength(string objectName = "");

            void Dispose();
    };

    string getOnly(string source, string allowed);

    std::string ReplaceString(std::string subject, const std::string& search, const std::string& replace);

    map<string, JSONObject*>::const_iterator getChildByIndex (map<string, JSONObject*> *maptosearch, int index);

    // trim from start (in place)
    static inline void ltrim(std::string &s);

    // trim from end (in place)
    static inline void rtrim(std::string &s);

    // trim from both ends (in place)
    static inline void trim(std::string &s);

    // trim from start (copying)
    static inline std::string ltrim_copy(std::string s);

    // trim from end (copying)
    static inline std::string rtrim_copy(std::string s);

    // trim from both ends (copying)
    static inline std::string trim_copy(std::string s);

    string __unescapeString(string data);
}