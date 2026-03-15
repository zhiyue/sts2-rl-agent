using MegaCrit.Sts2.Core.Modding;

namespace STS2BridgeMod;

[ModInitializer("Initialize")]
public class BridgeMod
{
    public static void Initialize()
    {
        Console.WriteLine("[STS2Bridge] Hello from Bridge Mod!");
        System.IO.File.WriteAllText(
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "sts2bridge_loaded.txt"),
            $"Bridge Mod loaded at {DateTime.Now}");
    }
}
