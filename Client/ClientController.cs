/////////////////////////////////////////////////////////////////////
// ClientController.cs - Test Harness                              //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Client                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the interface to communicate with proxy and 
 * to handle other critical functions. This is used by the xaml.cs files.
 * 
 * Public Interface:
 * =================
 * ClientController cc = new ClientController();
 * OpenChannel();
 * GetLogFile(string test_suite_name, string userName);
 * POstMessage(Object Messgae);
 * public bool Login(string username, string pass)
 * public bool Register(string username, string pass)
 * public string makeXmlMessgae(List<string> files, string prjName, string username)
 * public void StartTesting();
 * public void sendFiles(List<string> files);
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   Proxy.cs
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.Collections;
using System.Threading;
using System.IO;

namespace Client
{
  public  class ClientController
    {
        Proxy proxy = new Proxy();

        String path = "";

        /* This method opens the 
         * chanel for the commmunication
         * with the server.
         */ 
        public void OpenChannel()
        {
            proxy.GetChannel();
        }

        // posts the message to the server
        public void PostMessage(Object message)
        {
           path = proxy.GetChannel().postMessage(message.ToString());
        }

      

        /* This method fetches the log file of the executed tests
         * from the server and saves that file at the specified 
         * location.
         * @Params
         * string test_suite_name -- test suite name
         * string userName -- username
         * @returns a string which is used to identify that validity of results.
         */
        public String GetLogFile(string test_suite_name, string userName, string fileName)
        {
            try
            {
                Stream stream = proxy.GetChannel().GetLogFile(test_suite_name, userName, fileName);
                byte[] block = new byte[1024];
                int totalBytes = 0;
                Directory.CreateDirectory("Test results");
                var outputStream = new FileStream("Test Results\\" + test_suite_name+"_"+ fileName, FileMode.Create);
                while (true)
                {
                    int bytesRead = stream.Read(block, 0, 1024);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
                outputStream.Close();
                return test_suite_name + ".xml";
            }
            catch
            {
                return null;
            }
        }
 
        public bool Login(string username, string pass)
        {
            return proxy.GetChannel().Login(username, pass);
        }

        public bool Register(string username, string pass)
        {
            return proxy.GetChannel().register(username, pass);
        }

        /*
         * This method makes the xml message that is transfered to 
         * the server. After making this XML message it is posted
         * to the server which maked the folders needed to store 
         * the files to be tested.
         */
        public string makeXmlMessgae(List<string> files, string prjName, string username)
        {
            StringBuilder sBuilder = new StringBuilder();
            string ip = getLocalIpAdd();
            XElement root = new XElement("test_suite");
            XElement projectName = new XElement("project_name", prjName);
            XElement user = new XElement("username",username);
            XElement ipElem = new XElement("client_ip",ip);
            XElement fileElems = getFileSet(files);
            root.Add(user);
            root.Add(ipElem);
            root.Add(projectName);
            root.Add(fileElems);
            PostMessage(root);
            return sBuilder.ToString();
        }

        /*
         * This method gives the instruction 
         * to ther server to start the testing.
         * This process is done in two steps:
         * -- Files are transferred to the server
         * -- when all the files are transferred to the server
         *    instructions are given to initiate the testing process
         * This whole process is done by a dfferent/new thread.    
         */
        public void StartTesting(List<string> files)
        {
            ParameterizedThreadStart tProc = new ParameterizedThreadStart(sendFiles);
            Thread thread = new Thread(tProc);
            thread.IsBackground = true;
            thread.Start(files);
        }

        // sends the files to the server + gives
        // instrcutions to test the files.
        private void sendFiles(Object fileset)
        {
            List<string> files = (List<string>)fileset;
            foreach (string file in files)
            {
                proxy.SendFile(file, path);
            }
            proxy.GetChannel().TransferComplete(path);
        }

      // it uses the filelist and makes an XMl Element of it.
        private XElement getFileSet(List<string> files)
        {
            XElement elem = new XElement("fileNames");
            string fileName = null;
            if (null != files)
            { 
                foreach(string file in files)
                {
                    string[] str = file.Split('\\');
                      fileName = str[(str.Count()-1)];
                      XElement fileElement = new XElement("file_name");
                      fileElement.Value = fileName;
                      elem.Add(fileElement);
                }
            }
            return elem;
        }

        // returns the Ip of the machine
        private string getLocalIpAdd()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            string ipAddress = "";
            foreach (IPAddress ip in addr)
            {
               if (ip.AddressFamily.ToString() == "InterNetwork")
                    ipAddress = ip.ToString();
            }
            return ipAddress;
        }
    }   
}
