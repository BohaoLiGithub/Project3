/////////////////////////////////////////////////////////////////////////////
// Shim.h  : defines a warpper for ICppClass which works with .NET code    //
// ver 1.0                                                                 //
//                                                                         //
// Application    : CSE-687 C++/CLI Shim demostration                      //
// Platform       : Visual Studio 2017 Community Edition                   //
//                  Windows 10 Professional 64-bit, Acer Aspire R5-571TG   //
// Author         : Ammar Salman, EECS Department, Syracuse University     //
//                  313/788-4694, hoplite.90@hotmail.com assalman@syr.edu  //
/////////////////////////////////////////////////////////////////////////////
/*
*  Package Description:
* ======================
*  This package defines Shim class which can be used by .NET applications.
*  In this demo, Shim is used in WPF project to allow making calls from
*  WPF C# code to CppClass native C++ code. The Shim is a mangaged (.NET) 
*  wrapper around ICppClass interface which can make calls to native C++
*  classes and, in the same time, can be called by any .NET code (C# here).
*
*  Public Interface:
* ===================
*  Shim localShim;
*  localShim.Start();
*  localShim.PostMessage("my message");
*  String^ msg = localShim.GetMessage();
*
*  Required Files:
* =================
*  Shim.h Shim.cpp ICppLibrary.h
*
*  Build Command:
* ================
*  msbuild Shim.vcxproj
*
*  Maintainence History:
* =======================
*  ver 1.0 - March 23rd 2018
*    - first release
*
*/

#pragma once

#include <string>
#include "../CppLibrary/ICppLibrary.h"

using namespace System;

// 'public ref' identifier tells the compiler this is
// a managed class, not native C++ class. this class
// has to be managed for the C# code in WPF to be able
// to use it. You CAN define native classes here but in
// this demo there is no point to do so. 
public ref class Shim {
public:
  Shim();
  ~Shim();
  // Wrapper around 'void ICppClass::Start()'
  void Start();
  // Wrapper around 'void ICppClass::PostMessage(const std::string* msg)'
  void PostMessage(String^ msg); // takes System::String and converts it to std::string internally
  // Wrapper around 'std::string ICppClass::GetMessage()'
  String^ GetMessage();  // converts std::string to System::String before returning it

private:
  // convert System::String to std::string
  std::string sysStrToStdStr(String^ str);
  // convert std::string to System::String
  String^ stdStrToSysStr(const std::string& str);
  // this will point to CppClass when instantiated through the ObjectFactory
  ICppClass* client;
};
