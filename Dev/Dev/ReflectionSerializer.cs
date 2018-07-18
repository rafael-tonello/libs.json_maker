using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonMaker
{
    class ReflectionSerializer
    {
        #region Serialization

        /// <summary>
        /// Convert objects to serializer's notation. Only public properties with get; and set; will be converted (Example: string foo{get; set;} or string foo{get{return _foo;} set{_foo = value}}
        /// </summary>
        /// <param name="obj">The object to be serialized</param>
        /// <param name="serializer">The serializer to insert properties</param>
        /// <param name="maxLevel">How far the serializator can serialize inner properties</param>
        /// <returns></returns>
        public static JSON SerializeObject(Object obj, int maxLevel = int.MaxValue,bool insertPrimaryType = true)
        {
            return _SerializeObject(obj, insertPrimaryType, maxLevel, 1);

        }

        /// <summary>
        /// Refined 'SerializeObject' method
        /// </summary>
        /// <param name="obj"> The object to be serialized</param>
        /// <param name="serializer">The serializer to insert properties</param>
        /// <param name="insertType">Define whether a type description is necessary</param>
        /// <param name="maxLevel">How far the serializator can serialize inner properties</param>
        /// <param name="currLevel">Current property tree index,used for recursive calls</param>
        /// <returns></returns>
        private static JSON _SerializeObject(Object obj, bool insertType = true, int maxLevel = int.MaxValue, int currLevel = 1)
        {
            JSON serializer = new JSON();
            if (currLevel > maxLevel)
                return null;

            if (insertType)
            {
                _addToSerializer(serializer, "__SerializationType", obj.GetType().ToString(), maxLevel, currLevel);
            }

            //in dictionary and list cases, the value prop is not totally necessary,
            if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>) ||
                obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                _addToSerializer(serializer, "", obj, maxLevel, currLevel);
            }

            //primitive values must have an identification
            else if((obj is int) || (obj is double) || (obj is bool) || (obj is DateTime) || (obj is string))
            {
                _addToSerializer(serializer, "__SerializationValue", obj, maxLevel, currLevel);
            }
            else if (!(obj is ExpandoObject))
            {
                var teste = obj.GetType().GetMembers();
                foreach (var prop in teste)
                {
                    if (prop.MemberType == MemberTypes.Method)
                    {
                        if (prop.Name.StartsWith("get_"))
                        {
                            string propName = prop.Name.Substring(prop.Name.IndexOf('_') + 1);

                            try
                            {

                                object propValue = ((MethodInfo)prop).Invoke(obj, new object[] { });

                                //when the type is diferent from the prototype object type, a 'type' adn a 'value' is necessary
                                bool found;
                                if (propValue.GetType().ToString() != ReflectionExtensions.GetPropertyInfo(obj,propName.Split('.'),out found).PropertyType.ToString())
                                {
                                    _addToSerializer(serializer, propName + ".__SerializationType", propValue.GetType().ToString(), maxLevel, currLevel);
                                    _addToSerializer(serializer, propName + ".__SerializationValue", propValue, maxLevel, currLevel);
                                }
                                else
                                    _addToSerializer(serializer, propName, propValue, maxLevel, currLevel);
                            }
                            //well, didn't know what to do here
                            catch (Exception e) { string a = e.Message; }
                        }
                    }
                }
            }

            //expando values serialization
            else
            {
                IDictionary<string, object> objDic = ((IDictionary<string, object>)obj);
                foreach (var c in objDic)
                {
                    _addToSerializer(serializer, c.Key, c.Value, maxLevel, currLevel);
                }
            }
            return serializer;
        }

        /// <summary>
        /// Insert a property in the serializer
        /// </summary>
        /// <param name="serializer">The serializer object</param>
        /// <param name="propName">The name of the property to insert</param>
        /// <param name="propValue">The value of the property</param>
        /// <param name="maxLevel">How far the serializator can serialize inner properties</param>
        /// <param name="currLevel">Current property tree index,used for recursive calls</param>
        public static void _addToSerializer(JSON serializer, string propName, object propValue, int maxLevel = int.MaxValue, int currLevel = 1)
        {
            if (currLevel > maxLevel)
                return;

            //root property
            if (propName != "" && !propName.EndsWith("."))
                propName += ".";

            string typeStr = propValue.GetType().ToString();
            string innerTypeStr = "";

            //inner types string
            if (typeStr.Contains("["))
            {
                innerTypeStr = typeStr.Remove(0, typeStr.IndexOf('[') + 1);
                innerTypeStr = innerTypeStr.Substring(0, innerTypeStr.LastIndexOf(']'));
            }

            //most simple case,value is primitive
            if ((propValue is int) || (propValue is double) || (propValue is bool))
                serializer.setString(propName, propValue.ToString());

            //default datetime format
            else if (propValue is DateTime)
            {
                string timeZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString();
                timeZone = timeZone.Remove(timeZone.LastIndexOf(':'));
                if (timeZone[0] != '-')
                    timeZone = "+" + timeZone;

                serializer.setString(propName, ((DateTime)propValue).ToString("yyyy-MM-ddTHH:mm:ss" + timeZone));
            }

            //primitive vaue
            else if (propValue is string)
            {
                serializer.setString(propName, (string)propValue);
            }

            //dictionary handle
            else if (typeStr.Contains("System.Collections.Generic.Dictionary"))
            {
                //just to be shure that is dictionary
                if (propValue.GetType().GetMethod("get_Keys") == null) return;
                if (propValue.GetType().GetMethod("get_Item") == null) return;

                //get dictionary keys
                object keys = propValue.GetType().GetMethod("get_Keys").Invoke(propValue, new object[] { });
                //test if is really a dictionary item
                if (keys.GetType().GetMethod("GetEnumerator") == null) return;

                //get the dictionary enumerator to loop keys
                object enumerator = keys.GetType().GetMethod("GetEnumerator").Invoke(keys, new object[] { });
                if (enumerator.GetType().GetMethod("get_Current") == null) return;

                //get the string of keys and values types
                string keysType = innerTypeStr.Substring(0, innerTypeStr.IndexOf(','));
                string valuesType = innerTypeStr.Substring(innerTypeStr.IndexOf(',') + 1);

                int count = 0;
                //loop until serialize all keys and values
                while ((bool)enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, new object[] { }))
                {
                    object current = enumerator.GetType().GetMethod("get_Current").Invoke(enumerator, new object[] { });
                    object ret3 = propValue.GetType().GetMethod("get_Item").Invoke(propValue, new object[] { current });

                    //check if key is the same type of the specified key type
                    if (keysType != current.GetType().ToString())
                    {
                        _addToSerializer(serializer, "Items[" + count + "].Key.__SerializationType", current.GetType().ToString(), maxLevel, currLevel);
                        _addToSerializer(serializer, propName + "Items[" + count + "].Key.__SerializationValue", current, maxLevel, currLevel + 1);
                    }
                    else
                        _addToSerializer(serializer, propName + "Items[" + count + "].Key", current, maxLevel, currLevel + 1);

                    //check if value is the same type of the specified value type
                    if (valuesType != ret3.GetType().ToString())
                    {
                        _addToSerializer(serializer, "Items[" + count + "].Value.__SerializationType", ret3.GetType().ToString(), maxLevel, currLevel);
                        _addToSerializer(serializer, propName + "Items[" + count + "].Value.__SerializationValue", ret3, maxLevel, currLevel + 1);
                    }

                    else
                        _addToSerializer(serializer, propName + "Items[" + count + "].Value", ret3, maxLevel, currLevel + 1);
                    
                    count++;
                }
            }

            //list handle
            else if (typeStr.Contains("System.Collections.Generic.List"))
            {
                //just to be shure that is list
                if (propValue.GetType().GetMethod("get_Item") == null) return;
                if (propValue.GetType().GetMethod("get_Count") == null) return;

                //get list length
                int listCount = (int)propValue.GetType().GetMethod("get_Count").Invoke(propValue, new object[] { });

                //test if is enumerable
                if (propValue.GetType().GetMethod("GetEnumerator") == null) return;

                //loop until serialize all items
                for (int count = 0; count < listCount; count++)
                {
                    object ret3 = propValue.GetType().GetMethod("get_Item").Invoke(propValue, new object[] { count });

                    //if the item is't the same list type, then insert 'type' and 'value'
                    if (innerTypeStr != ret3.GetType().ToString())
                    {
                        _addToSerializer(serializer, "Items[" + count + "].__SerializationType", ret3.GetType().ToString(), maxLevel, currLevel);
                        _addToSerializer(serializer, propName + "Items[" + count + "].__SerializationValue", ret3, maxLevel, currLevel + 1);
                    }

                    else
                        _addToSerializer(serializer, propName + "Items[" + count + "]", ret3, maxLevel, currLevel + 1);
                }
            }

            //expando handle
            else if (propValue is ExpandoObject)
            {
                IDictionary<string, object> dic = ((IDictionary<string, object>)propValue);
                if (currLevel <= maxLevel)
                {
                    foreach (var currItem in dic)
                    {
                        _addToSerializer(serializer, propName + currItem.Key, currItem.Value, maxLevel, currLevel + 1);
                        //var serializedData = _SerializeObject(currItem.Value, (SerializerInterface)Activator.CreateInstance(serializer.GetType()),true, maxLevel, currLevel + 1);
                        //if (serializedData != null)
                        //    serializer.addProperty(propName + "." +currItem.Key , serializedData);
                        //else
                        //    serializer.addProperty(propName, "null");
                    }
                }
            }

            //system type handle
            else if (!typeStr.Contains("System.Collections.Generic"))
            {
                if (currLevel <= maxLevel)
                {
                    //serialize entire object
                    var serializedData = _SerializeObject(propValue, false, maxLevel, currLevel + 1);
                    if (propName != "" && propName.EndsWith("."))
                        propName = propName.Substring(0, propName.Length - 1);
                    serializer.set(propName, serializedData);
                }
            }
        }
        #endregion

        #region Unserialization
        /// <summary>
        /// Creates an object with the given type, if is primitive value, then the value is parsed
        /// </summary>
        /// <param name="value">The object string value</param>
        /// <param name="type">The object type</param>
        /// <returns></returns>
        private static object createObject(string value,string type)
        {
            object result = null;
            string innerTypeStr = "";

            if (type.Contains("["))
            {
                innerTypeStr = type.Remove(0, type.IndexOf('[') + 1);
                innerTypeStr = innerTypeStr.Substring(0, innerTypeStr.LastIndexOf(']'));
            }

            if (type == "System.Boolean")
                result = bool.Parse(value);
            else if (type == "System.Double")
                result = double.Parse(value);
            else if (type == "System.Int" || type == "System.Int32")
                result = int.Parse(value);
            else if (type.EndsWith("DateTime"))
                result = DateTime.Parse(value);
            else if (type == "System.String")
                result = value;

            else if (type.StartsWith("System.Collections.Generic.List"))
            {
                Type list = typeof(List<>).MakeGenericType(Type.GetType(innerTypeStr));
                result = list.GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            else if (type.StartsWith("System.Collections.Generic.Dictionary"))
            {
                string keysType = innerTypeStr.Substring(0, innerTypeStr.IndexOf(','));
                string valuesType = innerTypeStr.Substring(innerTypeStr.IndexOf(',') + 1);

                Type dicType = typeof(Dictionary<,>).MakeGenericType(new Type[] { Type.GetType(keysType), Type.GetType(valuesType) });
                result = dicType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            else
                result = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(type);

            return result;
        }

        /// <summary>
        /// Insert type and value int the propertyes of the serializer when it's necessary
        /// </summary>
        /// <param name="original">The original serializer</param>
        /// <param name="propName">The property name to search in the original serializer</param>
        /// <param name="type">The type of the property</param>
        /// <returns></returns>
        private static JSON NormalizeSerialization(JSON original, string propName,string type)
        {
            //rebuild key data notation
            JSON itemRebuilder = new JSON(original.get(propName));
            //SerializerInterface aux = (SerializerInterface)Activator.CreateInstance(original.GetType());
            if (original.getString(propName + ".__SerializationType") == "")
            {
                if(original.getChildsNames(propName).Count == 0)
                {
                    itemRebuilder.setString("__SerializationValue", original.getString(propName));
                    //aux.unSerialize(original.getProperty(propName));
                    //itemRebuilder.addProperty("Value", aux);
                }
                //else
                //{
                //    itemRebuilder.addProperty("Value", original.getProperty(propName));
                //}

                itemRebuilder.setString("__SerializationType", type);
            }
            //else
            //    itemRebuilder.unSerialize(original.getProperty(propName));

            return itemRebuilder;
        }

        /// <summary>
        /// Creates a system object where its properties are in the string formated to the serializer's notation
        /// </summary>
        /// <param name="data">String containing serialized data</param>
        /// <param name="serializer">Serializer that receives the string content</param>
        /// <param name="destObject">The object that will receive the serialized data</param>
        /// <param name="rebuildChildren">If the childrem properties are desired</param>
        /// <returns></returns>
        public static object DeSerializeObject(string data, object destObject = null, bool rebuildChildren = true)
        {
            JSON serializer = new JSON(data);
            return UnSerializeObject(serializer, destObject, rebuildChildren);
        }

        /// <summary>
        /// Creates a system object where its properties are in the serializer
        /// </summary>
        /// <param name="PopulatedSerializer">Filled Serializer</param>
        /// <param name="destObject">The object that will receive the serialized data</param>
        /// <param name="rebuildChildren">If the childrem properties are desired</param>
        /// <returns></returns>
        public static object UnSerializeObject(JSON PopulatedSerializer, object destObject = null, bool rebuildChildren = true)
        {
            Dictionary<string, MemberInfo> props = new Dictionary<string, MemberInfo>();

            //create a instance for the destination obj 
            object result = null;
            if (destObject != null)
                result = destObject;
            else if (PopulatedSerializer.contains("__SerializationType"))
                result = createObject(PopulatedSerializer.getString("__SerializationValue", ""),PopulatedSerializer.getString("__SerializationType"));

            if (result == null)
            {
                //if the assembly don't contais the current type, use an expando object
                result = new ExpandoObject();
            }

            string prefix = "";
            //when it has a value field the prefix is inserted
            if (PopulatedSerializer.contains("__SerializationValue"))
                prefix = "__SerializationValue.";

            //Try to create an expando with all properties in the serializer
            if (result is ExpandoObject)
            {
                List<string> propsInJson = PopulatedSerializer.getChildsNames("");
                var x = new ExpandoObject() as IDictionary<string, Object>;

                foreach (var c in propsInJson)
                {
                    object parsedValue = null;

                    //try find value
                    JSON typeFinder = new JSON(PopulatedSerializer.get(c));
                    int iv;
                    double dv;
                    bool bv;
                    DateTime dt;

                    if (typeFinder.getChildsNames().Count > 0)
                    {
                        try
                        {
                            parsedValue = UnSerializeObject(typeFinder, null, rebuildChildren);
                        }
                        catch { };
                    }
                    else if (int.TryParse(PopulatedSerializer.getString(c), out iv))
                        parsedValue = iv;
                    else if (double.TryParse(PopulatedSerializer.getString(c), out dv))
                        parsedValue = dv;
                    else if (bool.TryParse(PopulatedSerializer.getString(c), out bv))
                        parsedValue = bv;
                    else if (DateTime.TryParse(PopulatedSerializer.getString(c), out dt))
                        parsedValue = dt;
                    else
                        parsedValue = PopulatedSerializer.get(c);

                    x.Add(c, parsedValue);
                }

                result = x;
            }

            //List handling
            else if (result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                int cont = 0;
                string listItemsType = result.GetType().GetGenericArguments().Single().ToString();

                //search list items
                while (PopulatedSerializer.contains(prefix + "Items[" + cont + "]"))
                {
                    string itemName = prefix + "Items[" + cont + "]";
                    object currItem = UnSerializeObject(NormalizeSerialization(PopulatedSerializer, itemName, listItemsType), null, rebuildChildren);
                    result.GetType().GetMethod("Add").Invoke(result, new object[] { currItem });
                    cont++;
                }

                return result;
            }

            //Dictionary handling
            else if (result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                int cont = 0;
                string keysType = result.GetType().GetGenericArguments()[0].ToString();
                string valuesType = result.GetType().GetGenericArguments()[1].ToString();

                //search dictionary items
                while (PopulatedSerializer.contains(prefix + "Items[" + cont + "].Key"))
                {
                    string itemName = prefix + "Items[" + cont + "]";

                    //find dictionary key and value in serializer
                    object currKey = UnSerializeObject(NormalizeSerialization(PopulatedSerializer, itemName + ".Key", keysType), null, rebuildChildren);
                    object currValue = UnSerializeObject(NormalizeSerialization(PopulatedSerializer, itemName + ".Value", valuesType),null,rebuildChildren);

                    //add the item to Dictionary
                    result.GetType().GetMethod("Add").Invoke(result, new object[] { currKey, currValue });
                    cont++;
                }
            }

            //Internal type handling
            else
            {
                var selfMethods = result.GetType().GetMembers();
                
                //search methods looking for "set_"
                foreach (var c in selfMethods)
                {
                    if (c.Name.StartsWith("set_"))
                    {
                        string propName = c.Name.Substring(c.Name.IndexOf('_') + 1);
                        props.Add(propName, c);

                        //search property in serializer
                        object v = UnSerializeObject
                            (
                                NormalizeSerialization(
                                    PopulatedSerializer,
                                    prefix + propName,
                                    ((MethodInfo)c).GetParameters()[0].ParameterType.ToString()
                                    ),
                                null,
                                rebuildChildren
                            );

                        //add the found value to the result object
                        ((MethodInfo)c)?.Invoke(result, new object[] { v });
                    }
                }
            }


            return result;
        }   
    }

    #endregion

    public static class RuntimeTypeCheckExtensions
    {
        public static bool IsAssignableToAnyOf(this Type typeOperand, IEnumerable<Type> types)
        {
            return types.Any(type => type.IsAssignableFrom(typeOperand));
        }

        public static bool IsAssignableToAnyOf(this Type typeOperand, params Type[] types)
        {
            return IsAssignableToAnyOf(typeOperand, types.AsEnumerable());
        }

        public static bool IsAssignableToAnyOf<T1, T2, T3>(this Type typeOperand)
        {
            return typeOperand.IsAssignableToAnyOf(typeof(T1), typeof(T2), typeof(T3));
        }

        public static bool IsAssignableToAnyOf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(this Type typeOperand)
        {
            return typeOperand.IsAssignableToAnyOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16), typeof(T17), typeof(T18), typeof(T19), typeof(T20));
        }
    }

    public static class CompileTimeTypeCheckUtils
    {
        public static IsAssignableToAnyOfWrapper<T1, T2, T3> IsAssignableToAnyOf<T1, T2, T3>()
        {
            return new IsAssignableToAnyOfWrapper<T1, T2, T3>();
        }

        public static IsAssignableToAnyOfWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> IsAssignableToAnyOf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>()
        {
            return new IsAssignableToAnyOfWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>();
        }
    }

    public class IsAssignableToAnyOfWrapper<T1, T2, T3>
    {
        public void OperandToCheck(T1 operand) { }
        public void OperandToCheck(T2 operand) { }
        public void OperandToCheck(T3 operand) { }
    }

    public class IsAssignableToAnyOfWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>
    {

        public void OperandToCheck(T1 operand) { }
        public void OperandToCheck(T2 operand) { }
        public void OperandToCheck(T3 operand) { }
        public void OperandToCheck(T4 operand) { }
        public void OperandToCheck(T5 operand) { }
        public void OperandToCheck(T6 operand) { }
        public void OperandToCheck(T7 operand) { }
        public void OperandToCheck(T8 operand) { }
        public void OperandToCheck(T9 operand) { }
        public void OperandToCheck(T10 operand) { }
        public void OperandToCheck(T11 operand) { }
        public void OperandToCheck(T12 operand) { }
        public void OperandToCheck(T13 operand) { }
        public void OperandToCheck(T14 operand) { }
        public void OperandToCheck(T15 operand) { }
        public void OperandToCheck(T16 operand) { }
        public void OperandToCheck(T17 operand) { }
        public void OperandToCheck(T18 operand) { }
        public void OperandToCheck(T19 operand) { }
        public void OperandToCheck(T20 operand) { }
    }

    public static class ReflectionExtensions
    {
        public static IEnumerable<T> GetEnumerableOfType<T>(Type[] constructorTypes, object[] constructorArgs) where T : class
        {
            List<T> objects = new List<T>();

            foreach (Type type in
                Assembly.GetExecutingAssembly().GetTypes()
                .Where(myType => !myType.IsAbstract && !myType.IsInterface && (typeof(T).IsAssignableFrom(myType))))
            {
                objects.Add((T)type.GetConstructor(constructorTypes).Invoke(constructorArgs));
            }
            return objects;
        }

        public static IEnumerable<T> GetEnumerableOfType<T>(Type[] constructorTypes, object[] constructorArgs, string nameSpace) where T : class
        {
            List<T> objects = new List<T>();

            foreach (Type type in
                Assembly.GetExecutingAssembly().GetTypes()
                .Where(myType => myType.Namespace == nameSpace && !myType.IsAbstract && !myType.IsInterface && (typeof(T).IsAssignableFrom(myType))))
            {
                objects.Add((T)type.GetConstructor(constructorTypes).Invoke(constructorArgs));
            }
            return objects;
        }

        public static IEnumerable<Type> GetTypesOfNamespace<T>(string nameSpace) where T : class
        {
            List<Type> objects = new List<Type>();

            foreach (Type type in
                Assembly.GetExecutingAssembly().GetTypes()
                .Where(myType => myType.Namespace == nameSpace && !myType.IsAbstract && !myType.IsInterface && (typeof(T).IsAssignableFrom(myType))))
            {
                objects.Add(type);

            }
            return objects;
        }

        public static object getPropertyInObject(object o, string[] propertyThree, out bool propertyFound, bool useCase = false)
        {
            propertyFound = false;
            List<PropertyInfo> oProps = (o.GetType().GetProperties()).ToList();

            for (int i = 0; i < propertyThree.Length; i++)
            {
                PropertyInfo found = oProps.Find(prop => useCase ? prop.Name == propertyThree[i] : prop.Name.ToUpper() == propertyThree[i].ToUpper());
                if (found != null)
                {
                    var value = found.GetValue(o);
                    if (i == propertyThree.Length - 1)
                    {
                        propertyFound = true;
                        return value;
                    }
                    else
                    {
                        return getPropertyInObject(found.GetValue(o), propertyThree.Skip(i + 1).ToArray(), out propertyFound, useCase);
                    }
                }

            }

            return null;
        }

        public static PropertyInfo GetPropertyInfo(object o, string[] propertyThree, out bool propertyFound, bool useCase = false)
        {
            propertyFound = false;
            List<PropertyInfo> oProps = (o.GetType().GetProperties()).ToList();

            for (int i = 0; i < propertyThree.Length; i++)
            {
                PropertyInfo found = oProps.Find(prop => useCase ? prop.Name == propertyThree[i] : prop.Name.ToUpper() == propertyThree[i].ToUpper());
                if (found != null)
                {
                    if (i == propertyThree.Length - 1)
                    {
                        propertyFound = true;
                        return found;
                    }
                    else
                    {
                        return GetPropertyInfo(found.GetValue(o), propertyThree.Skip(i + 1).ToArray(), out propertyFound, useCase);
                    }
                }

            }

            return null;
        }

        public static void setPropertyInObject(object o, string[] propertyThree, object value, bool useCase = false)
        {
            List<PropertyInfo> oProps = (o.GetType().GetProperties()).ToList();

            for (int i = 0; i < propertyThree.Length; i++)
            {
                PropertyInfo found = oProps.Find(prop => useCase ? prop.Name == propertyThree[i] : prop.Name.ToUpper() == propertyThree[i].ToUpper());
                if (found != null)
                {
                    if (i == propertyThree.Length - 1)
                    {
                        found.SetValue(o, value);

                    }
                    else
                        setPropertyInObject(found.GetValue(o), propertyThree.Skip(i + 1).ToArray(), value, useCase);
                }

            }
        }
    }
}
