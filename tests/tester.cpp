//compile with g++ -std=c++14 -o saida tester.cpp
#include <iostream>
#include <string>
#include <JSON.h>

using namespace std;
using namespace JsonMaker;

string readFile(string fName);
void test1();
void test2();
void test3();

int main()
{
    test1();
    test2();
    test3();
    return 0;
}

void test1()
{
    JSON *js = new JSON();
    js->setString("mas.em.0", "heheheh");
    js->setString("mas.em.0", "huhu");
    js->setString("mas.em.1", "suahsuahsuhas");

    cout << js->ToJson(true);
    cout << endl;
    cout << js->ToJson(false);


    cout << endl << flush;
}

void test2()
{
    string content = readFile("commented.json");

    JSON js(content);

    cout << js.ToJson(true);
    cout << endl << flush;

}

void test3()
{
    //set datetime in a string
    JSON js;
    string dateTime = "0102-03-04 05:06:07";
    js.setString("datetime", dateTime, true);

    if(dateTime == js.getString("datetime"))
        cout << "datetime setted with setString(force type detection) is working well" << endl;
    else
        cout << "datetime setted with setString(force type detection) is not working" << endl;
}

string readFile(string fName)
{
    FILE* f;
    f = fopen(fName.c_str(), "r");
    fseek(f, 0, SEEK_END);
    size_t s = ftell(f);
    fseek(f, 0, SEEK_SET);
    char* buffer = new char[s];
    fread(buffer, s, 1, f);
    fclose(f);
    string ret(buffer);
    delete[] buffer;
    return ret;
}

