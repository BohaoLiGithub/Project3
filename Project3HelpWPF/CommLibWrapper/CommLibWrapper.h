#pragma once

#include "../CppCommWithFileXfer/MsgPassingComm/IComm.h"

#ifdef IN_DLL
#define DLL_DECL __declspec(dllexport)
#else
#define DLL_DECL __declspec(dllimport)
#endif

namespace MsgPassingCommunication
{
  extern "C" {
    struct CommFactory {
      static DLL_DECL IComm* create(const std::string& machineAddress, size_t port);
    };
  }
}