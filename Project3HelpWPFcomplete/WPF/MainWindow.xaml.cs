/////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs : WPF Client that uses Shim to call native C++ code  //
// ver 1.1                                                                 //
//                                                                         //
// Application    : CSE-687 C++/CLI WPF Client GUI                         //
// Platform       : Visual Studio 2017 Community Edition                   //
//                  Windows 10 Professional 64-bit, Acer Aspire R5-571TG   //
// Author         : Bohao Li (SUID:877573155) bli107@syr.edu               //
// Source         : Ammar Salman, EECS Department, Syracuse University     //
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
 *  ver 1.1 - April 12th, 2018
 *  modify the previous version of GUI
 *  and add WPF control to Tabs in GUI
 *  
 *  
 *  
 *  ver 1.0 - March 23rd, 2018
 *    - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using MsgPassingCommunication;
using Navigator;

namespace WPF
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
        public MainWindow()
        {
            InitializeComponent();
            Navigator.Environment.root = ClientEnvironment.root;
            fileMgr = FileMgrFactory.create(FileMgrType.Local);
            getTopFiles();
        }
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        private Stack<string> pathStack_ = new Stack<string>();
        private Translater translater;
        private CsEndPoint endPoint_;
        private Thread rcvThrd = null;
        private int machinePort = 8080;
        private string machineAddress = "localhost";
        private Dictionary<string, Action<CsMessage>> dispatcher_
          = new Dictionary<string, Action<CsMessage>>();

        //----< process incoming messages on child thread >----------------

        private void processMessages()
        {
            ThreadStart thrdProc = () => {
                while (true)
                {
                    CsMessage msg = translater.getMessage();
                    string msgId = msg.value("command");
                    if (dispatcher_.ContainsKey(msgId))
                        dispatcher_[msgId].Invoke(msg);
                }
            };
            rcvThrd = new Thread(thrdProc);
            rcvThrd.IsBackground = true;
            rcvThrd.Start();
        }
        //----< function dispatched by child thread to main thread >-------

        private void clearDirs()
        {
            DirList.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        private void addDir(string dir)
        {
            DirList.Items.Add(dir);
        }
        //----< function dispatched by child thread to main thread >-------

        private void insertParent()
        {
            DirList.Items.Insert(0, "..");
        }
        //----< function dispatched by child thread to main thread >-------

        private void clearFiles()
        {
            FileList.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        private void addFile(string file)
        {
            FileList.Items.Add(file);
        }
        //----< add client processing for message with key >---------------

        private void addClientProc(string key, Action<CsMessage> clientProc)
        {
            dispatcher_[key] = clientProc;
        }

        //----< Dispatcher: view file from Repo >---------------
        private void DispatcherLoadGetFileContent()
        {
            Action<CsMessage> getFileContent = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                int line = 1;
                StringBuilder fileBuilder = new StringBuilder();
                string line_str;
                bool flag = true;
                while (true)
                {
                    flag = true;
                    while (enumer.MoveNext())
                    {     
                        string key = enumer.Current.Key;
                        if (key.Equals("content line" + line))
                        {
                            line_str = enumer.Current.Value;
                            line++;
                            fileBuilder.AppendLine(line_str);
                            flag = false;
                            break;
                        }
                    }
                    enumer = rcvMsg.attributes.GetEnumerator();
                    if (flag)
                        break;
                }
                string content = fileBuilder.ToString();
                Action<string> doShow = (string fileContent) =>
                {
                    showFile(fileContent);
                };
                Dispatcher.Invoke(doShow, new Object[] { content });
            };
            addClientProc("getFileContent", getFileContent);
        }
        //----< load getDirs processing into dispatcher dictionary >-------

        private void DispatcherLoadGetDirs()
        {
            Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
            {
                Action clrDirs = () =>
                {
                    clearDirs();
                };
                Dispatcher.Invoke(clrDirs, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDir = (string dir) =>
                        {
                            addDir(dir);
                        };
                        Dispatcher.Invoke(doDir, new Object[] { enumer.Current.Value });
                    }
                }
                Action insertUp = () =>
                {
                    insertParent();
                };
                Dispatcher.Invoke(insertUp, new Object[] { });
            };
            addClientProc("getDirs", getDirs);
        }
        //----< load getFiles processing into dispatcher dictionary >------

        private void DispatcherLoadGetFiles()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    clearFiles();
                };
                Dispatcher.Invoke(clrFiles, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            addFile(file);
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("getFiles", getFiles);
        }
        //----< load all dispatcher processing >---------------------------

        private void loadDispatcher()
        {
            DispatcherLoadGetDirs();
            DispatcherLoadGetFiles();
            DispatcherLoadGetFileContent();
        }
        //----< start Comm, fill window display with dirs and files >------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // start Comm
            string[] commandPara = System.Environment.GetCommandLineArgs();
            endPoint_ = new CsEndPoint();
            if (commandPara.Length == 3)
            {
                endPoint_.machineAddress = commandPara[1];
                endPoint_.port = int.Parse(commandPara[2]);

            }
            
            
            translater = new Translater();
            translater.listen(endPoint_);

            // start processing messages
            processMessages();

            // load dispatcher
            loadDispatcher();

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;

            //PathTextBlock.Text = "Storage";
            pathStack_.Push("../Storage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
            /// test case executes only when port number is 8083
            if(endPoint_.port == 8083)
            {
                ThreadStart ts = test;
                Thread a = new Thread(ts);
                a.IsBackground = true;
                a.Start();
            }
            
        }

        //------< pop up window >----------------
        private void showFile(string fileContents)
        {
            try
            {
                CodePopup popup = new CodePopup();
                
                popup.codeView.Text = fileContents;
                popup.Show();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //----< strip off name of first part of path >---------------------

        private string removeFirstDir(string path)
        {
            string modifiedPath = path;
            int pos = path.IndexOf("/");
            modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
            return modifiedPath;
        }
        //----< respond to mouse double-click on dir name >----------------

        private void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // build path for selected dir
            string selectedDir = (string)DirList.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                if (pathStack_.Count > 1)  // don't pop off "Storage"
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }
            // display path in Dir TextBlcok
            //PathTextBlock.Text = removeFirstDir(pathStack_.Peek());

            // build message to get dirs and post it
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);

            // build message to get files and post it
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
            
        }
        //----< first test not completed >---------------------------------
        void test()
        {
            //System.Threading.Thread.Sleep(500);
            test1();
            System.Threading.Thread.Sleep(500);
            test2();
            System.Threading.Thread.Sleep(500);
            test3();
            System.Threading.Thread.Sleep(500);
            test4();
            System.Threading.Thread.Sleep(500);
            test5();
            System.Threading.Thread.Sleep(500);
            test6();
        }
        void test1()
        {
            Console.WriteLine("\n ===============Demo 1 connect================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "echo");
            translater.postMessage(msg);
        }
        void test2()
        {
            Console.WriteLine("\n ===============Demo 2 check in================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkIn");
            msg.add("package", "testpackage");
            msg.add("file1", "file1.c");
            msg.add("file2", "file2.c");
            msg.add("file3", "file3.c");
            translater.postMessage(msg);
            //statusBarText.Text = "Check In";
        }

        void test3()
        {
            Console.WriteLine("\n ===============Demo 3 check out================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkOut");
            msg.add("package", "testpackage");
            msg.add("file1", "file1.c.1");
            translater.postMessage(msg);
        }

        void test4()
        {
            Console.WriteLine("\n ===============Demo 4 browse================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "browse");
            msg.add("search condition", "package1");
        }

        void test5()
        {
            Console.WriteLine("\n ===============Demo 5 view remote file================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getFileContent");
            msg.add("path", "../storage/IComm.h");
            translater.postMessage(msg);
        }

        void test6()
        {
            Console.WriteLine("===============Demo 6 view metadata================");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "view metadata");
            msg.add("package name", "package1");
            msg.add("file name", "file1.c");
            translater.postMessage(msg);
            //statusBarText.Text = "View meta data";
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        //----- < check in tab add file >---------------------
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SelectedFiles.Items.Add(localFiles.SelectedItem.ToString());
        }

        //----- < check in tab delete file >---------------------
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFiles.SelectedItem != null)
            {
                SelectedFiles.Items.Remove(SelectedFiles.SelectedItem.ToString());
            }
        }

        //--------< check in tab submit>---------------------------------
        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkIn");
            translater.postMessage(msg);
            statusBarText.Text = "Check In";
        }

        //--------< check out tab submit>------------------------------
        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkOut");
            translater.postMessage(msg);
            statusBarText.Text = "Check Out";
        }

        //--------< local file up button>------------------------------
        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            foreach (string file in files)
            {
                localFiles.Items.Add(file);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }

        //--------< local file top button>------------------------------
        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }

        //--------< local dir double click>------------------------------
        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirName = localDirs.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
            statusBarText.Text = "Get Local Files";
        }

        //--------< local file double click>------------------------------
        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopup popup = new CodePopup();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            statusBarText.Text = "Get Local File Content";
        }

        //--------< remote file double click>------------------------------
        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = FileList.SelectedItem.ToString();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getFileContent");
            msg.add("path", path);
            translater.postMessage(msg);
            statusBarText.Text = "Get Remote File Content";
        }

        //--------< browse tab click>------------------------------
        private void Browse_click(object sender, RoutedEventArgs e)
        {
            string condition = ResultList.Text.ToString();
            statusBarText.Text = "Browse package";
            if (condition != "")
            {
                CsEndPoint serverEndPoint = new CsEndPoint();
                serverEndPoint.machineAddress = machineAddress;
                serverEndPoint.port = machinePort;
                CsMessage msg = new CsMessage();
                msg.add("to", CsEndPoint.toString(serverEndPoint));
                msg.add("from", CsEndPoint.toString(endPoint_));
                msg.add("command", "browse");
                msg.add("search condition", condition);
                translater.postMessage(msg);
            }
            else
            {
                MessageBox.Show("please input search condition.");
            }
            
        }

        //--------< view metadata click>------------------------------
        private void search_meta(object sender, RoutedEventArgs e)
        {
            statusBarText.Text = "Search meta data";
            string packageName = package_meta.Text.ToString();
            string fileName = file_meta.Text.ToString();
            if(packageName != "" && fileName != "")
            {
                CsEndPoint serverEndPoint = new CsEndPoint();
                serverEndPoint.machineAddress = machineAddress;
                serverEndPoint.port = machinePort;
                CsMessage msg = new CsMessage();
                msg.add("to", CsEndPoint.toString(serverEndPoint));
                msg.add("from", CsEndPoint.toString(endPoint_));
                msg.add("command", "view metadata");
                msg.add("package name", packageName);
                msg.add("file name", fileName);
                translater.postMessage(msg);
            }
            else
            {
                MessageBox.Show("input is not complete");
            }
        }

        //--------< connect server click>------------------------------
        private void connect_server(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = machineAddress;
            serverEndPoint.port = machinePort;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "echo");
            translater.postMessage(msg);
        }
    }
}
