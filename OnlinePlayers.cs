using static CGGO.OnlinePlayersUtility;

namespace CGGO
{
    public class OnlinePlayersPatches
    {
        [HarmonyPatch(typeof(ServerHandle), nameof(ServerHandle.GameRequestToSpawn))]
        [HarmonyPrefix]
        public static void OnServerHandleGameRequestToSpawnPre(ulong __0)
        {
            if (!IsHost()) return;
            if (__0 != clientId || !hostAfk) return;

            LobbyManager.Instance.GetClient(clientId).field_Public_Boolean_0 = false;
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.SetPlayer))]
        [HarmonyPostfix]
        static void OnGameManagerSpawnPlayerPost(PlayerManager __instance, ulong __0)
        {
            ulong steamId = __0;

            if (steamId == 0) return;

            if (!activePlayers.ContainsKey(steamId))
            {
                CreatePlayerData(steamId);

                activePlayers.Add(steamId, __instance);

                activePlayers = activePlayers
                .OrderBy(pair => pair.Value.playerNumber)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            }
        }

        [HarmonyPatch(typeof(LobbyManager), nameof(LobbyManager.RemovePlayerFromLobby))]
        [HarmonyPrefix]
        public static void OnLobbyManagerRemovePlayerFromLobbyPre(CSteamID __0)
        {
            ulong steamId = (ulong)__0;

            if (activePlayers.ContainsKey(steamId)) activePlayers.Remove(steamId);

            if (!cggoEnabled || !MapsManager.isCGGO) return;

            CGGOPlayer cggoPlayer = CGGOPlayer.GetCGGOPlayer(steamId);

            if (cggoPlayer != null)
            {
                if (cggoPlayersList.Contains(cggoPlayer)) cggoPlayersList.Remove(cggoPlayer);
                if (attackersList.Contains(cggoPlayer)) attackersList.Remove(cggoPlayer);
                if (defendersList.Contains(cggoPlayer)) defendersList.Remove(cggoPlayer);

                if (attackersList.Count == 0 || defendersList.Count == 0) GamePhaseManager._instance.SetPhase(GamePhaseType.GameEndingPhase);
            }
        }
    }

    public class OnlinePlayersManager : MonoBehaviour
    {
        void Awake()
        {
            activePlayers.Clear();
        }
    }

    public static class OnlinePlayersUtility
    {

        public static void CreatePlayerData(ulong steamId)
        {
            var dbManager = Database._instance;

            var newPlayer = new PlayerData { ClientId = steamId };
            newPlayer.Properties["Username"] = SteamFriends.GetFriendPersonaName((CSteamID)steamId).Replace("|", "");

            dbManager.AddNewPlayer(newPlayer);
        }
    }

    public class CGGOPlayer
    {
        public ulong SteamId { get; set; }
        public string Username { get; set; }
        public int Balance { get; set; }
        public int Team { get; set; }
        public bool Dead { get; set; }
        public bool Katana { get; set; }
        public bool Pistol { get; set; }
        public bool Shotgun { get; set; }
        public bool Rifle { get; set; }
        public bool Revolver { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public List<CGGOPlayer> Assisters { get; set; }
        public int KatanaId { get; set; }
        public int KnifeId { get; set; }
        public int Shield { get; set; }
        public int DamageTaken { get; set; }
        public ulong Killer { get; set; }

        public CGGOPlayer(PlayerManager player, int team)
        {
            ulong steamId = player.steamProfile.m_SteamID;
            SteamId = steamId;
            Username = player.username;
            Balance = 0;
            Team = team;
            Dead = false;
            Katana = false;
            Pistol = false;
            Shotgun = false;
            Rifle = false;
            Revolver = false;
            Kills = 0;
            Deaths = 0;
            Assists = 0;
            Assisters = [];
            KatanaId = 0;
            KnifeId = 0;
            Shield = 0;
            DamageTaken = 0;
            Killer = 0;
        }
        public static CGGOPlayer GetCGGOPlayer(ulong id)
        {
            return cggoPlayersList.Find(player => player.SteamId == id);
        }
    }
}
