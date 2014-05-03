/////////////////////////////////////////////////////////////////////
// Program.cs - Test Harness                              //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Server                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the functionality 
 * to host the webservices.
 * 
 * Build Process:
 * ==============
 * Files Required:
 *  HarnessServiceImpl.cs
 *  IHarnessService.cs
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace TestHarnessServer
{
    class ServiceLauncher
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Communication Server Starting up");
            Console.Write("\n ==================================\n");

            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri("http://localhost:4000/HarnessService");

            using (ServiceHost serviceHost = new ServiceHost(typeof(HarnessServiceImpl), baseAddress))
            {
                serviceHost.AddServiceEndpoint(typeof(IHarnessService), binding, baseAddress);
                serviceHost.Open();
                Console.Write("\n  HarnessServiceImpl is ready.");
                Console.Write("\n\n  Press <ENTER> to terminate service.\n\n");
                Console.ReadLine();//waits for the user instruction to close the service
            }
        }
    }
}
