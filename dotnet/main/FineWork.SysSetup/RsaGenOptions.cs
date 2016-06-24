using System;
using CommandLine;

namespace FineWork.SysSetup
{
    public class RsaGenOptions
    {
        [Option("fname", DefaultValue = "RSA.Key", HelpText = "File Path and Name.")]
        public String FileName { get; set; }

        [Option("keysize", DefaultValue = 2048, HelpText = "The key size.")]
        public int KeySize { get; set; }
    }
}