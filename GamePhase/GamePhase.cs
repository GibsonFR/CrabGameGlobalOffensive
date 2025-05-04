using static CGGO.GamePhaseManager;
using static CGGO.GamePhaseUtility;
using static CGGO.GamePhaseConstants;

namespace CGGO
{
    public static class GamePhaseConstants
    {
        public const int ASSIST_MAX_DELAY_MS = 5000;
        public const int DEFAULT_TILE_DRIVE_MAP_ID = 30;
        public const int TILE_DRIVE_GAMEMODE_ID = 9;
        public const int WAITING_GAMEMODE_ID = 0;
    }
    public static class GamePhaseUtility
    {
        public static void ManageRoundVictory(int winnersTeamId, int LosersTeamId)
        {
            roundWinnerTeamId = winnersTeamId;

            if (winnersTeamId == TeamsId.ATTACKERS_ID) attackersScore += 1;
            else defendersScore += 1;

            DistributeEndRoundMoney(winnersTeamId);

            ManageLoseStrike(LosersTeamId);
        }
        public static bool IsGamePhase(GamePhaseType gamePhaseType) => _currentGamePhase == gamePhaseType;
        public static void SendShieldValue()
        {
            if (elapsedShield < 0.5f) return;
            foreach (var player in cggoPlayersList) if (player.SteamId != clientId) ServerSend.SendGameModeTimer(player.SteamId, player.Shield, 0); // No shield display for host
            elapsedShield = 0f;

        }
        public static void ManageLoseStrike(int loserTeam)
        {
            if (TeamsId.ATTACKERS_ID == loserTeam && attackersLoseStrike < 2)
            {
                attackersLoseStrike += 1;
                defendersLoseStrike = 0;
            }
            else if (TeamsId.DEFENDERS_ID != loserTeam && defendersLoseStrike < 2)
            {
                defendersLoseStrike += 1;
                attackersLoseStrike = 0;
            }
        }

        public static bool IsAssistDelayRespected(DateTime lastHitTime) => (DateTime.Now - lastHitTime).TotalMilliseconds <= ASSIST_MAX_DELAY_MS;

        public static void ManagePlayerDied(CGGOPlayer player)
        {
            player.Dead = true;
            player.Deaths++;
            ResetPlayerShield(player);
            ResetPlayerWeapons(player);
        }

        public static void RecomputeAliveFlags()
        {
            allAttackersDead = attackersList.All(attacker => attacker.Dead);
            allDefendersDead = defendersList.All(defender => defender.Dead);
        }

        public static CGGOPlayer ResolveKiller(CGGOPlayer killed)
        {
            if (killed.Killer != 0)
            {
                var recordedKiller = CGGOPlayer.GetCGGOPlayer(killed.Killer);
                killed.Killer = 0;
                return recordedKiller;
            }

            if (hitPlayersDico.TryGetValue(killed.SteamId, out var hitInfo) &&
                IsAssistDelayRespected(hitInfo.Value))
            {
                hitPlayersDico.Remove(killed.SteamId);
                return CGGOPlayer.GetCGGOPlayer(hitInfo.Key);
            }

            hitPlayersDico.Remove(killed.SteamId);
            return null;
        }
        public static void HandleAssists(CGGOPlayer victim, CGGOPlayer killer)
        {
            if (killer == null) return;

            foreach (var assister in victim.Assisters)
            {
                if (assister.SteamId == killer.SteamId) continue;
                assister.Assists++;
                GiveAssistBounty(assister);
            }
        }

        public static void HandleKill(CGGOPlayer killer, CGGOPlayer victim)
        {
            if (killer == null || killer.Team == victim.Team) return;
            killer.Kills++;
            GiveKillBounty(killer);
        }
    }
    public static class TeamsId
    {
        public const int ATTACKERS_ID = 0;
        public const int DEFENDERS_ID = 1;
    }

    public abstract class GamePhase
    {
        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }

    public enum GamePhaseType
    {
        InitPhase,
        BuyingPhase,
        PlantingPhase,
        DefusingPhase,
        RoundEndingPhase,
        GameEndingPhase,
    }

    public class GamePhasePatches
    {
        [HarmonyPatch(typeof(GameMode), nameof(GameMode.Method_Private_Void_0))]
        [HarmonyPostfix]
        static void OnGameModeMethod_Private_Void_0Post()
        {
            allPlayersSpawned = true;
        }

        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPrefix]
        public static bool ServerSendSendChatMessagePost(ulong __0, string __1)
        {
            if (!IsHost()) return true;

            if (IsGamePhase(GamePhaseType.BuyingPhase) || IsGamePhase(GamePhaseType.PlantingPhase) || IsGamePhase(GamePhaseType.DefusingPhase)) return false;

            return true;
        }

        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.LoadMap), [typeof(int), typeof(int)])]
        [HarmonyPrefix]
        static void OnServerSendLoadMapPre(ref int __0, ref int __1)
        {
            if (cggoEnabled)
            {
                if (__1 != WAITING_GAMEMODE_ID && __1 != TILE_DRIVE_GAMEMODE_ID) // Force TileDrive GameMode
                {
                    __0 = DEFAULT_TILE_DRIVE_MAP_ID;
                    __1 = TILE_DRIVE_GAMEMODE_ID;
                }
            }

            mapId = __0;
            gameModeId = __1;
        }

        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.PlayerDied))]
        [HarmonyPrefix]
        static void OnServerSendPlayerDiedPre(ref ulong __0, ref ulong __1)
        {
            if (!IsHost() || !cggoEnabled) return;

            CGGOPlayer victim = CGGOPlayer.GetCGGOPlayer(__0);
            if (victim == null || victim.Dead) return;

            ManagePlayerDied(victim);
            RecomputeAliveFlags();

            CGGOPlayer killer = ResolveKiller(victim);
            HandleAssists(victim, killer);
            HandleKill(killer, victim);

            victim.Assisters.Clear();

            if (allAttackersDead && IsGamePhase(GamePhaseType.PlantingPhase))
            {
                ManageRoundVictory(TeamsId.DEFENDERS_ID, TeamsId.ATTACKERS_ID);

                _instance.SetPhase(GamePhaseType.RoundEndingPhase);
            }
            if (allDefendersDead && (IsGamePhase(GamePhaseType.PlantingPhase) || IsGamePhase(GamePhaseType.DefusingPhase)))
            {
                ManageRoundVictory(TeamsId.ATTACKERS_ID, TeamsId.DEFENDERS_ID);

                _instance.SetPhase(GamePhaseType.RoundEndingPhase);
            }
        }
    }
    public class GamePhaseManager : MonoBehaviour
    {
        public static GamePhaseManager _instance { get; private set; }
        private GamePhase _currentPhase;
        private Dictionary<GamePhaseType, GamePhase> _phases;
        public static GamePhaseType _currentGamePhase = GamePhaseType.InitPhase;

        public static float elapsedShield = 0f;

        private void Awake()
        {
            if (!IsHost() || !MapsManager.isCGGO) return;

            if (!IsAlive(_instance))
                _instance = this;
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            _phases = new Dictionary<GamePhaseType, GamePhase>
            {
                { GamePhaseType.InitPhase,        new InitPhase() },
                { GamePhaseType.BuyingPhase,      new BuyingPhase() },
                { GamePhaseType.PlantingPhase,    new PlantingPhase() },
                { GamePhaseType.DefusingPhase,    new DefusingPhase() },
                { GamePhaseType.RoundEndingPhase, new RoundEndingPhase() },
                { GamePhaseType.GameEndingPhase,  new GameEndingPhase() }
            };

            SetPhase(GamePhaseType.InitPhase);
        }

        private static bool IsAlive(UnityEngine.Object obj)
        {
            try { return obj != null; }
            catch (UnhollowerBaseLib.ObjectCollectedException) { return false; }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(_instance, this))
                _instance = null;
        }

        private void Update()
        {
            if (!IsHost() || !MapsManager.isCGGO) return;
            // Code that should run every frame, regardless of the active phase
            GlobalUpdate();

            _currentPhase?.Update();  // Call Update on the active phase
        }

        private void GlobalUpdate()
        {
            elapsedShield += Time.deltaTime;

            SendShieldValue();
        }
        public void SetPhase(GamePhaseType newPhaseType)
        {
            _currentPhase?.Exit();  // Exit the current phase if it exists

            if (_phases.TryGetValue(newPhaseType, out var newPhase))
            {
                _currentPhase = newPhase;
                _currentGamePhase = newPhaseType;
                _currentPhase.Enter();  // Enter the new phase

                ForceMessage($"current phase : {_currentGamePhase}");
            }
        }
    }
}
