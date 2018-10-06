#pragma once
// IComm.h

#include <string>
#include "../Message/Message.h"

namespace MsgPassingCommunication
{
  class IComm
  {
  public:
    static IComm* create(const std::string& machineAddress, size_t port);
    virtual void start() = 0;
    virtual void stop() = 0;
    virtual void postMessage(Message msg) = 0;
    virtual Message getMessage() = 0;
    virtual std::string name() = 0;
    virtual ~IComm() {}
  };
}