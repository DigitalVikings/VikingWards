using BepInEx;
using HarmonyLib;
using VikingWards.Conduits;

namespace VikingWards
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.digitalvikings.vikinglib", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.digitalvikings.vikingwards";
        public const string ModName = "VikingWards";
        public const string ModVersion = "0.1.1";

        private Harmony? _harmony;

        private void Awake()
        {
            Configuration.ModConfig.Init(Config);

            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll();

            Logger.LogInfo($"{ModName} v{ModVersion} loaded successfully with 5 Divine Conduits.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
