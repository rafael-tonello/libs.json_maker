#include <iostream>
#include <string>
#include "JSON.h"

using namespace std;
using namespace JsonMaker;
int main()
{
    JSON *js = new JSON();
    js->setString("mas.em.que.legal", "heheheh");

    cout << js->ToJson();



}

#include "JSON.cpp"