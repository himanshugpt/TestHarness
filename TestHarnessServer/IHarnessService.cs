/////////////////////////////////////////////////////////////////////
// IHarnessService.cs - Test Harness                              //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Server                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the interface of the 
 * services which will be provided as web-services.
 * The annotations are used here to indicate that which 
 * method will be exposed as services.
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

 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;

namespace TestHarnessServer
{
    [ServiceContract]
    interface IHarnessService
    {
        //metod to post the XML message as String
        [OperationContract]
        String postMessage(String xmlMessage);

        // Method to open file for writing
        // the data.
        [OperationContract]
        bool OpenFileForWrite(string name, string path);

        // this method call have to be preceded by 
        // OpenFileForWrite method.
        [OperationContract]
        bool WriteFileBlock(byte[] block);

        // method to close file.
        [OperationContract]
        bool CloseFile();

        // This method is called when the transfer of files
        // is complete.
        [OperationContract]
        void TransferComplete(string path);

        // This methid takes username and password and 
        // returns a boolean value after vallidating them.
        [OperationContract]
        bool Login(string username, string password);

        // Checks that the user name is already present 
        // in the system or not.
        [OperationContract]
        bool IsUserNameRegistered(string username);

        // method to register the user in the system.
        [OperationContract]
        bool register(string username, string password);

        // Mehtod to fetch the results.
       /* [OperationContract]
        String GetResults(string test_suite_name, string userName);*/

        // Method to get the generated log file.
        [OperationContract]
        Stream GetLogFile(string test_suite_name, string userName, string fileName);

         [OperationContract]
        Stream GetAdditionalLogFIles(string test_suite_name,string userName);
    }
}
