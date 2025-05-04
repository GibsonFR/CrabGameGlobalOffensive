using static CGGO.EconomyUtility;
using static CGGO.DefusingPhaseUtility;
using static CGGO.DefusingPhaseConstants;
using static CGGO.DefusingPhase;
using UnhollowerBaseLib;

namespace CGGO
{
    public static class DefusingPhaseConstants
    {
        public const float DEFUSING_PHASE_DURATION = 35f;
        public const float DEFUSING_DELAY = 5f;
        public const float DEFUSING_RANGE = 2.5f;
        public const float EXPLOSION_RANGE = 30f;
        public const float MESSAGE_DELAY = 1f;
        public const int MAX_COLLIDERS = 32;
    }

    public class DefusingPhase : GamePhase
    {
        public static float elapsedDefusingPhaseTime = 0f;
        public static float elapsedDefuseTime = 0f;
        private static float elapsedMessageTime = 0f;

        static readonly Il2CppReferenceArray<Collider> s_hitBuffer =
            new Il2CppReferenceArray<Collider>(MAX_COLLIDERS);

        private LayerMask playerLayer;

        public override void Enter()
        {
            elapsedDefusingPhaseTime = 0f;
            elapsedDefuseTime = 0f;
            playerLayer = LayerMask.GetMask("Player");
        }

        public override void Update()
        {
            elapsedDefusingPhaseTime += Time.deltaTime;
            elapsedMessageTime += Time.deltaTime;

            if (elapsedDefusingPhaseTime > DEFUSING_PHASE_DURATION)
            {
                ManageRoundVictory(TeamsId.ATTACKERS_ID, TeamsId.DEFENDERS_ID);

                RemovePlantedBomb();

                GamePhaseManager._instance.SetPhase(GamePhaseType.RoundEndingPhase);

                ManageBombExplosion();
                return;
            }

            PlayerManager validDefuser = GetFirstValidDefuser();

            elapsedDefuseTime = validDefuser != null
                                ? elapsedDefuseTime + Time.deltaTime
                                : 0f;

            if (elapsedMessageTime >= MESSAGE_DELAY)
            {
                elapsedMessageTime = 0f;
                if (elapsedDefuseTime != 0) SendDefusingMessage();
                else SendDefusingPhaseMessage();
            }

            if (elapsedDefuseTime > DEFUSING_DELAY)
            {
                ManageRoundVictory(TeamsId.DEFENDERS_ID, TeamsId.ATTACKERS_ID);

                RemovePlantedBomb();

                GamePhaseManager._instance.SetPhase(GamePhaseType.RoundEndingPhase);
            }
        }

        public override void Exit() { }

        private PlayerManager GetFirstValidDefuser()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                              bombPos,
                              DEFUSING_RANGE,
                              s_hitBuffer,
                              playerLayer);

            for (int i = 0; i < hitCount; i++)
            {
                try
                {
                    PlayerManager pm = s_hitBuffer[i].GetComponentInParent<PlayerManager>();
                    if (IsValidDefuser(pm)) return pm;
                }
                catch { }
            }

            return null;
        }

        private static bool IsValidDefuser(PlayerManager playerManager)
        {
            if (playerManager == null) return false;

            CGGOPlayer cggoPl = CGGOPlayer.GetCGGOPlayer(playerManager.steamProfile.m_SteamID);
            if (cggoPl == null || cggoPl.Team == TeamsId.ATTACKERS_ID) return false;

            if (cggoPl.SteamId == clientId)
            {
                if (!__PlayerMovement.field_Private_Boolean_1) return false;

                if (__PlayerInventory == null ||
                __PlayerInventory.currentItem == null ||
                __PlayerInventory.currentItem.name != "Snowball(Clone)")
                    return false;

                return true;

            }
            else
            {
                if (!playerManager.field_Private_MonoBehaviourPublicObVeSiVeRiSiAnVeanTrUnique_0.field_Private_Boolean_0 || playerManager.itemHandle == null) return false;

                return playerManager.itemHandle.field_Private_MonoBehaviour1PublicAbstractItitBoGapiTrrileTrObUnique_0.name == "Snowball(Clone)";

            }
        }
    }

    public static class DefusingPhaseUtility
    {
        private static readonly Vector3 RespawnVoid = new(0f, 9.9e13f, 0f);

        public static void SendDefusingPhaseMessage()
        {
            foreach (var player in cggoPlayersList)
            {
                if (player.Team == TeamsId.ATTACKERS_ID)
                {
                    SendPrivateMessage(player.SteamId, $"\n");
                    SendPrivateMessage(player.SteamId, $"--- D E F U S I N G --- P H A S E ---");
                    SendPrivateMessage(player.SteamId, $"You are an ATTACKER!\n" +
                                                       $"Protect the Bomb for {DEFUSING_PHASE_DURATION - (int)elapsedDefusingPhaseTime} secondes!\n");
                    SendPrivateMessage(player.SteamId, $"Defusing the bomb take 5 secondes!\n" +
                        $"Dont let defender reaching it!\n");
                }
                else
                {
                    SendPrivateMessage(player.SteamId, $"\n");
                    SendPrivateMessage(player.SteamId, $"--- D E F U S I N G --- P H A S E ---");
                    SendPrivateMessage(player.SteamId, $"You are a DEFENDER!\n" +
                                                       $"You have {DEFUSING_PHASE_DURATION - (int)elapsedDefusingPhaseTime}s to defuse the Bomb!");
                    SendPrivateMessage(player.SteamId, $"To defuse the bomb, reach it on the MilkZone\n" +
                                                       $"Press 4 to grab snowball and crouch!\n");
                }
            }
        }

        public static void SendDefusingMessage()
        {
            string defuseProgression = "";
            switch ((int)elapsedDefuseTime)
            {
                case 0:
                    defuseProgression = "D E F U S I N G [_ _ _ _ _]";
                    break;
                case 1:
                    defuseProgression = "D E F U S I N G [X _ _ _ _]";
                    break;
                case 2:
                    defuseProgression = "D E F U S I N G [X X _ _ _]";
                    break;
                case 3:
                    defuseProgression = "D E F U S I N G [X X X _ _]";
                    break;
                case 4:
                    defuseProgression = "D E F U S I N G [X X X X _]";
                    break;
                case 5:
                    defuseProgression = "D E F U S I N G [X X X X X]";
                    break;
            }

            SendServerMessage($"\n-\n-\n-\n-\n{defuseProgression}\n-\n-\n-\n-");
        }


        public static void ManageBombExplosion()
        {
            float rangeSqr = EXPLOSION_RANGE * EXPLOSION_RANGE;

            foreach (var cggoPlayer in cggoPlayersList)
            {
                if (cggoPlayer.Dead) continue;
                if (!activePlayers.TryGetValue(cggoPlayer.SteamId, out var pm)) continue;
                if ((pm.transform.position - bombPos).sqrMagnitude > rangeSqr) continue;

                ulong id = cggoPlayer.SteamId;
                ServerSend.RespawnPlayer(id, RespawnVoid);
                ServerSend.PlayerDied(id, id, Vector3.zero);
            }
        }

        public static void RemovePlantedBomb() =>
            ServerSend.PlayerActiveItem((ulong)bombDummyId, 14);
    }
}
