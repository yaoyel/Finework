using System;
using CommandLine;
using CommandLine.Text;

namespace FineWork.SysSetup
{
    public class Options
    {
        public const String DbSetupVerb = "dbsetup";

        [VerbOption(DbSetupVerb, HelpText = "Database Setup.")]
        public DbSetupOptions DbSetupOptions { get; set; } = new DbSetupOptions();

        public const String GenRsaVerb = "genrsa";

        [VerbOption(GenRsaVerb, HelpText = "Generate RSA.Key")]
        public RsaGenOptions RsaGenOptions { get; set; } = new RsaGenOptions();

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("FileWork.SysSetup"),
                Copyright = new CopyrightInfo("chinahrd.net", 2015),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };
            help.AddOptions(this);
            return help;
        }
    }
}