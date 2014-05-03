/////////////////////////////////////////////////////////////////////
// FileController.cs - Test Harness                                //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Server                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the functionalities to make the
 * server ready to store the files received from 
 * the client.
 * 
 * Public Interface:
 * =================
 * saveMessage(String message) // saves the xml Message
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using TestHarness;

namespace TestHarnessServer
{
 public class FileController
    {
     String xmlMessagePath = @"TestHarnessTestSuites\XMLMessages";
     String clinetFilespath = @"TestHarnessTestSuites\Files";

     /*
      * This method saves the XML message at the 
      * server side. It also creats the required
      * Directories where the XML message and files
      * have to be saved.
      */ 
     public string saveMessage(String message)
     {   
        return SetUpClientSpace(message);
     }

     private string SetUpClientSpace(String message)
     {
         XDocument xdoc = XDocument.Parse(message);
         XElement ip = xdoc.Elements("test_suite").Elements("client_ip").First();
         String userName = xdoc.Elements("test_suite").Elements("username").First().Value;
         Console.WriteLine("Receiveing request from IP: {0}", ip.Value);
         String testSuiteName = xdoc.Elements("test_suite").Elements("project_name").First().Value;
         string xmlMessPath = xmlMessagePath + "\\" + userName + "\\" + testSuiteName;
         string filePath = clinetFilespath + "\\" + userName + "\\" + testSuiteName;
         if (!Directory.Exists(xmlMessPath))
         {
             DirectoryInfo dir = Directory.CreateDirectory(xmlMessPath);
         }
         else
         {
             Console.WriteLine("Directory found for storing xml Messages");
         }

         if (!Directory.Exists(filePath))
         {
             DirectoryInfo dir = Directory.CreateDirectory(filePath);
         }
         else
         {
             Console.WriteLine("Directory found for storing client files");
         }
         TextWriter tw = new StreamWriter(xmlMessPath +"\\message.xml");
         Console.WriteLine("message from client received {0}", message);
         tw.WriteLine(message);
         tw.Close();
         return filePath;
     }
    }
}
