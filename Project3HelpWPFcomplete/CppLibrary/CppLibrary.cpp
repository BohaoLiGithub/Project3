/////////////////////////////////////////////////////////////////////////////
// ICppLibrary.cpp  : defines a worker CppClass implementing ICppClass     //
// ver 1.0                                                                 //
//                                                                         //
// Application    : CSE-687 C++/CLI Shim demostration                      //
// Platform       : Visual Studio 2017 Community Edition                   //
//                  Windows 10 Professional 64-bit, Acer Aspire R5-571TG   //
// Author         : Ammar Salman, EECS Department, Syracuse University     //
//                  313/788-4694, hoplite.90@hotmail.com assalman@syr.edu  //
/////////////////////////////////////////////////////////////////////////////

#define IN_DLL

#include "ICppLibrary.h"

#include <thread>  // worker threads
#include "../Cpp11-BlockingQueue/Cpp11-BlockingQueue.h"  // for received and created messages

#include <iostream>  // std::cout 
#include <chrono>    // std::this_thread::sleep_for(100ms) - chrono_literals


// CppClasss - this is the class we wish to use in the WPF application
class CppClass : public ICppClass {
public:
  CppClass();
  ~CppClass() override;

  // Sets-up the threads to start working
  void Start() override;
  // Posts message into the received messages queue
  void PostMessage(const std::string& msg) override;
  // Returns message from the created messages queue
  std::string GetMessage() override;

private:
  std::thread printingThread;   // prints any received messages to the console
  std::thread msgCreatorThread; // creates a message every 800 milliseconds
  BlockingQueue<std::string> receivedMessages;  // contains received messages
  BlockingQueue<std::string> createdMessages;   // contains created messages 

  void printThreadProc();  // defines printing thread behavior 
  void creatThreadProc();  // defines msg creator thread behavior
};

// -----< constructor for CppClass >----------------------------------
CppClass::CppClass(){
  std::cout << "\n  CppClass instance created. ";
}

// -----< destructor for CppClass >-----------------------------------
CppClass::~CppClass()
{
  std::cout << "\n  Detaching CppClass worker threads";
  printingThread.detach();
  msgCreatorThread.detach();
  std::cout << "\n  CppClass instance destroyed";
}

// -----< Start class - starts worker threads >-----------------------
void CppClass::Start()
{
  std::cout << "\n  Starting CppClass worker threads";
  printingThread = std::thread(&CppClass::printThreadProc, this);
  msgCreatorThread = std::thread(&CppClass::creatThreadProc, this);
}

// -----< Post message into received messages queue >----------------- 
void CppClass::PostMessage(const std::string & msg)
{
  receivedMessages.enQ(msg);
}

// -----< Return message from creates messages queue >----------------
std::string CppClass::GetMessage()
{
  return createdMessages.deQ();
}


// -----< Printing thread behavior definition >-----------------------
void CppClass::printThreadProc()
{
  while (true) {
    std::string msg = receivedMessages.deQ();
    std::cout << "\n  CppClass received a message: {" << msg << "} ";
  }
}

// -----< Message creation thread behavior definition >---------------
void CppClass::creatThreadProc()
{
  using namespace std::chrono_literals;
  int counter = 0;
  while (true) {
    std::this_thread::sleep_for(800ms);
    createdMessages.enQ("Cpp worker thread created a new message. #" + std::to_string(counter++));
  }
}

// -----< Factory for returning CppClass as ICppClass* >--------------
ICppClass* ObjectFactory::createClient() {
  return new CppClass;
}


// -----< Test Stub >-------------------------------------------------
#ifdef TEST_CPPLIBRARY

int main() {
  ObjectFactory factory;
  ICppClass* instance = factory.createClient();
  instance->Start();
  std::cout << "\n\n  Test Stub Posting 10 messages though ICppClass*:";
  std::cout << "\n ==================================================\n";
  
  for (int i = 0; i < 10; i++)
    instance->PostMessage("Message from Test Stub - " + std::to_string(i));
  
  // sleep for 100 milliseconds to allow printingThread to print
  // all messages to the screen to have organized output
  using namespace std::chrono_literals;
  std::this_thread::sleep_for(100ms);

  std::cout << "\n\n  Test Stub getting 10 messages from ICppClass*:";
  std::cout << "\n ================================================\n";
  for (int i = 0; i < 10; i++)
    std::cout << "\n  Message from ICppClass*: " << instance->GetMessage();

  std::cout << "\n\n  Finished demonstration. Deleting ICppClass*";
  delete instance;

  std::cout << "\n\n";
  return 0;
}

#endif
