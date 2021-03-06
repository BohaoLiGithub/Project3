// CommLibWrapper.cpp : Defines the exported functions for the DLL application.
//

#define IN_DLL
#include "CommLibWrapper.h"
#include "../CppCommWithFileXfer/MsgPassingComm/Comm.h"

using namespace MsgPassingCommunication;

DLL_DECL IComm* CommFactory::create(const std::string& machineAddress, size_t port)
{
  ::Beep(1000, 100);
  std::cout << "\n  using CommFactory to invoke IComm::create";
  return IComm::create(machineAddress, port);
}


