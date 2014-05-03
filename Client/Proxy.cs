/////////////////////////////////////////////////////////////////////
// Proxy.cs - Test Harness                                         //
// ver 1.0                                                         //
//                                                                 //
// Application: Test harness Client                                //
// Author:      Himanshu Gupta, Syracuse University,               //
//              hgupta01@syr.edu,                                  //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the interface to communicate with the Services
 * and to make the channel for the same purpose.
 * 
 * Public Interface:
 * =================
 * GetChannel() // returns teh channel
 * SendFile(string file, string path)
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   IHarnessService.cs
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using TestHarnessServer;
using System.IO;

namespace Client
{
    class Proxy
    {
        IHarnessService channel = null;

        // creates a channel for the communication
        // using the given URL.
        void CreateChannel(string url)
        {
            EndpointAddress address = new EndpointAddress(url);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<IHarnessService> factory = new ChannelFactory<IHarnessService>(binding, address);
            channel = factory.CreateChannel();
        }

        // returns the channel object refrence
        public IHarnessService GetChannel() {
            if(channel == null)
            CreateChannel("http://localhost:4000/HarnessService");
            return channel;
        }

        // transfers the file to the server in blocks of bytes.
        public bool SendFile(string file, string path)
        {
            long blockSize = 512;
            Proxy proxy = new Proxy();
            
            try
            {
                string filename = Path.GetFileName(file);
                proxy.GetChannel().OpenFileForWrite(filename, path);
                FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read);
                int bytesRead = 0;
                while (true)
                {
                    long remainder = (int)(fs.Length - fs.Position);
                    if (remainder == 0)
                        break;
                    long size = Math.Min(blockSize, remainder);
                    byte[] block = new byte[size];
                    bytesRead = fs.Read(block, 0, block.Length);
                    proxy.channel.WriteFileBlock(block);
                }
                fs.Close();
                proxy.GetChannel().CloseFile();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  can't open {0} for writing - {1}", file, ex.Message);
                return false;
            }
        }
    }
}
