using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using com.clusterrr.cloverhack;
using System.IO;
using System.Reflection;

namespace UsbTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int result = -1;
//#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(System.Console.Error));
//#endif
            using (var nes = new ClovershellConnection())
            {
                try
                {
                    if (args.Length == 0)
                    {
                        ShowHelp();
                        Environment.Exit(-1);
                    }
                    var command = args[0].ToLower();
                    nes.Enabled = true;
                    int t = 300;
                    while (!nes.Online)
                    {
                        Thread.Sleep(10);
                        t--;
                        if (t == 0) throw new Exception("no clovershell connection");
                    }
                    var ping = nes.Ping();
                    if (ping < 0) throw new Exception("connected to NES mini but clovershell is not responding");
                    switch (command)
                    {
                        case "shell":
                            nes.ShellEnabled = true;
                            nes.Autoreconnect = true;
                            Console.WriteLine("Started shell server on port {0}.", nes.ShellPort);
                            Console.WriteLine("Connect to it using terminal client (raw mode, no local echo).");
                            Console.WriteLine("Press ENTER to stop.");
                            Console.ReadLine();
                            result = 0;
                            break;
                        case "exec":
                            if (args.Length < 2)
                            {
                                ShowHelp();
                                Environment.Exit(-1);
                            }
                            Stream stdin = null;
                            if (args.Length >= 3)
                            {
                                if (args[2] == "null")
                                    stdin = null;
                                else if (args[2] == "-")
                                    stdin = Console.OpenStandardInput();
                                else
                                    stdin = new FileStream(args[2], FileMode.Open);
                            }
                            Stream stdout;
                            if (args.Length >= 4)
                            {
                                if (args[3] == "-")
                                    stdout = Console.OpenStandardOutput();
                                else if (args[3] == "null")
                                    stdout = null;
                                else
                                    stdout = new FileStream(args[3], FileMode.Create);
                            }
                            else stdout = Console.OpenStandardOutput();
                            Stream stderr;
                            if (args.Length >= 5)
                            {
                                if (args[4] == "-")
                                    stderr = Console.OpenStandardError();
                                else if (args[4] == "null")
                                    stderr = null;
                                else
                                    stderr = new FileStream(args[4], FileMode.Create);
                            }
                            else stderr = Console.OpenStandardError();
                            var s = DateTime.Now;
                            result = nes.Execute(args[1], stdin, stdout, stderr);
                            Console.Error.WriteLine("Done in {0}ms. Exit code: {1}", (int)(DateTime.Now - s).TotalMilliseconds, result);
                            break;
                        default:
                            ShowHelp();
                            Environment.Exit(-1);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error: " + ex.Message);
                }
#if DEBUG
                //Debug.WriteLine("Done.");
                //Console.ReadLine();
#endif
            }
            Environment.Exit(result);
        }

        static void ShowHelp()
        {
            Console.WriteLine("clovershell client (c) cluster, 2017");
            Console.WriteLine("Usage: {0} shell\r\nUsage: {0} exec <command> [stdin [stdout [stderr]]]", Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase));
        }
    }
}
