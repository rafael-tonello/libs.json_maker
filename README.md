# JsonMaker (A self contained and easy to use JSON library)

JsonMaker is a C++ self-contained library to make working with JSON easier.

Each instance of this library represents a JSON object. This instance can do many things, such as parsing a string with json, exporting its data to a string, and allowing you to set, change, and delete any JSON property.
To access JSON object properties, you must use object notation names (such as "root.othernode.myproperty").

```c++
    JSON *jm = new JSON();`<br/>
    jm->setString("MyObject.Child.Child2.property", "example");`<br/>
    cout << jm.ToString());`<br/>
```


# Tasks lists

    



