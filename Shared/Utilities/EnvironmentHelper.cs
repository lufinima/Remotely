﻿using Remotely.Shared.Enums;
using System;
using System.Diagnostics;

namespace Remotely.Shared.Utilities
{
    public static class EnvironmentHelper
    {
        public static string AgentExecutableFileName
        {
            get
            {
                switch (Platform)
                {
                    case Platform.Windows:
                        return "Remotely_Agent.exe";
                    case Platform.Linux:
                    case Platform.MacOS:
                        return "Remotely_Agent";
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }

        public static string DesktopExecutableFileName
        {
            get
            {
                switch (Platform)
                {
                    case Platform.Windows:
                        return "Remotely_Desktop.exe";
                    case Platform.Linux:
                    case Platform.MacOS:
                        return "Remotely_Desktop";
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }


        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
    }
}


        public static bool IsLinux => OperatingSystem.IsLinux();

        public static bool IsMac => OperatingSystem.IsMacOS();

        public static bool IsWindows => OperatingSystem.IsWindows();

        public static Platform Platform
        {
            get
            {
                if (IsWindows)
                {
                    return Platform.Windows;
                }
                else if (IsLinux)
                {
                    return Platform.Linux;
                }
                else if (IsMac)
                {
                    return Platform.MacOS;
                }
                else
                {
                    return Platform.Unknown;
                }
            }
        }

        public static string StartProcessWithResults(string command, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo(command, arguments)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "RunAs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var proc = Process.Start(psi);
                proc.WaitForExit();

                return proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Failed to start process.");
                return string.Empty;
            }
        }
    }
}
