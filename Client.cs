using UnityEngine;
using static CGGO.ClientConstants;
using static CGGO.ClientManager;
using static CGGO.ClientUtility;

namespace CGGO
{
    public class ClientConstants
    {

    }
    public class ClientManager : MonoBehaviour
    {
        void Awake()
        {
            clientObject = null;
            clientBody = null;
            clientCamera = null;
        }

        void FixedUpdate()
        {
            if (clientObject == null) clientObject = GetClientObject();
            if (clientBody == null) clientBody = GetClientBody();
            if (clientCamera == null) clientCamera = GetClientCamera();
        }

        void Update()
        {
            if (clientObject == null) return;

            clientPosition = GetClientPosition() ?? Vector3.zero;
        }
    }
    public class ClientUtility
    {
        public static GameObject GetClientObject() => GameObject.Find("/Player");
        
        public static Rigidbody GetClientBody() => GetClientObject()?.GetComponent<Rigidbody>();
        
        public static Camera GetClientCamera() => UnityEngine.Object.FindObjectOfType<Camera>();
        
        public static PlayerMovement GetClientMovement() => GetClientObject()?.GetComponent<PlayerMovement>();

        public static Vector3? GetClientPosition()
        {
            return clientBody 
                ? clientBody.transform.position 
                : null;
        }

        public static string FormatClientPosition()
        {
            Vector3? position = GetClientPosition();
            return position.HasValue
                ? $"({position.Value.x:F1}, {position.Value.y:F1}, {position.Value.z:F1})"
                : "<color=red>N/A</color>";
        }

        public static float? GetClientSpeed()
        {
            return clientBody
                ? new Vector3(clientBody.velocity.x, 0f, clientBody.velocity.z).magnitude
                : null;
        }

        public static string FormatClientSpeed()
        {
            float? speed = GetClientSpeed();
            return speed.HasValue
                ? $"{speed.Value:F1} u/s"
                : "<color=red>N/A</color>";
        }

        public static Vector3? GetClientRotation()
        {
            return clientBody && clientCamera
                ? clientCamera.transform.rotation.eulerAngles
                : null;
        }

        public static string FormatClientRotation()
        {
            Vector3? rotation = GetClientRotation();
            return rotation.HasValue
                ? $"({rotation.Value.x:F1}, {rotation.Value.y:F1}, {rotation.Value.z:F1})"
                : "<color=red>N/A</color>";
        }

        public static void DisableClientMovement()
        {
            if (clientBody != null)
            {
                clientBody.isKinematic = true;
                clientBody.useGravity = false;
            }
        }
        public static void EnableClientMovement()
        {
            if (clientBody != null)
            {
                clientBody.isKinematic = false;
                clientBody.useGravity = true;
            }
        }
    }
}
