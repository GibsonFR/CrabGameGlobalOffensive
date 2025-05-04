
namespace CGGO
{
    public class Patches
    {
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        [HarmonyPrefix]
        static void OnGameManagerAwakePre(GameManager __instance)
        {
            __GameManager = __instance;

            allPlayersSpawned = false;
        }

        [HarmonyPatch(typeof(PlayerInventory), nameof(PlayerInventory.Awake))]
        [HarmonyPrefix]
        static void OnPlayerInventoryAwakePre(PlayerInventory __instance)
        {
            __PlayerInventory = __instance;
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.Awake))]
        [HarmonyPrefix]
        static void OnPlayerMovementAwakePre(PlayerMovement __instance)
        {
            __PlayerMovement = __instance;
        }

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
        [HarmonyPrefix]
        static void OnPlayerMovementAwakePre(SteamManager __instance)
        {
            __SteamManager = __instance;

            hostId = __instance.originalLobbyOwnerId.m_SteamID;
            lobbyId = __instance.currentLobby.m_SteamID;
        }
    }
}
