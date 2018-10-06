///////////////////////////////////////////////////////////////////////
// ServerPrototype.h - Console App that processes incoming messages  //
// ver 1.0                                                           //
// Author: Bohao Li (SUID:877573155) bli107@syr.edu                  //
// Source: Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018 //
///////////////////////////////////////////////////////////////////////

/*
*  Package Operations:
* ---------------------
*  Package contains two structs, Environment and ClientEnvironment
*  
*  Maintenance History:
*  ver 1.0 : 4/12/2018
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
  public struct Environment
  {
    public static string root { get; set; }
    public const long blockSize = 1024;
    public static bool verbose { get; set; }
  }

  public struct ClientEnvironment
  {
    public static string root { get; set; } = "../../../Localfiles/";
    public const long blockSize = 1024;
    public static bool verbose { get; set; } = false;
  }

}
