using static CGGO.RoundEndingPhaseUtility;
using static CGGO.RoundEndingPhaseConstants;

namespace CGGO
{
    public static class RoundEndingPhaseConstants
    {
        public const float ROUND_ENDING_DURATION = 5f;
    }
    public class RoundEndingPhase : GamePhase
    {
        private float roundEndingElapsed = 0f;
        private bool nextRoundLoaded = false;
        public override void Enter()
        {
            roundEndingElapsed = 0f;

            if (roundWinnerTeamId == TeamsId.ATTACKERS_ID) SendAttackersWonMessage();
            else SendDefendersWonMessage();

        }

        public override void Update()
        {
            roundEndingElapsed += Time.deltaTime;

            if (roundEndingElapsed < ROUND_ENDING_DURATION) return;

            if (attackersScore >= 6)  
            {
                GamePhaseManager._instance.SetPhase(GamePhaseType.GameEndingPhase);
            }
            else if (defendersScore >= 6)
            {
                GamePhaseManager._instance.SetPhase(GamePhaseType.GameEndingPhase);
            }
            else if (!nextRoundLoaded) 
            {
                nextRoundLoaded = true;

                ResetRoundVariables();
                ReloadCGGOMap();
            }
        }

        public override void Exit()
        {

        }
    }
    public static class RoundEndingPhaseUtility
    {
        public static void ResetRoundVariables()
        {
            allAttackersDead = false;
            allDefendersDead = false;
        }

        public static void ReloadCGGOMap()
        {
            try
            {
                ServerSend.LoadMap(MapsManager.currentCGGOMapId, 9);
            }
            catch
            {
                ServerSend.LoadMap(mapId, 9);
            }
        }
        public static void SendAttackersWonMessage()
        {
            SendScoreBoardAndSpecialMessage("---- A T T A C K E R S ---- W O N ----");
        }
        public static void SendDefendersWonMessage()
        {
            SendScoreBoardAndSpecialMessage("---- D E F E N D E R S ---- W O N ----");
        }
        public static void SendScoreBoardAndSpecialMessage(string specialMessage) 
        {
            List<string> messageList = [];
            List<CGGOPlayer> topAttackersPlayers = attackersList.OrderByDescending(p => p.Kills).Take(6).ToList();
            List<CGGOPlayer> topDefendersPlayers = defendersList.OrderByDescending(p => p.Kills).Take(6).ToList();

            while (topAttackersPlayers.Count < 6) topAttackersPlayers.Add(null);
            while (topDefendersPlayers.Count < 6) topDefendersPlayers.Add(null);

            static string AdjustName(string name, int maxLength)
            {
                if (name.Length > maxLength)
                    return name.Substring(0, maxLength - 3) + "...";

                var adjustedName = name;
                while (adjustedName.Length < maxLength)
                {
                    adjustedName += "- ";
                }

                adjustedName = adjustedName.Replace("-----", "- - -");

                return adjustedName;
            }

            int maxNameLength = 12;

            for (int i = 0; i < 6; i++)
            {
                string message = "";

                if (topAttackersPlayers[i] != null)
                {
                    var playerA = topAttackersPlayers[i];
                    string playerNameA = AdjustName(playerA.Username, maxNameLength);
                    message += $"{playerNameA} {playerA.Kills}/{playerA.Deaths}/{playerA.Assists}";
                }
                else
                {
                    message += " - - - - - - - - - - - - - -";
                }

                message += " | ";

                if (topDefendersPlayers[i] != null)
                {
                    var playerB = topDefendersPlayers[i];
                    string playerNameB = AdjustName(playerB.Username, maxNameLength);
                    message += $"{playerNameB} {playerB.Kills}/{playerB.Deaths}/{playerB.Assists}";
                }
                else
                {
                    message += "- - - - - - - - - - - - - -";
                }
                messageList.Add(message);
            }

            SendServerMessage("#");
            SendServerMessage(specialMessage);
            SendServerMessage($"- ATTACKERS[{attackersScore}] | [{defendersScore}]DEFENDERS -");
            SendServerMessage(messageList[0]);
            SendServerMessage(messageList[1]);
            SendServerMessage(messageList[2]);
            SendServerMessage(messageList[3]);
            SendServerMessage(messageList[4]);

        }
    }
}
