namespace AndysModsPlugin.mods.ModToggle
{
    public class ToggleModClass
    {
        public bool IsEnabled = true; // default state of all mods
        public void Toggle() => IsEnabled = !IsEnabled;
    }

}