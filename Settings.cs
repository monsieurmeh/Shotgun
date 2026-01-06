using ModSettings;
using Il2Cpp;


namespace Shotgun
{
    public class Settings : JsonModSettings
    {
        public Settings() : base(Path.Combine("Shotgun", "Shotgun"))
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