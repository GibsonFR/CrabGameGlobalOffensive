global using BepInEx;
global using BepInEx.IL2CPP;
global using HarmonyLib;
global using SteamworksNative;
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Linq;
global using System.IO;
global using System.Reflection;
global using UnityEngine;
global using UnhollowerRuntimeLib;
global using System.Globalization;
global using System.Text;
global using UnityEngine.UI;

global using static CGGO.Variables;
global using static CGGO.Utility;
global using static CGGO.ClientUtility;
global using static CGGO.WeaponsUtility;
global using static CGGO.EconomyUtility;
global using static CGGO.GamePhaseUtility;

namespace CGGO
{
    [BepInPlugin("81739262-E302-4CC8-B574-CECA45855793", "CGGO", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public static Plugin Instance;
        public override void Load()
        {
            Instance = this;

            ClassInjector.RegisterTypeInIl2Cpp<MainManager>();
            ClassInjector.RegisterTypeInIl2Cpp<DatabaseManager>();
            ClassInjector.RegisterTypeInIl2Cpp<MenuManager>();
            ClassInjector.RegisterTypeInIl2Cpp<ClientManager>();
            ClassInjector.RegisterTypeInIl2Cpp<OnlinePlayersManager>();
            ClassInjector.RegisterTypeInIl2Cpp<KeyBindManager>();
            ClassInjector.RegisterTypeInIl2Cpp<MapsManager>();
            ClassInjector.RegisterTypeInIl2Cpp<GamePhaseManager>();
            ClassInjector.RegisterTypeInIl2Cpp<MilkZoneManager>();
            ClassInjector.RegisterTypeInIl2Cpp<ItemsManager>();

            Harmony.CreateAndPatchAll(typeof(Plugin));
            Harmony harmony = new("gibson.cggo");
            harmony.PatchAll(typeof(MainPatches));
            harmony.PatchAll(typeof(OnlinePlayersPatches));
            harmony.PatchAll(typeof(Patches));
            harmony.PatchAll(typeof(GamePhasePatches));
            harmony.PatchAll(typeof(WeaponsPatchs));
            harmony.PatchAll(typeof(CommandPatchs));
            harmony.PatchAll(typeof(PlantingPatches));

            CreateFolder(mainFolderPath);
            CreateFolder(playersDataFolderPath);
            CreateFile(playersDataFilePath);

            CreateFile(logFilePath);
            ResetFile(logFilePath);

            CreateFile(configFilePath);
            SetConfigFile(configFilePath);

            Database.UpdateProperties(playersDataFilePath);

            BetterUnityParameters();

            Log.LogInfo("Mod created by Gibson, discord : gib_son, github : GibsonFR");
        }

        [HarmonyPatch(typeof(GameUI), nameof(GameUI.Awake))]
        [HarmonyPostfix]
        public static void UIAwakePatch(GameUI __instance)
        {
            GameObject pluginObj = new();

            Text text = pluginObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.supportRichText = true;
            text.raycastTarget = false;

            MenuManager menu = pluginObj.AddComponent<MenuManager>();
            menu.menuText = text;

            pluginObj.AddComponent<MainManager>();
            pluginObj.AddComponent<DatabaseManager>();
            pluginObj.AddComponent<ClientManager>();
            pluginObj.AddComponent<OnlinePlayersManager>();
            pluginObj.AddComponent<KeyBindManager>();
            pluginObj.AddComponent<MapsManager>();
            pluginObj.AddComponent<GamePhaseManager>();
            pluginObj.AddComponent<MilkZoneManager>();
            pluginObj.AddComponent<ItemsManager>();


            pluginObj.transform.SetParent(__instance.transform);
            pluginObj.transform.localPosition = new(pluginObj.transform.localPosition.x, -pluginObj.transform.localPosition.y, pluginObj.transform.localPosition.z);
            RectTransform rt = pluginObj.GetComponent<RectTransform>();
            rt.pivot = new(0, 1);
            rt.sizeDelta = new(1920, 1080);

        }

        [HarmonyPatch(typeof(EffectManager), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(LobbyManager), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(LobbySettings), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(MethodBase __originalMethod)
        {
            return false;
        }
    }
}