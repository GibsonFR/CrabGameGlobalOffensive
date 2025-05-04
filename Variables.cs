namespace CGGO
{
    public static class Variables
    {
        // folder
        public static string assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string defaultFolderPath = assemblyFolderPath + "\\";
        public static string mainFolderPath = defaultFolderPath + @"CGGO\";
        public static string playersDataFolderPath = mainFolderPath + @"PlayersData\";

        // file
        public static string logFilePath = mainFolderPath + "log.txt";
        public static string playersDataFilePath = playersDataFolderPath + "database.txt";
        public static string configFilePath = mainFolderPath + "config.txt";

        // Dictionnary
        public static Dictionary<ulong, PlayerManager> activePlayers = [];
        public static Dictionary<ulong, KeyValuePair<ulong, DateTime>> hitPlayersDico = [];
        public static Dictionary<int, DateTime> itemToDelete = [];

        // List
        public static List<CGGOPlayer> cggoPlayersList = [], attackersList = [], defendersList = [];
        public static List<int> pistolList = [],
                                shotgunList = [],
                                rifleList = [],
                                katanaList = [],
                                revolverList = [];

        // Client Script Instancies
        public static GameObject clientObject;
        public static Rigidbody clientBody;
        public static Camera clientCamera;
        public static PlayerInventory __PlayerInventory;
        public static PlayerMovement __PlayerMovement;

        // Game Script Instancies
        public static GameManager __GameManager;
        public static SteamManager __SteamManager;
        public static GameModeTileDrive __GameModeTileDrive;

        // Vector3
        public static Vector3 clientPosition, lastTargetOnlinePlayerPosition, bombPos;

        // string
        public static string menuKey;

        // ulong
        public static ulong clientId, hostId, lobbyId;

        // int
        public static int mapId = 6, gameModeId = 0, totalTeamScore, defendersScore, attackersScore, defendersLoseStrike, attackersLoseStrike, sharedObjectId = 5000, bombId, originalBombId, siteId, roundWinnerTeamId = -1, bombDummyId;

        // bool
        public static bool hostAfk = true, configOnStart, menuTrigger, snowballTrajectory, cggoTeamSet,  allPlayersSpawned, cggoEnabled = true, allAttackersDead, allDefendersDead;
    }
}
