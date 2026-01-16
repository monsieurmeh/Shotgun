using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shotgun
{
    internal static class SetupTest
    {
        public static bool Validate(ConfigurationReferences refs)
        {
            if (!ValidateTopLevel(refs)) return false;

            return true;
        }


        internal static bool ValidateTopLevel(ConfigurationReferences refs)
        {
            if (!ValidateFPSCamera(refs)) return false;
            if (!ValidateFPSPlayer(refs)) return false;
            return true;
        }


        internal static bool ValidateFPSCamera(ConfigurationReferences refs)
        {
            if (!TestAndReport(() => refs.FPSCamera != null, "Null FPSCamera")) return false;
            return true;
        }


        internal static bool ValidateFPSPlayer(ConfigurationReferences refs)
        {
            if (!TestAndReport(() => refs.FPSPlayer != null, "Null FPSPlayer")) return false;
            return true;
        }


        internal static bool TestAndReport(Func<bool> test, string error)
        {
            if (!test.Invoke())
            {
                MelonLogger.Msg($"[TEST FAILURE]: {error}");
                return false;
            }
            return true;
        }
    }

}
