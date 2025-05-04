namespace CGGO
{
    public class MainConstants
    {

    }

    public class MainManager : MonoBehaviour
    {
        /// <summary>
        /// Reads the configuration file to ensure up-to-date settings each Round.
        /// </summary>
        void Awake()
        {
            ReadConfigFile();
        }
    }

    public class MainPatches
    {
        /// <summary>
        /// Retrieves and sets the Steam ID of the mod owner when SteamManager initializes, and some config parameters.
        /// </summary>
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
        [HarmonyPostfix]
        public static void OnSteamManagerAwakePost(SteamManager __instance)
        {
            if (clientId < 1)
            {
                clientId = (ulong)__instance.field_Private_CSteamID_0;
            }

            ServerConfig.field_Public_Static_Int32_5 = 4; 
            ServerConfig.field_Public_Static_Int32_6 = 4; 
            ServerConfig.field_Public_Static_Int32_7 = 3; 
            ServerConfig.field_Public_Static_Int32_8 = 3; 
        }

        // Damageable AK/Shotgun by lammas123 modified
        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.Awake))]
        [HarmonyPostfix]
        internal static void PostItemManagerAwake()
        {
            if (!SteamManager.Instance.IsLobbyOwner()) return;

            ItemManager.idToItem[0].itemName = "Vandal";
            ItemManager.idToItem[3].itemName = "Shorty";
        }
        // Damageable AK/Shotgun by lammas123 modified
    }
    public class MainUtility
    {
       
    }
}
