# libs.json_maker

A C++ and C# library to create JSON using javascript notation:

## C#
`JSON jm = new JSON();`
`jm.setString("MyObject.Child.Child2.property", "example");`
`Console.WriteLine(jm.ToString());`


## C++
`JSON *jm = new JSON();`
`jm->setString("MyObject.Child.Child2.property", "example");`
`cout << jm.ToString());`







