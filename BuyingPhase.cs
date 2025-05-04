using static CGGO.BuyingPhaseConstants;
using static CGGO.BuyingPhaseUtility;
using static CGGO.BuyingPhase;
using static CGGO.MapsManager;
using static CGGO.MilkZoneUtility;

namespace CGGO
{
    public static class BuyingPhaseConstants
    {
        public const float BUYING_PHASE_DURATION = 15f;
        public const float MESSAGE_DELAY = 1f;
        public const int RED_COLOR_ID = 0;
        public const int GREEN_COLOR_ID = 2;
    }
    public class BuyingPhase : GamePhase
    {
        private float elapsedBuyPhase;
        private float elapsedMessageTime = 0f;
        private bool bombSpawnerRemoved;
        public static ulong bombSpawnerId;
        public override void Enter()
        {
            elapsedMessageTime = 0f;

            __GameModeTileDrive = __GameManager.GetComponent<GameModeTileDrive>();

            if (totalTeamScore == 0 || totalTeamScore == 5) GiveStartingMoney(cggoPlayersList);

            ManageAfkBonus();
            CapPlayerBalances();
            ColorTeams();
            SpawnBomb();
            GiveLastRoundWeapons();
            GiveDefusers();

        }

        public override void Update()
        {
            elapsedBuyPhase += Time.deltaTime;
            elapsedMessageTime += Time.deltaTime;

            if (elapsedBuyPhase < BUYING_PHASE_DURATION)
            {
                ManageMilkZone();
                SpawnTeams();

                if (elapsedBuyPhase > 1 && !bombSpawnerRemoved)
                {
                    SendPlayerIntoVoid(bombSpawnerId);
                    bombSpawnerRemoved = true;
                }
            }
            else GamePhaseManager._instance.SetPhase(GamePhaseType.PlantingPhase);

            if (elapsedMessageTime >= MESSAGE_DELAY)
            {
                elapsedMessageTime = 0f;
                SendBuyingPhaseMessage();
            }
        }

        public override void Exit()
        {

        }
    }
    public static class BuyingPhaseUtility
    {
        public static void ManageAfkBonus()
        {
            int playersDelta = attackersList.Count - defendersList.Count;
            if (playersDelta == 0) return;

            if (playersDelta > 0) ApplyAfkBonus(defendersList, playersDelta);
            else if (playersDelta < 0) ApplyAfkBonus(attackersList, -playersDelta);         
        }

        public static void ApplyAfkBonus(List<CGGOPlayer> players, int afkCount)
        {
            foreach (var player in players)
            {
                if (totalTeamScore == 0 || totalTeamScore == 5) player.Shield = WeaponsUtility.CalculateShieldBonus(players.Count, afkCount);
                else player.Balance += CalculateAFKBonus(players.Count, afkCount);
            }
        }
        public static void SendBuyingPhaseMessage()
        {
            foreach (var player in cggoPlayersList)
            {
                SendPrivateMessage(player.SteamId, "\n" +
                    $"--- B U Y --- P H A S E ---\n" +
                    $"ROUND {totalTeamScore + 1} | SCORE: {attackersScore} - {defendersScore}");
                SendPrivateMessage(player.SteamId, $"Your Money : {player.Balance}$\n" +
                                                   $"To buy, type in chat the following commands:");
                SendPrivateMessage(player.SteamId, $"/classic (900$) - /shorty (1 850$)");
                SendPrivateMessage(player.SteamId, $"/vandal (2 900$) - /revolver (4 700$)");
                SendPrivateMessage(player.SteamId, $"/katana (3 200$)");
                SendPrivateMessage(player.SteamId, $"/shield25 (400 $) - /shield50 (1 000$)");
  
            }
        }
        public static void GiveDefusers()
        {
            foreach (var player in cggoPlayersList)
            {
                try
                {
                    sharedObjectId++;
                    if (player.Team == 1) GameServer.ForceGiveWeapon(player.SteamId, 9, sharedObjectId);                    
                }
                catch { }
            }
        }
        public static void SpawnBomb()
        {
            try
            {
                sharedObjectId++;
                bombSpawnerId = (ulong)sharedObjectId;


                CreateDummy(currentCGGOMap.BombSpawnPosition, (int)bombSpawnerId);
                try
                {
                    sharedObjectId++;
                    bombId = sharedObjectId;
                    ServerSend.DropItem(bombSpawnerId, 5, bombId, 0);
                }
                catch { }
            }
            catch { }
        }

        public static void SpawnTeams()
        {
            int teamAttackersCount = 0, teamDefendersCount = 0;
            Vector3 spawnPosition = Vector3.zero;

            foreach (var player in cggoPlayersList)
            {
                try
                {
                    if (player.Team == TeamsId.ATTACKERS_ID)
                    {
                        spawnPosition = currentCGGOMap.SpawnTeamAttackers + currentCGGOMap.SpawnDirectionTeamAttackers * teamAttackersCount;
                        teamAttackersCount++;
                    }
                    else if (player.Team == TeamsId.DEFENDERS_ID)
                    {
                        spawnPosition = currentCGGOMap.SpawnTeamDefenders + currentCGGOMap.SpawnDirectionTeamDefenders * teamDefendersCount;
                        teamDefendersCount++;
                    }
                    ServerSend.RespawnPlayer(player.SteamId, spawnPosition);
                }
                catch { }
            }
        }

        public static void ColorTeams()
        {
            int redColorId = RED_COLOR_ID;
            int greenColorId = GREEN_COLOR_ID; 

            foreach (var player in cggoPlayersList)
            {
                try
                {
                    if (player.Team == TeamsId.ATTACKERS_ID)
                    {
                        try
                        {
                            __GameModeTileDrive?.MakeTeam(player.SteamId, redColorId);
                        }
                        catch { }
                    }
                    else if (player.Team == TeamsId.DEFENDERS_ID)
                    {
                        try
                        {
                            __GameModeTileDrive?.MakeTeam(player.SteamId, greenColorId);
                        }
                        catch { }
                    }
                }
                catch { }
            }

            try
            {
                __GameModeTileDrive?.SendTeam();
            }
            catch { }
        }
    }
}
