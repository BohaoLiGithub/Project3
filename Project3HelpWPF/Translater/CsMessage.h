#pragma once
// CsMessage.h
#include <string>
#include <iostream>

using namespace System;
using namespace System::Text;
using namespace System::Collections::Generic;

namespace MsgPassingCommunication
{
  using Key = String;
  using Value = String;
  using Attributes = Dictionary<String^, String^>;

  public ref class Sutils
  {
  public:
    static std::string MtoNstr(String^ s);
    static String^ NtoMstr(const std::string& s);
  };

  std::string Sutils::MtoNstr(System::String^ str)
  {
    std::string temp;
    for (int i = 0; i < str->Length; ++i)
    {
      temp.push_back((char)str[i]);
    }
    return temp;
  }

  String^ Sutils::NtoMstr(const std::string& str)
  {
    StringBuilder^ sb = gcnew StringBuilder;
    for (auto ch : str)
    {
      sb->Append((wchar_t)ch);
    }
    return sb->ToString();
  }

  public ref class CsEndPoint
  {
  public:
    CsEndPoint()
    {
      machineAddress = gcnew String("");
      port = 0;
    }
    property String^ machineAddress;
    property int port;
    static String^ toString(CsEndPoint^ ep);
    static CsEndPoint^ fromString(String^ epStr);
  };

  String^ CsEndPoint::toString(CsEndPoint^ ep)
  {
    String^ epStr = ep->machineAddress + ":" + ep->port.ToString();
    return epStr;
  }

  CsEndPoint^ CsEndPoint::fromString(String^ epStr)
  {
    CsEndPoint^ ep = gcnew CsEndPoint;
    int pos = epStr->IndexOf(":");
    ep->machineAddress = epStr->Substring(0, pos);
    String^ portStr = epStr->Substring(pos + 1, epStr->Length - pos - 1);
    ep->port = System::Convert::ToInt32(portStr);
    return ep;
  }

  public ref class CsMessage
  {
  public:
    CsMessage() 
    {
      attributes = gcnew Attributes;
    }
    CsMessage(CsEndPoint to, CsEndPoint from)
    {
      attributes = gcnew Attributes;
      attributes->Add("to", to.machineAddress + ":" + to.port.ToString());
      attributes->Add("from", from.machineAddress + ":" + from.port.ToString());
    }
    void add(Key^ key, Value^ value)
    {
      attributes->Add(key, value);
    }
    Value^ value(Key^ key)
    {
      return attributes[key];
    }
    property Attributes^ attributes;

    void show()
    {
      Console::Write("\n  CsMessage:");
      auto enumer = attributes->GetEnumerator();
      while (enumer.MoveNext())
      {
        String^ key = enumer.Current.Key;
        String^ value = enumer.Current.Value;
        std::cout << "\n  { " << Sutils::MtoNstr(key) << ", " << Sutils::MtoNstr(value) << " }";
      }
      Console::Write("\n");
    }
  };
}