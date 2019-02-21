# libs.json_maker

A C++ and C# library to create JSON using javascript notation:

## C#
`JSON jm = new JSON();`<br/>
`jm.setString("MyObject.Child.Child2.property", "example");`<br/>
`Console.WriteLine(jm.ToString());`<br/>
<br/>

## C++ <span color="red">the c ++ version is out of date but will be updated soon</span>
`JSON *jm = new JSON();`<br/>
`jm->setString("MyObject.Child.Child2.property", "example");`<br/>
`cout << jm.ToString());`<br/>







