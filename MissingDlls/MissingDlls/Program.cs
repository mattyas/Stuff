using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace MissingDlls
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("MissingDlls (source at http://www.github.com/mattyas/stuff/)");
                Console.WriteLine("Usage:");
                Console.WriteLine("MissingDlls <path to assembly> [onlyerrors] [hidegac] [ignore=exactAssemblyName]");
                return 1;
            }

            var path = args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine("File: '{0}' does not exist.", path);
                return 2;
            }

            var missing = new Missing(path, IsSet(args, "onlyerrors"), IsSet(args, "hidegac"), Ignores(args));
            missing.PrintTree();
            return missing.IsSuccess ? 0 : 3;
        }

        private static string[] Ignores(IEnumerable<string> args)
        {
            return
                args.Skip(1)
                    .Select(x => x.Split('='))
                    .Where(x => x.Length == 2 && x[0] == "ignore")
                    .Select(x => x[1])
                    .ToArray();
        }

        private static bool IsSet(IEnumerable<string> args, string arg)
        {
            return args.Skip(1).Any(x => x == arg);
        }
    }
}

