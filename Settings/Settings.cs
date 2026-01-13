using ModSettings;
using Il2Cpp;


namespace Shotgun
{
    internal class Settings : JsonModSettings
    {
        internal Settings() : base(Path.Combine("Shotgun", "Shotgun"))
        {
            Initialize();
        }

        protected void Initialize()
        {
            AddToModSettings("Shotgun");
            RefreshGUI();
        }
    }
}