using static CGGO.MapsConstants;
using static CGGO.MapsUtility;
using static CGGO.MapsManager;

namespace CGGO
{
    public static class MapsConstants
    {
        public static readonly int[] CGGO_MAP_IDS = [0, 2, 7, 20];
        public static readonly int[] TILE_DRIVE_CLASSIC_MAP_IDS = [30, 31];
    }

    public class MapsManager : MonoBehaviour
    {
        public static bool isTileDriveClassicMap = false, isCGGO = false;
        public static int currentCGGOMapId = -1;
        public static Map currentCGGOMap = null;

        private bool hasActed = false;
        void Awake()
        {
            if (!IsHost()) return;

            if (gameModeId != 9 || !cggoEnabled) return; // Exit early if the modId isnt Tile Drive

            isTileDriveClassicMap = TILE_DRIVE_CLASSIC_MAP_IDS.Contains(mapId);
            isCGGO = CGGO_MAP_IDS.Contains(mapId);
        }

        void Update()
        {
            if (!IsHost() || !cggoEnabled) return;

            if (!allPlayersSpawned) return;

            else if (!hasActed)
            {
                if (gameModeId == 0)
                {
                    currentCGGOMapId = -1;
                    hasActed = true;
                }
                else if (isTileDriveClassicMap)
                {
                    currentCGGOMapId = SelectRandomMap(CGGO_MAP_IDS);
                    SetCurrentMap(currentCGGOMapId);

                    ServerSend.LoadMap(currentCGGOMapId, 9);

                    hasActed = true;
                }
            }

        }
    }

    public class MapsUtility
    {
        public static int SelectRandomMap(int[] mapsId)
        {
            Il2CppSystem.Random random = new();
            return mapsId[random.Next(0, mapsId.Length)];
        }
        public static void SetCurrentMap(int mapId)
        {
            switch (mapId)
            {
                case 0:
                    currentCGGOMap = new MapBitterBeach();
                    break;
                case 2:
                    currentCGGOMap = new MapCockyContainers();
                    break;
                case 7:
                    currentCGGOMap = new MapFunkyField();
                    break;
                case 20:
                    currentCGGOMap = new MapReturnToMonke();
                    break;
            }
        }
    }

    public class MapBitterBeach : Map
    {
        public override int MapId => 0;
        public override Vector3 BombSpawnPosition => new(44.6f, 30f, -32f);
        public override Vector3 MilkZoneAcorner1 => new(-25.2f, 1.9f, -19.2f);
        public override Vector3 MilkZoneAcorner2 => new(-38.8f, 6f, -32.8f);
        public override Vector3 MilkZoneBcorner1 => new(-20.0f, -4.1f, 36.3f);
        public override Vector3 MilkZoneBcorner2 => new(3.8f, 2.6f, 51.8f);
        public override Vector3 SpawnTeamAttackers => new(49.0f, -4.1f, -41.0f);
        public override Vector3 SpawnDirectionTeamAttackers => new(-3, 0, 0);
        public override Vector3 SpawnTeamDefenders => new(-43.0f, -4.1f, 51.0f);
        public override Vector3 SpawnDirectionTeamDefenders => new(0, 0, -3);

    }

    public class MapCockyContainers : Map
    {
        public override int MapId => 2;
        public override Vector3 BombSpawnPosition => new(36.6f, -10f, 24f);
        public override Vector3 MilkZoneAcorner1 => new(36f, -25.1f, -10f);
        public override Vector3 MilkZoneAcorner2 => new(11.5f, -18.7f, -32f);
        public override Vector3 MilkZoneBcorner1 => new(-34.0f, -25.1f, 6f);
        public override Vector3 MilkZoneBcorner2 => new(-48f, -18.7f, 47f);
        public override Vector3 SpawnTeamAttackers => new(44f, -25.1f, 16.0f);
        public override Vector3 SpawnDirectionTeamAttackers => new(0, 0, 3);
        public override Vector3 SpawnTeamDefenders => new(-47.0f, -25.1f, 2.0f);
        public override Vector3 SpawnDirectionTeamDefenders => new(0, 0, -3);

    }

    public class MapReturnToMonke : Map
    {
        public override int MapId => 20;
        public override Vector3 BombSpawnPosition => new(51f, 15f, -20f);
        public override Vector3 MilkZoneAcorner1 => new(-49f, -5.1f, -21f);
        public override Vector3 MilkZoneAcorner2 => new(-33.5f, 0f, -7f);
        public override Vector3 MilkZoneBcorner1 => new(-38f, -1.1f, 11f);
        public override Vector3 MilkZoneBcorner2 => new(-17f, 0.3f, 33f);
        public override Vector3 SpawnTeamAttackers => new(54.0f, -5.1f, -31.0f);
        public override Vector3 SpawnDirectionTeamAttackers => new(0, 0, 3);
        public override Vector3 SpawnTeamDefenders => new(-48f, -5.1f, 49f);
        public override Vector3 SpawnDirectionTeamDefenders => new(0, 0, -3);

    }

    public class MapFunkyField : Map
    {
        public override int MapId => 7;
        public override Vector3 BombSpawnPosition => new(-47f, -5.8f, 3f);
        public override Vector3 MilkZoneAcorner1 => new(20.8f, -8.1f, -24.8f);
        public override Vector3 MilkZoneAcorner2 => new(41.2f, -5.7f, -41.2f);
        public override Vector3 MilkZoneBcorner1 => new(13.8f, -23.3f, 50f);
        public override Vector3 MilkZoneBcorner2 => new(-13.8f, -21.8f, 25.5f);
        public override Vector3 SpawnTeamAttackers => new(-48.0f, -23.1f, 11f);
        public override Vector3 SpawnDirectionTeamAttackers => new(0, 0, 3);
        public override Vector3 SpawnTeamDefenders => new(31.4f, -23.1f, 35.0f);
        public override Vector3 SpawnDirectionTeamDefenders => new(0, 0, -3);
    }
    public class MapSnowTop : Map
    {
        public override int MapId => 29;
        public override Vector3 BombSpawnPosition => new(-7f, 70f, -33f);
        public override Vector3 MilkZoneAcorner1 => new(31.8f, 73.9f, -46.8f);
        public override Vector3 MilkZoneAcorner2 => new(54.1f, 77f, -27.2f);
        public override Vector3 MilkZoneBcorner1 => new(-23.5f, 56.9f, 25f);
        public override Vector3 MilkZoneBcorner2 => new(-8.5f, 60.9f, 31.5f);
        public override Vector3 SpawnTeamAttackers => new(-19.0f, 55.9f, -40.0f);
        public override Vector3 SpawnDirectionTeamAttackers => new(3, 0, 0);
        public override Vector3 SpawnTeamDefenders => new(-39.0f, 69.9f, 46.0f);
        public override Vector3 SpawnDirectionTeamDefenders => new(3, 0, 0);
    }
    public abstract class Map
    {
        public abstract int MapId { get; } //Map Id
        public abstract Vector3 BombSpawnPosition { get; } //Define the spawn position of the spike (close to Attackers) add +20 to y
        public abstract Vector3 MilkZoneAcorner1 { get; } //MUST Always define the ground level very important !!  
        public abstract Vector3 MilkZoneAcorner2 { get; } //MUST Always define the max height of the zone very important !! (for exemple to avoid spawning spike on box)
        public abstract Vector3 MilkZoneBcorner1 { get; } //Same condition as MilkZoneA
        public abstract Vector3 MilkZoneBcorner2 { get; } //Same condition as MilkZoneA
        public abstract Vector3 SpawnTeamAttackers { get; } //Define where the first attacker spawn
        public abstract Vector3 SpawnDirectionTeamAttackers { get; } //Define the direction in which other attackers will spawn relative to first attacker
        public abstract Vector3 SpawnTeamDefenders { get; } //Define where the first defender spawn
        public abstract Vector3 SpawnDirectionTeamDefenders { get; } //Define the direction in which other defenders will spawn relative to first defender
    }
}
