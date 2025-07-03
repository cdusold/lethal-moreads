using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using MoreAds.Patches;
using MoreAds.Compat;
using MoreAds.Configs;
using HarmonyLib;

namespace MoreAds
{
    [BepInPlugin("com.github.cdusold.LethalMoreAds", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("cdusold.LethalMoreAds");
        internal static readonly bool debug = true;
        private static Plugin Instance;

        public static ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            logger = Logger;
            Logger.LogInfo("Mod cdusold.LethalMoreAds is loaded!");

            ConfigManager.Init(Config);

            if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            {
                LethalConfigManager.Init(Config);
            }
            if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
            {
                LobbyCompatibilityManager.Init();
            }
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(TimeOfDayPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
