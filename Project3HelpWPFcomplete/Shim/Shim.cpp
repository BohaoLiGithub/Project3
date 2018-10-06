/////////////////////////////////////////////////////////////////////////////
// Shim.cpp       : provides definitions and test stub for Shim.h          //
// ver 1.0                                                                 //
//                                                                         //
// Application    : CSE-687 C++/CLI Shim demostration                      //
// Platform       : Visual Studio 2017 Community Edition                   //
//                  Windows 10 Professional 64-bit, Acer Aspire R5-571TG   //
// Author         : Ammar Salman, EECS Department, Syracuse University     //
//                  313/788-4694, hoplite.90@hotmail.com assalman@syr.edu  //
/////////////////////////////////////////////////////////////////////////////

#include "Shim.h"

// -----< Shim constructor - instantiates ICppClass >------------------
Shim::Shim() {
  Console::Write("\n  Shim created");
  ObjectFactory factory;
  // use factory to get instance of CppClient
  client = factory.createClient();
  Console::Write("\n  Shim instantiated CppClient as ICppClient*");
}


// -----< Shim destructor - deletes ICppClass >------------------------
Shim::~Shim() {
  delete client;
  Console::Write("\n  Shim destroyed\n\n");
}

// -----< ICppClass::Start() wrapper >---------------------------------
void Shim::Start()
{
  client->Start();
}

// -----< ICppClass::PostMessage(const std::string&) wrapper >---------
void Shim::PostMessage(String^ msg) {
  // send converted string to CppClient
  client->PostMessage(sysStrToStdStr(msg));
}

// -----< ICppClass::GetMessage() wrapper >----------------------------
String^ Shim::GetMessage() {
  // CppClient returns std::string, convert it and send it to C# client
  return stdStrToSysStr(client->GetMessage());
}

//----< convert std::string to System.String >-------------------------
String^ Shim::stdStrToSysStr(const std::string& str) {
  return gcnew String(str.c_str());
}

//----< convert System.String to std::string >-------------------------
std::string Shim::sysStrToStdStr(String^ str) {
  std::string temp;
  for (int i = 0; i < str->Length; ++i)
    temp += char(str[i]);
  return temp;
}


// -----< Shim Test Stub >---------------------------------------------
#ifdef TEST_SHIM

int main(array<System::String^> ^args) {
  Shim localShim;  // C++ style construction
  // Shim^ shim = gcnew Shim(); // managed heap allocation and construction
  localShim.Start();

  Console::Write("\n\n  Shim posting 10 messages to CppClass:");
  Console::Write("\n =======================================\n");
  for (int i = 0; i < 10; i++)
    localShim.PostMessage("Message from C++/CLI Shim Test Stub");

  // sleep for 100ms to allow better formatted output
  System::Threading::Thread::Sleep(100);

  Console::Write("\n\n  Shim getting 10 messages from CppClass:");
  Console::Write("\n =========================================\n");
  for (int i = 0; i < 10; i++)
    Console::Write("\n  Shim got message from CppClass: {0}", localShim.GetMessage());

  Console::Write("\n\n  Finished demonstration.");
  Console::Write("\n  Shim dtor is automatically called after 'main' goes out of scope.\n");

  return 0;
} // NOTE: Shim destructor will automatically be called here just like in native C++

#endif