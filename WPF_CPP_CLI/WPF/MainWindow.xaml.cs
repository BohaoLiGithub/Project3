/////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs : WPF Client that uses Shim to call native C++ code  //
// ver 1.0                                                                 //
//                                                                         //
// Application    : CSE-687 C++/CLI Shim demostration                      //
// Platform       : Visual Studio 2017 Community Edition                   //
//                  Windows 10 Professional 64-bit, Acer Aspire R5-571TG   //
// Author         : Ammar Salman, EECS Department, Syracuse University     //
//                  313/788-4694, hoplite.90@hotmail.com assalman@syr.edu  //
/////////////////////////////////////////////////////////////////////////////
/*
 *  Package Decription:
 * =====================
 *  This package defines the GUI using MainWindow.xaml file which is compiled
 *  to a working GUI saving you the effort of programatically creating GUIs.
 *  This package depends on Shim project, if Shim project is NOT built then
 *  Shim.dll does not exist and therefore you'll get Intellisense error saying
 *  you have missing reference. Do not worry about the error, once Shim project
 *  is successfully built this will build and run.
 *  
 *  Public Interface: N/A
 *  
 *  Required Files:
 * =================
 *  MainWindow.xaml MainWindow.xaml.cs Shim.dll
 *  
 *  Build Process:
 * ================
 *  msbuild WPF.csproj
 *  
 *  Maintainence History:
 * =======================
 *  ver 1.0 - March 23rd, 2018
 *    - first release
 */

using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace WPF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    Shim shim;  // this Shim can only be called if: 1) reference to Shim exists 2) Shim is built as DLL
    Thread workerThread;  // pulls messages from shim and updates the GUI

    // -----< MainWindow constructor - Do not modify >---------------------------
    public MainWindow()
    {
      InitializeComponent();
    }

    // -----< Window loaded event handler - Add your initialization here >-------
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      shim = new Shim(); // construct the shim
      shim.Start();      // start the CppClass 

      // setup C# worker thread to start working
      workerThread = new Thread(() =>
      {
        ReceivingThreadProc();
      });
      workerThread.Start();

      tabControl.SelectedIndex = 6;  // automatically go to the last tab
    }

    // -----< Worker thread behavior definition >----------------------------------
    private void ReceivingThreadProc()
    {
      while (true)
      {
        try
        {
          string msg = shim.GetMessage();
          Dispatcher.Invoke(() =>
          {
            lstBoxFilesForCheckin.Items.Insert(0, msg);
            statusBarText.Text = "Received " + lstBoxFilesForCheckin.Items.Count + " messages";
          });
        }
        catch { }
      }
    }

    // -----< Send button click event handler >-----------------------------------
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      shim.PostMessage(txtMsg.Text);  // post message to CppClass - see Console for output
      txtMsg.Text = "";
    }

    // -----< key down event handler >--------------------------------------------
    // performs Send button click when Enter is pressed
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        Button_Click(this, null);
    }

    // -----< Window closed event handler - finialize your stuff here >-----------
    private void Window_Closed(object sender, EventArgs e)
    {
      try
      {
        workerThread.Abort();
      }
      catch { }
      Console.Write("\n\n");
    }
  }
}
