using static CGGO.PlantingPhaseUtility;
using static CGGO.MapsManager;
using static CGGO.PlantingPhaseConstants;
using static CGGO.PlantingPhase;

namespace CGGO
{
    public static class PlantingPhaseConstants
    {
        public const float PLANTING_PHASE_DURATION = 35f;
        public const float MESSAGE_DELAY = 1f;
    }
    public class PlantingPhase : GamePhase
    {
        public static float plantingPhaseElapsed = 0f;
        private float messageElapsed = 0f;
        public override void Enter()
        {
            plantingPhaseElapsed = 0f;
        }

        public override void Update()
        {
            plantingPhaseElapsed += Time.deltaTime;
            messageElapsed += Time.deltaTime;

            if (plantingPhaseElapsed >= PLANTING_PHASE_DURATION) 
            {
                ManageRoundVictory(TeamsId.DEFENDERS_ID, TeamsId.ATTACKERS_ID);

                GamePhaseManager._instance.SetPhase(GamePhaseType.RoundEndingPhase);
            }
            else if (messageElapsed >= MESSAGE_DELAY)
            {
                messageElapsed = 0f;
                SendPlantingPhaseMessage();
            }
        }

        public override void Exit()
        {

        }
    }

    public class PlantingPatches
    {
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.DropItem))]
        [HarmonyPostfix]
        public static void OnServerSendDropItemPost(ulong __0, int __1, int __2, int __3)
        {
            ulong steamId = __0;
            int itemId = __2;

            if (!IsGamePhase(GamePhaseType.PlantingPhase)) return;

            if (itemId == bombId) 
            {
                Vector3 bombHandlerPosition = activePlayers[__0].transform.position;

                bool isOnSite = false;

                if (IsPointOnSite(bombHandlerPosition + new Vector3(0, 1.5f, 0), currentCGGOMap.MilkZoneAcorner1, currentCGGOMap.MilkZoneAcorner2, -0.5f))
                {
                    siteId = 0;
                    isOnSite = true;
                }
                else if (IsPointOnSite(bombHandlerPosition + new Vector3(0, 1.5f, 0), currentCGGOMap.MilkZoneBcorner1, currentCGGOMap.MilkZoneBcorner2, -0.5f))
                {
                    siteId = 1;
                    isOnSite = true;
                }

                if (isOnSite)
                {
                    SetBombPosition(bombHandlerPosition, siteId);
                    SpawnPlantedBomb(siteId);
                    itemToDelete.Add(bombId, DateTime.Now);
                    GivePlantingBounty(attackersList);
                    GamePhaseManager._instance.SetPhase(GamePhaseType.DefusingPhase);
                }


            }
        }
    }

    public static class PlantingPhaseUtility
    {
        public static void SendPlantingPhaseMessage()
        {
            foreach (var player in cggoPlayersList)
            {
                if (player.Team == TeamsId.ATTACKERS_ID)
                {
                    SendPrivateMessage(player.SteamId, $"\n");
                    SendPrivateMessage(player.SteamId, $"--- P L A N T I N G --- P H A S E ---");
                    SendPrivateMessage(player.SteamId, $"You are an ATTACKER!\n" +
                        $"You have {PLANTING_PHASE_DURATION - (int)plantingPhaseElapsed} secondes to plant the Bomb!\n");
                    SendPrivateMessage(player.SteamId, $"Drop the Bomb on a MilkZone to plant!\n" +
                        $"Check your settings to find the drop key!\n");
                }
                else
                {
                    SendPrivateMessage(player.SteamId, $"\n");
                    SendPrivateMessage(player.SteamId, $"--- P L A N T I N G --- P H A S E ---");
                    SendPrivateMessage(player.SteamId, $"You are a DEFENDER!\n" +
                        $"{PLANTING_PHASE_DURATION - (int)plantingPhaseElapsed}s left to stop the planting of the Bomb!\n");
                    SendPrivateMessage(player.SteamId, $"Kill the attackers with your weapons!\n" +
                        $"Before they can reach the site!\n");
                }
            }
        }
        public static void SpawnPlantedBomb(int site)
        {
            Vector3 dummyPos = Vector3.zero;
            if (site == 0) dummyPos = new Vector3(bombPos.x, currentCGGOMap.MilkZoneAcorner1.y - 3, bombPos.z);
            else dummyPos = new Vector3(bombPos.x, currentCGGOMap.MilkZoneBcorner1.y - 3, bombPos.z);

            sharedObjectId++;
            bombDummyId = sharedObjectId;

            CreateDummy(dummyPos, bombDummyId);
            ServerSend.PlayerRotation((ulong)bombDummyId, 0, -180);
            ServerSend.PlayerActiveItem((ulong)bombDummyId, 5);
            ServerSend.PlayerAnimation((ulong)bombDummyId, 0, true);
        }
        public static void SetBombPosition(Vector3 bombHandlerPosition, int siteId)
        {
            if (siteId == 0) bombPos = new Vector3(bombHandlerPosition.x, currentCGGOMap.MilkZoneAcorner1.y - 1.5f, bombHandlerPosition.z);
            else bombPos = new Vector3(bombHandlerPosition.x, currentCGGOMap.MilkZoneBcorner1.y - 1.5f, bombHandlerPosition.z);
        }

        public static bool IsPointOnSite(Vector3 p, Vector3 c1, Vector3 c2, float margin = 0f)
        {
            return p.x >= Mathf.Min(c1.x, c2.x) - margin       &&
                   p.x <= Mathf.Max(c1.x, c2.x) + margin       &&
                   p.y >= Mathf.Min(c1.y, c2.y) - 1            &&
                   p.y <= Mathf.Max(c1.y, c2.y) + 1            &&
                   p.z >= Mathf.Min(c1.z, c2.z) - margin       &&
                   p.z <= Mathf.Max(c1.z, c2.z) + margin;
        }
    }
}
