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
            string source;
            string target;
            int result = -1;
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(System.Console.Error));
#endif
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
                    nes.Connect();
                    var ping = nes.Ping();
                    if (ping < 0) throw new Exception("connected to NES Mini but clovershell is not responding");
                    switch (command)
                    {
                        case "shell":
                            if (args.Length >= 2)
                                nes.ShellPort = ushort.Parse(args[1]);
                            nes.ShellEnabled = true;
                            nes.AutoReconnect = true;
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
                        case "pull":
                            if (args.Length < 2)
                            {
                                ShowHelp();
                                Environment.Exit(-1);
                            }
                            source = args[1];
                            if (args.Length >= 3)
                                target = args[2];
                            else
                            {
                                target = source;
                                int pos;
                                while ((pos = target.IndexOf("/")) >= 0)
                                    target = target.Substring(pos + 1);
                            }
                            source = source.Replace("'", "\\'");
                            result = nes.Execute("cat '" + source + "'", null, new FileStream(target, FileMode.Create), Console.OpenStandardError());
                            break;
                        case "push":
                            if (args.Length < 3)
                            {
                                ShowHelp();
                                Environment.Exit(-1);
                            }
                            source = args[1];
                            target = args[2];
                            target=target.Replace("'", "\\'");
                            result = nes.Execute("cat > '" + target + "'", new FileStream(source, FileMode.Open), Console.OpenStandardOutput(), Console.OpenStandardError());
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
            Console.WriteLine("clovershell client v{0} (c) Alexey 'Cluster' Avdyukhin, 2017", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(
                   "Usage: {0} shell [port]\r\n"+
                   "Usage: {0} exec <command> [stdin [stdout [stderr]]]\r\n"+
                   "Usage: {0} pull <remote_file> [local_file]\r\n"+
                   "Usage: {0} push <local_file> <remote_file>\r\n"
                   , Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase));
            Console.WriteLine("Examples:");
            Console.WriteLine(
                   "Start shell server on port 23:\r\n {0} shell 23\r\n" +
                   "List files:\r\n {0} exec \"ls /etc/\"\r\n" +
                   "Download file:\r\n {0} pull /etc/inittab inittab\r\n" +
                   "Upload file:\r\n {0} push inittab /etc/inittab\r\n" +
                   "Archive and download files:\r\n {0} exec \"cd /etc && tar -czv *\" > file.tar.gz\r\n" +
                   "Archive and download files (alternative):\r\n {0} exec \"cd /etc && tar -czv *\" null file.tar.gz\r\n" +
                   "Upload and extract files:\r\n {0} exec \"cd /etc && tar -xzv\" file.tar.gz\r\n" +
                   "Upload and extract files (alternative):\r\n {0} exec \"cd /etc && tar -xzv\" - <file.tar.gz\r\n" +
                   "Dump the whole decrypted filesystem:\r\n {0} exec \"dd if=/dev/mapper/root-crypt | gzip\" > dump.img.gz\r\n" +
                   "Dump the whole decrypted filesystem (alternative):\r\n {0} exec \"dd if=/dev/mapper/root-crypt | gzip\" null dump.img.gz\r\n"
                   , Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase));

        }
    }
}
