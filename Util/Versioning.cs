using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shotgun.Util
{
    internal static class Versioning
    {
        internal const string CurrentVersion = "0.4.6";

        internal static bool IsVersionCorrect(string basePath)
        {
            bool shouldRefreshEmbeddedData = false;
            string detectedVersion;
            if (!File.Exists(Path.Combine(basePath, "VersionInfo.txt")))
            {
                MelonLogger.Msg($"Could not detect version info file, making with current version {CurrentVersion}");
                shouldRefreshEmbeddedData = true;
                File.WriteAllText(Path.Combine(basePath, "VersionInfo.txt"), CurrentVersion, System.Text.Encoding.UTF8);
            }
            detectedVersion = File.ReadAllText(Path.Combine(basePath, "VersionInfo.txt"), System.Text.Encoding.UTF8);
            if (detectedVersion != CurrentVersion)
            {
                MelonLogger.Msg($"Detected version {detectedVersion} is not {CurrentVersion}");
                shouldRefreshEmbeddedData = true;
                File.WriteAllText(Path.Combine(basePath, "VersionInfo.txt"), CurrentVersion, System.Text.Encoding.UTF8);
            }
            return shouldRefreshEmbeddedData;
        }
    }
}
