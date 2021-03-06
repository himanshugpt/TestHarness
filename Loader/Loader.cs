/////////////////////////////////////////////////////////////////////
// Loader.cs - Loads test libraries into Test Application Domain   //
// ver 1.1                                                         //
//                                                                 //
// Platform:    Dell Dimension 8300, Windows XP Pro, SP 2.0        //
// Application: CSE784 - Software Studio, Final Project Prototype  //
// Author:      Jim Fawcett, Syracuse University, CST 2-187        //
//              jfawcett@twcny.rr.com, (315) 443-3948              //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides operations to load test assemblies into a
 * child application domain, called TestDomain.  Interestingly, this
 * code runs in TestDomain.  This is necessary so that the primary
 * application domain of the test harness does not need to have any
 * information about the test types it will invoke, remotely, in
 * TestDomain.
 * 
 * Note: code shows options for loading from AppDomain.Load(...).
 * That is not used here, since we want to load from a specified
 * Directory.
 * 
 * Public Interface:
 * =================
 * AppDomain ad = AppDomain.CreateDomain("TestDomain",evidence,domainInfo);
 * ad.AppendPrivatePath(pathToLoader_);
 * ad.Load("Loader");
 * ObjectHandle oh = ad.CreateInstance("loader","TestHarness.Loader");
 * Loader ldr = oh.Unwrap() as Loader;
 * ldr.SetPath(pathToLibs_);
 * ldr.LoadTests();
 * ldr.RunTests();
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   Tester.cs, Loader.cs, ITest.cs, Test1.cs, Tested1.cs, ...
 * Compiler Command:
 *   csc /t:Library Loader.cs
 *   csc /t:Library ITest.cs
 *   csc /t:Library Test1.cs, Tested1.cs, ...
 *   csc /t:exe     Tester.cs
 * Deployment:
 *   Loader.dll --> TesterDir/Loader
 *   ITest.dll  --> TesterDir/TestLibraries
 *   Test dlls  --> TesterDir/TestLibraries
 *   where TesterDir is the folder containing Tester.exe
 *  
 * Maintence History:
 * ==================
 * ver 1.1 : 17 Oct 05
 *   - changed loading from AppDomain.Load() to Assembly.LoadFrom()
 *     to load only from a specified directory.
 *   - simplified function Invoker() by making id and ret members
 *     of the Loader class. 
 * ver 1.0 : 09 Oct 05
 *   - first release
 * 
 */
//
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;

namespace TestHarness
{
  public class Loader : MarshalByRefObject
  {
    string pathToTestLibs_;
    string id;
    bool ret;

    //----< Constructor placeholder >--------------------------------

    public Loader()
    {
    }
    //----< Set path to test libraries >-----------------------------
    //
    //  Path must be the name of a subdirectory under directory
    //  containing tester.exe.  Configure class will setup this
    //  path.  Use this for loading with AppDomain.Load(...).
    //
    public void SetPath(string path)
    {
      pathToTestLibs_ = path;

   ////////////////////////////////////////////////////////
   // Use this if loading wth AppDomain.Load(...)
   //   AppDomain.CurrentDomain.AppendPrivatePath(path);
   
      Console.Write("\n  Load Path: {0}",pathToTestLibs_);
    }
    //
    //----< Load tests into Test Domain >----------------------------

    public void LoadTests()
    {
      Console.Write("\n\n  Loader loading Tests into TestDomain");
      Console.Write("\n ----------------------------------------");

      Console.Write(
        "\n  Loading Tests into: {0}",
        AppDomain.CurrentDomain.FriendlyName
      );
      Console.Write(
        "\n  Loading from: {0}", Path.GetFullPath(pathToTestLibs_));

   //////////////////////////////////////////////////////
   // Use this if loading using AppDomain.Load(...)
   //   Console.Write(
   //   AppDomain.CurrentDomain.RelativeSearchPath);

      string[] libs = Directory.GetFiles(pathToTestLibs_,"*.dll");

      foreach (string lib in libs)
      {
          Console.Write("\n  Loading {0}", lib);
          Assembly.LoadFrom(lib);

          ////////////////////////////////////////////////////
          // Can load using Fusion rules with the following:
          //   string libFile = Path.GetFileNameWithoutExtension(lib);
          //   Console.Write("\n  Loading {0}",libFile);
          //   AppDomain.CurrentDomain.Load(libFile);

      }
    }
    //----< dynamic invocation >-------------------------------------

    void Invoker(Type type)
    {
      object testObj = Activator.CreateInstance(type);
      Type created = testObj.GetType();
      object[] args = new object[0];

      id = (string)created.InvokeMember(
        /* method name */ "id",
        /* action      */ BindingFlags.Default|BindingFlags.InvokeMethod,
        /* binder      */ null,
        /* instance    */ testObj,
        /* method args */ args
      );
      Console.Write("\n  classid: {0}",id.ToString());

      // test may throw so prepare for that exception

      ret = (bool)created.InvokeMember(
        /* method name */ "test",
        /* action      */ BindingFlags.Default|BindingFlags.InvokeMethod,
        /* binder      */ null,
        /* instance    */ testObj,
        /* method args */ args
      );
    }
    //
    //----< Run tests in libraries that support ITest interface >----

    public string RunTests()
    {
      StringWriter sw = new StringWriter();
      XmlTextWriter xtw = new XmlTextWriter(sw);
      xtw.WriteStartDocument();
      xtw.WriteStartElement("TestResults");

      Console.Write("\n\n  Running Tests in TestDomain");
      Console.Write("\n -----------------------------");

      ///////////////////////////////////////////////////
      // part of an alternate way to save test results
      // int count = 0;

      Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();
      foreach(Assembly assem in assems)
      {
        if(assem.FullName.IndexOf("mscorlib") != -1)
          continue;
        if(assem.FullName.IndexOf("ITest") != -1)
          continue;
        if (assem.FullName.IndexOf("Loader") != -1)
          continue;
        if (assem.FullName.IndexOf("AbstractTest") != -1)
            continue;
        Console.Write("\n  Loaded: {0}",assem.FullName);
        
        Type[] types = assem.GetTypes();
        foreach(Type type in types)
        {
          Type interf = type.GetInterface("ITest");
          if(interf != null)
          {
            HRTimer.HiResTimer timer = new HRTimer.HiResTimer();
            timer.Start();
            try
            {
              Invoker(type);
            }
            catch(Exception ex)
            {
              Console.Write("\n  {0}",ex.Message);
              ret = false;
            }
            finally // save test result as XML element
            {
              xtw.WriteElementString(id.ToString(),ret.ToString());
            }
            timer.Stop();
            ulong timeSpan = timer.ElapsedMicroseconds;

            if(ret.ToString().ToLower() == "true")
              Console.Write(
                "\n  test passed, elapsed time = {0} microsecs",timeSpan
              );
            else
              Console.Write(
                "\n  test failed, elapsed time = {0} microsecs",timeSpan
              );
            Console.WriteLine();

            ///////////////////////////////////////////
            // An alternate way to save test results
            // ++count;
            // AppDomain.CurrentDomain.SetData(count.ToString(),id);
            // AppDomain.CurrentDomain.SetData(id,ret);
          }
        }
      }
      xtw.WriteEndElement();
      xtw.WriteEndDocument();
      xtw.Close();
      return sw.ToString();
    }
  }
}
