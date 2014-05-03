﻿/////////////////////////////////////////////////////////////////////
// HarnessServiceImpl.cs - Test Harness                            //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Server                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the implemantaion of the IHarnessService
 * interface. 
 * 
 * Public Interface:
 * =================
        String postMessage(String xmlMessage);  
        bool OpenFileForWrite(string name, string path);
        bool WriteFileBlock(byte[] block);
        bool CloseFile();
        void TransferComplete(string path);
        bool Login(string username, string password);
        bool IsUserNameRegistered(string username);
        bool register(string username, string password);
        String GetResults(string test_suite_name, string userName);
        Stream GetLogFile(string test_suite_name, string userName);
 * Build Process:
 * ==============
 * Required Files:
 *  IHarnessService.cs
 *  FileController.cs
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.ServiceModel;
using TestHarness;
using System.Threading;

namespace TestHarnessServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    class HarnessServiceImpl : IHarnessService
    {
        string filePath = ".\\SentFiles";
        string fileSpec = "";
        FileStream fs = null;  // remove static for WSHttpBinding

        /**
         * This method uses the saveMessage method of
         * fileController and returns the path 
         * to the calling function..
         */
        public String postMessage(String xmlMessage)
        {
            FileController fc = new FileController();
            string path = fc.saveMessage(xmlMessage);
            return path;
        }

        // path used by the server
        public void SetServerFilePath(string path)
        {
            filePath = path;
        }

        /* It returns the stream to the log file generated by the
         * testharness server system. It takes the name of the test 
         * suite and username to detect the location of the file to 
         * be transferred.
         */ 
        public Stream GetLogFile(string test_suite_name, string userName, string fileName)
        {
            FileStream stream = null;
            String filePath = "TestHarnessTestSuites\\Files\\"+ userName +"\\" + test_suite_name+"\\" + fileName;
            try
            {
                String p = Path.GetFullPath(filePath);
                stream = new FileStream(p, FileMode.Open);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                stream = null;
                return stream;
            }
            return stream;
        }

        /*
         * This function opens the file so that the data
         * can be written into it. The clinet function
         * provides the name of the file and the path
         * where the file is loacted.
         * This path parameter is the value when the client
         * application code gets when the XML message is
         * posted by it.
         */ 
        public bool OpenFileForWrite(string name,String path)
        {
            if (path == "")
                return false;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
                fileSpec = path + "\\" + name;
                Console.WriteLine(fileSpec + " filespec");
            try
            {
                fs = File.Open(fileSpec, FileMode.Create, FileAccess.Write);
                this.SetServerFilePath(path);
                return true;
            }
            catch
            {
                Console.Write("\n  {0} failed to open", fileSpec);
                return false;
            }
        }


        public Stream GetAdditionalLogFIles(string test_suite_name, string userName)
        {
            //List<Stream> streamList = new List<Stream>();
            String filePath = "TestHarnessTestSuites\\Files\\" + userName + "\\" + test_suite_name;
            FileStream stream = null;
           
            try
            {
                String[] files = Directory.GetFiles(filePath);
                Console.WriteLine(files.Count().ToString() + "  files count");

                Console.WriteLine("searchign for {0}", test_suite_name);
                    foreach (string fname in files)
                    {
                        Console.WriteLine("file found for {0}", fname);
                        if (test_suite_name.Contains(fname) && fname.Contains(".log"))
                            Console.WriteLine(fname);
                        //if(fname.Contains(yes))
                        stream = new FileStream(filePath, FileMode.Open);
                        //streamList.Add(stream);
                    }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " Exception occured");
                stream = null;
                return null;
            }
            return stream;
        }

        // writes the bytes in the file.
        public bool WriteFileBlock(byte[] block)
        {
            try
            {
               // Console.Write("\n  writing block with {0} bytes", block.Length);
                if (fs != null)
                    fs.Write(block, 0, block.Length);
                else
                {
                    fs = File.Open(fileSpec, FileMode.Open, FileAccess.Write);
                    fs.Write(block, 0, block.Length);
                }
                fs.Flush();
                return true;
            }
            catch(Exception e) {
                
                Console.WriteLine("Error in writing byutes\n {0}", e);
                return false; }
        }

        // closes the file when writing is finished.
        public bool CloseFile()
        {
            try
            {
                fs.Close();
                return true;
            }
            catch { return false; }
        }

        // called when all the files are transferred 
        // and testing has to be initiated.
        public void TransferComplete(string path)
        {
            Tester tstr = new Tester();
            Thread t = tstr.SelectConfigAndRun(path);
        }

        // used to login the user.
        public bool Login(string username, string password)
        {
            bool status = false;
            XDocument doc = null;
            try
            {
                doc = XDocument.Load("Users.xml");
                var elems = doc.Element("users").Elements("user");
                foreach (XElement elem in elems)
                {
                    if (elem.Element("username").Value == username && elem.Element("password").Value == password)
                        return true;
                }
            }
            catch
            {
                return false;
            }
            return status;
        }

        // checks for the given username in the user registry.
        public bool IsUserNameRegistered(string username)
        {
            bool status = false;
            XDocument doc = null;
            try
            {
                doc = XDocument.Load("Users.xml");
                var elems = doc.Element("users").Elements("user");
                foreach (XElement elem in elems)
                {
                    if (elem.Element("username").Value == username)
                        return true;
                }
            }
            catch
            {
                return false;
            }
         return status;
        }
        
        /* used to register for a new user.
         * It also creates a new registry file if
         * it is not present.
         */ 
        public bool register(string username, string password)
        {
            Console.WriteLine(" in register {0} {1}", username, password);
            if (IsUserNameRegistered(username))
                return false;
            bool status = false;
            XDocument doc = null;
            try
            {
                doc = XDocument.Load("Users.xml");
                XElement users = doc.Element("users");
                XElement user = new XElement("user");
                XElement loginname = new XElement("username", username);
                XElement pass = new XElement("password", password);
                user.Add(loginname);
                user.Add(pass);
                users.Add(user);
                users.Save("Users.xml");
                status = true;
            }
            catch 
            {
                doc = new XDocument();
                XElement users = new XElement("users");
                XElement user = new XElement("user");
                XElement loginname = new XElement("username", username);
                XElement pass = new XElement("password", password);
                user.Add(loginname);
                user.Add(pass);
                users.Add(user);
                users.Save("Users.xml");
                status = true;
            }

            return status;
        }
    }

  
}