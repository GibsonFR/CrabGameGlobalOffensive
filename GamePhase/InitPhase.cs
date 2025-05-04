using static CGGO.InitPhaseConstants;
using static CGGO.InitPhaseUtility;

namespace CGGO
{
    public static class InitPhaseConstants
    {
        public const int MAX_ROUND_DURATION = 100;
    }
    public class InitPhase : GamePhase
    {
        public override void Enter()
        {
        }

        public override void Update()
        {
            if (!allPlayersSpawned) return; 
            SetGameTime(MAX_ROUND_DURATION);
            MakeCGGOTeam();
            ResetPlayersRoundStats();
            ResetItemsOnMap();
            totalTeamScore = defendersScore + attackersScore;
            if (totalTeamScore == 5) TeamSwitch();

            GamePhaseManager._instance.SetPhase(GamePhaseType.BuyingPhase);
        }

        public override void Exit()
        {
            SetGameStateFreeze();
        }
    }

    public static class InitPhaseUtility
    {
        public static void SetGameStateFreeze() => GameManager.Instance.gameMode.modeState = GameMode.EnumNPublicSealedvaFrPlEnGa5vUnique.Freeze;
        public static void SetGameTime(int time)
        {
            GameManager.Instance.gameMode.SetGameModeTimer(time, 1);
        }
        public static void MakeCGGOTeam()
        {
            if (cggoTeamSet) return;

            List<PlayerManager> validPlayers = [];

            foreach (var player in activePlayers)
            {
                validPlayers.Add(player.Value);
            }

            List<CGGOPlayer> teamAttackers = [];
            List<CGGOPlayer> teamDefenders = [];

            var shuffledPlayers = validPlayers.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < shuffledPlayers.Count; i++)
            {
                int teamId = (i % 2 == 0) ? 0 : 1;
                if (teamId == 0) teamAttackers.Add(new CGGOPlayer(shuffledPlayers[i], teamId));
                else teamDefenders.Add(new CGGOPlayer(shuffledPlayers[i], teamId));
            }

            cggoPlayersList.AddRange(teamAttackers);
            cggoPlayersList.AddRange(teamDefenders);

            attackersList = teamAttackers;
            defendersList = teamDefenders;

            cggoTeamSet = true;
        }

        public static void ResetPlayersRoundStats()
        {
            foreach (var player in cggoPlayersList)
            {
                player.Assisters = [];
                player.Dead = false;
                player.Killer = 0;
                player.DamageTaken = 0;
            }
        }
        public static void TeamSwitch()
        {       
            int oldAttackersScore = attackersScore;
            int oldDefendersScore = defendersScore;
            attackersScore = oldDefendersScore;
            defendersScore = oldAttackersScore;
            attackersLoseStrike = 0;
            defendersLoseStrike = 0;

            attackersList = [];
            defendersList = [];

            foreach (var player in cggoPlayersList)
            {
                if (player.Team == 0)
                {
                    player.Team = 1;
                    defendersList.Add(player);
                }
                else
                {
                    player.Team = 0;
                    attackersList.Add(player);
                }

                player.Balance = 0;
                player.Shield = 0;
                player.Katana = false;
                player.Pistol = false;
                player.Shotgun = false;
                player.Rifle = false;
                player.Revolver = false;
            } 
        }
    }
}
