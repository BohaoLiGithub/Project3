#pragma once
// Project > Properties > C/C++ > Language > Conformance mode set to No
// Add references to System, System.Core

#include <string>
#include "../CppCommWithFileXfer/MsgPassingComm/IComm.h"
#include "CsMessage.h"
#include "../CommLibWrapper/CommLibWrapper.h"

using namespace System;
using namespace System::Text;

namespace MsgPassingCommunication
{
  public ref class Translater
  {
  public:
    Translater();
    void listen(CsEndPoint^ ep);
    void postMessage(CsMessage^ msg);
    CsMessage^ getMessage();
    CsMessage^ fromMessage(Message& msg);
    Message fromCsMessage(CsMessage^ csMsg);
  private:
    CsEndPoint^ ep_;
    MsgPassingCommunication::CommFactory* pFactory;
    MsgPassingCommunication::IComm* pComm;
  };

  Translater::Translater()
  {
    ep_ = gcnew CsEndPoint;
  }
  void Translater::listen(CsEndPoint^ ep)
  {
    std::cout << "\n  using CommFactory to create instance of Comm on native heap";
    ep_->machineAddress = ep->machineAddress;
    ep_->port = ep->port;
    pFactory = new CommFactory;
    pComm = pFactory->create(Sutils::MtoNstr(ep_->machineAddress), ep_->port);
    //IComm* pComm = MsgPassingCommunication::IComm::create(Sutils::MtoNstr(ep_->machineAddress), ep_->port);
    //delete pFactory;
  }

  CsMessage^ Translater::fromMessage(Message& msg)
  {
    CsMessage^ csMsg = gcnew CsMessage;
    Message::Attributes attribs = msg.attributes();
    for (auto attrib : attribs)
    {
      csMsg->add(Sutils::NtoMstr(attrib.first), Sutils::NtoMstr(attrib.second));
    }
    return csMsg;
  }

  Message Translater::fromCsMessage(CsMessage^ csMsg)
  {
    Message msg;
    auto enumer = csMsg->attributes->GetEnumerator();
    while (enumer.MoveNext())
    {
      String^ key = enumer.Current.Key;
      String^ value = enumer.Current.Value;
      msg.attribute(Sutils::MtoNstr(key), Sutils::MtoNstr(value));
    }
    return msg;
  }

  inline void Translater::postMessage(CsMessage^ csMsg)
  {
    std::cout << "\n  posting message in Translater";
    Message msg = this->fromCsMessage(csMsg);
    pComm->postMessage(msg);
  }

  inline CsMessage^ Translater::getMessage()
  {
    std::cout << "\n  getting message in Translater";
    Message msg = pComm->getMessage();
    return fromMessage(msg);
  }
}