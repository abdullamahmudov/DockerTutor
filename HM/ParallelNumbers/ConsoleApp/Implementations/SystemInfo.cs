using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    public class SystemInfo : IInfo
    {
        /// <inheritdoc/>
        public string GetInfo()
        {
            return string.Concat(
                "OS Description: ", RuntimeInformation.OSDescription, "\n",
                "OS Version (Environment.OSVersion): ", Environment.OSVersion.VersionString, "\n",
                "OS Architecture: ", RuntimeInformation.OSArchitecture, "\n",
                "Process Architecture: ", RuntimeInformation.ProcessArchitecture, "\n",
                "Machine Name: ", Environment.MachineName, "\n",
                "Processor Count: ", Environment.ProcessorCount, "\n",
                "64-bit OS: ", Environment.Is64BitOperatingSystem);
        }
    }
}