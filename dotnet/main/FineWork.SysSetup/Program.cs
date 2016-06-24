using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CommandLine.Text;

namespace FineWork.SysSetup
{
    internal class Program
    {
        /// <summary> The entry point. </summary>
        /// <param name="args">The command line args.</param>
        private static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                var helpUri = ConfigurationManager.AppSettings["CommandLineHelpUri"];
                Console.WriteLine($"No parameters, see {helpUri} for help.");
                return;
            }

            string invokedVerb = null;
            object verbOptions = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
                {
                    // if parsing succeeds the verb name and correct instance
                    // will be passed to onVerbCommand delegate (string,object)
                    invokedVerb = verb;
                    verbOptions = subOptions;
                }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            switch (invokedVerb)
            {
                case Options.DbSetupVerb:
                    new DbSetup().Execute((DbSetupOptions)verbOptions);
                    break;
                case Options.GenRsaVerb:
                    new RsaGen().Execute((RsaGenOptions)verbOptions);
                    break;
            }
        }
    }

    public class RsaGen
    {
        public void Execute(RsaGenOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(options.KeySize);
            var contents = provider.ToXmlString(true);
            System.IO.File.WriteAllText(options.FileName, contents);
        }
    }
}