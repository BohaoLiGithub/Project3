/////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages  //
// ver 1.1                                   
// Author: Bohao Li (SUID: 877573155)                                  //
// Source: Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018   //
/////////////////////////////////////////////////////////////////////////

#include "ServerPrototype.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include <chrono>

namespace MsgPassComm = MsgPassingCommunication;

using namespace Repository;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;

Files Server::getFiles(const Repository::SearchPath& path)
{
  return Directory::getFiles(path);
}

Dirs Server::getDirs(const Repository::SearchPath& path)
{
  return Directory::getDirectories(path);
}

template<typename T>
void show(const T& t, const std::string& msg)
{
  std::cout << "\n  " << msg.c_str();
  for (auto item : t)
  {
    std::cout << "\n    " << item.c_str();
  }
}

std::function<Msg(Msg)> echo = [](Msg msg) {
  Msg reply = msg;
  reply.to(msg.from());
  reply.from(msg.to());
  return reply;
};

std::function<Msg(Msg)> getFiles = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getFiles");
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files files = Server::getFiles(searchPath);
    size_t count = 0;
    for (auto item : files)
    {
      std::string countStr = Utilities::Converter<size_t>::toString(++count);
      reply.attribute("file" + countStr, path+"/"+item);
    }
  }
  else
  {
    std::cout << "\n  getFiles message did not define a path attribute";
  }
  return reply;
};

//------< check in operation in dispatcher >----------------------------
std::function<Msg(Msg)> checkIn = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("checkIn");
	// To do in proj4

	return reply;
};

//------< check out operation in dispatcher >----------------------------
std::function<Msg(Msg)> checkOut = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("checkOut");
	//To do in proj4

	return reply;
};

//------< browse operation in dispatcher >----------------------------
std::function<Msg(Msg)> Browse = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("browse");
	//To do in proj4
	return reply;
};

//------< view Metadata in operation in dispatcher >----------------------------
std::function<Msg(Msg)> viewMetadata = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("view metadata");
	//To do in proj4
	return reply;
};

//------< view files in Ropo operation in dispatcher >----------------------------
std::function<Msg(Msg)> getFileContent = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("getFileContent");
	/*if (msg.containsKey("fileRequest")) {
		std::string key = "fileRequest";
		std::string path = msg.value(key);
		reply.name("a.txt");
		reply.file(path);
	}*/
	std::string path = msg.value("path");
	if (path != "") {
		//std::string searchPath = storageRoot;
		std::string fileContent;
		if (path != ".") {
			//searchPath = searchPath + "\\" + path;
			/*FileSystem::File file(path);
			file.open(FileSystem::File::in);
			if (file.isGood())
			{
				std::string fileContent = file.readAll(false);
				reply.attribute("file content" , fileContent);
			}
			else {
				std::cout << "\n --Alert: Open file error ";
			}*/
			std::ifstream in(path);
			std::string line;
			if (in) {
				int i = 0;
				while (getline(in, line)) {
					std::string countStr = Utilities::Converter<size_t>::toString(++i);
					reply.attribute("content line"+countStr, line);
				}
			}
		}
	}
	return reply;
};
std::function<Msg(Msg)> getDirs = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getDirs");
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files dirs = Server::getDirs(searchPath);
    size_t count = 0;
    for (auto item : dirs)
    {
      if (item != ".." && item != ".")
      {
        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("dir" + countStr, item);
      }
    }
  }
  else
  {
    std::cout << "\n  getDirs message did not define a path attribute";
  }
  return reply;
};

//----< start server>----------
int main()
{
  std::cout << "\n  Testing Server Prototype";
  std::cout << "\n ==========================";
  std::cout << "\n";

  //StaticLogger<1>::attach(&std::cout);
  //StaticLogger<1>::start();

  Server server(serverEndPoint, "ServerPrototype");
  server.start();

  std::cout << "\n  testing getFiles and getDirs methods";
  std::cout << "\n --------------------------------------";
  Files files = server.getFiles();
  show(files, "Files:");
  Dirs dirs = server.getDirs();
  show(dirs, "Dirs:");
  std::cout << "\n";

  std::cout << "\n  testing message processing";
  std::cout << "\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("getFileContent", getFileContent);
  server.addMsgProc("checkIn", checkIn);
  server.addMsgProc("checkOut", checkOut);
  server.addMsgProc("browse", Browse);
  server.addMsgProc("view metadata", viewMetadata);
  server.processMessages();
  
  Msg msg(serverEndPoint, serverEndPoint);  // send to self
  msg.name("msgToSelf");
  msg.command("echo");
  msg.attribute("verbose", "show me");
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));

  /*msg.command("getFiles");
  msg.remove("verbose");
  msg.attributes()["path"] = storageRoot;
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));

  msg.command("getDirs");
  msg.attributes()["path"] = storageRoot;
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));*/

  std::cout << "\n  press enter to exit";
  std::cin.get();
  std::cout << "\n";

  msg.command("serverQuit");
  server.postMessage(msg);
  server.stop();
  return 0;
}

