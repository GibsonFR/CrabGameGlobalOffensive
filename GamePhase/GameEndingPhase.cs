using static CGGO.GameEndingPhaseUtility;
using static CGGO.GameEndingPhaseConstants;
using static CGGO.GamePhaseUtility;

namespace CGGO
{
    public static class GameEndingPhaseConstants
    {
        public const float GAME_ENDING_DURATION = 5f;
    }
    public class GameEndingPhase : GamePhase
    {
        private float gameEndingElapsed = 0f;
        private bool lobbyLoaded;
        public override void Enter()
        {
            gameEndingElapsed = 0f;
        }

        public override void Update()
        {
            gameEndingElapsed += Time.deltaTime;

            if (gameEndingElapsed > GAME_ENDING_DURATION && !lobbyLoaded)
            {
                ResetGameVariables();
                LoadLobby();
                lobbyLoaded = true;
            }
        }

        public override void Exit()
        {

        }
    }
    public static class GameEndingPhaseUtility
    {
        public static void LoadLobby() => ServerSend.LoadMap(6, 0);
        
        public static void ResetGameVariables()
        {
            cggoPlayersList.Clear();
            attackersList.Clear();
            defendersList.Clear();
            attackersScore = 0;
            defendersScore = 0;
            attackersLoseStrike = 0;
            defendersLoseStrike = 0;
            totalTeamScore = 0;
            roundWinnerTeamId = -1;
            cggoTeamSet = false;
            allAttackersDead = false;
            allDefendersDead = false;
            ResetItemsOnMap();
        }
    }
}
