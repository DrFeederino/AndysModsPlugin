using BepInEx.Configuration;

namespace AndysModsPlugin.mods.ModToggle
{
    public class ToggleModClass
    {
        public ConfigEntry<bool> enabled; // default state of all mods
        public string name = null;
        public ToggleModClass(string name) { this.name = name; }
        public void Toggle() => enabled.Value = !enabled.Value;
    }

}