using static CGGO.MilkZoneUtility;
using static CGGO.MilkZoneManager;
using static CGGO.MapsManager;

namespace CGGO
{
    public class MilkZoneManager : MonoBehaviour
    {
        public static float elapsedMilkZone = 0f;
        public static bool milkZoneA, milkZoneB;

        void Awake()
        {
            elapsedMilkZone = 0f;
            perimeterCurrentSegment = 0;    
            perimeterCurrentDistance = 0;
            dummyPerimeterId = 0;
            heightOffset = 0;
            perimeterCompleted = false;
            perimeterInit = false;
            milkZoneA = false;
            milkZoneB = false;
        }
        void Update()
        {
            elapsedMilkZone += Time.deltaTime;
        }
    }
    public class MilkZoneUtility
    {
        public static Vector3[] perimeterExpandedCorners; // The corners after margin addition
        public static int perimeterCurrentSegment = 0;    // Current segment
        public static float perimeterCurrentDistance = 0; // Distance traveled on the current segment
        public static Vector3 perimeterCurrentPosition;   // Current position on the perimeter
        public static bool perimeterCompleted, perimeterInit; // Completed round indicator
        public static int dummyPerimeterId = 0; // Dummy ID for perimeter object
        public static float heightOffset = 0; // Height offset for item placement
        public static void InitPerimeter(Vector3 corner1, Vector3 corner2)
        {
            // Calculate the cuboid corners with the margin
            perimeterExpandedCorners = new Vector3[4];
            perimeterExpandedCorners[0] = corner1;
            perimeterExpandedCorners[1] = new Vector3(corner2.x, corner1.y, corner1.z);
            perimeterExpandedCorners[2] = corner2;
            perimeterExpandedCorners[3] = new Vector3(corner1.x, corner2.y, corner2.z);
            perimeterCurrentPosition = perimeterExpandedCorners[0];
            perimeterInit = true;
            perimeterCurrentSegment = 0; // Reset segment
            perimeterCurrentDistance = 0; // Reset distance
            perimeterCompleted = false;
            dummyPerimeterId = 0;

            sharedObjectId++;
            dummyPerimeterId = sharedObjectId;
            CreateDummy(perimeterCurrentPosition, dummyPerimeterId);
        }

        public static void MoveAlongPerimeter()
        {
            if (perimeterExpandedCorners == null || perimeterExpandedCorners.Length < 4)
            {
                return;
            }

            // Calculate the current segment
            Vector3 start = perimeterExpandedCorners[perimeterCurrentSegment];
            Vector3 end = perimeterExpandedCorners[(perimeterCurrentSegment + 1) % perimeterExpandedCorners.Length];
            Vector3 direction = (end - start).normalized;
            float segmentLength = Vector3.Distance(start, end);

            // Move 1 unit
            perimeterCurrentDistance += 1;

            if (perimeterCurrentDistance > segmentLength)
            {
                // Move to the next segment
                perimeterCurrentSegment = (perimeterCurrentSegment + 1) % perimeterExpandedCorners.Length;
                perimeterCurrentDistance = 0;
                perimeterCurrentPosition = perimeterExpandedCorners[perimeterCurrentSegment];

                // Check if a full round is completed
                if (perimeterCurrentSegment == 0)
                {
                    perimeterCompleted = true;
                }
            }
            else
            {
                // Update the current position
                perimeterCurrentPosition = start + direction * perimeterCurrentDistance;
            }
        }

        public static bool HasCompletedPerimeter()
        {
            if (perimeterCompleted)
            {
                perimeterCompleted = false;
                return true;
            }
            return false;
        }
        public static bool CreateMilkZone(Vector3 corner1, Vector3 corner2, float height)
        {
            if (!perimeterInit)
            {
                InitPerimeter(new Vector3(corner1.x, height, corner1.z), new Vector3(corner2.x, height, corner2.z));
                return false;
            }
            else
            {
                if (!HasCompletedPerimeter())
                {
                    sharedObjectId++;
                    try
                    {
                        ServerSend.DropItem((ulong)dummyPerimeterId, 11, sharedObjectId, 0);
                    }
                    catch { }
                    MoveAlongPerimeter();
                    ServerSend.PlayerPosition((ulong)dummyPerimeterId, perimeterCurrentPosition + new Vector3(0, heightOffset, 0));
                    return false;
                }
                else
                {
                    perimeterInit = false;
                    return true;
                }
            }
        }
        public static void CreateMilkZoneA()
        {
            if (elapsedMilkZone < 0.05f) return;

            elapsedMilkZone = 0f;
            milkZoneA = CreateMilkZone(currentCGGOMap.MilkZoneAcorner1, currentCGGOMap.MilkZoneAcorner2, currentCGGOMap.MilkZoneAcorner1.y + 1f);
            if (milkZoneA) SendPlayerIntoVoid((ulong)dummyPerimeterId);

        }
        public static void CreateMilkZoneB()
        {
            if (elapsedMilkZone < 0.05f) return;

            milkZoneB = CreateMilkZone(currentCGGOMap.MilkZoneBcorner1, currentCGGOMap.MilkZoneBcorner2, currentCGGOMap.MilkZoneBcorner1.y + 1f);
            if (milkZoneB) SendPlayerIntoVoid((ulong)dummyPerimeterId);
            elapsedMilkZone = 0f;
        }


        public static void ManageMilkZone()
        {
            if (!milkZoneA) CreateMilkZoneA();
            if (milkZoneA && !milkZoneB) CreateMilkZoneB();
        }
    }
}
