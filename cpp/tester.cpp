#include <iostream>
#include <string>
#include "JSON.h"

using namespace std;
using namespace JsonMaker;
int main()
{
    JSON *js = new JSON();
    js->setString("mas.em.0", "heheheh");
    js->setString("mas.em.0", "huhu");
    js->setString("mas.em.1", "suahsuahsuhas");

    cout << js->ToJson(true);


    cout << endl << flush;



}

#include "JSON.cpp"