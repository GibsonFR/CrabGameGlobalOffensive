using UnityEngine.Events;

namespace CGGO
{
    public class KeyBindManager : MonoBehaviour
    {
        [Serializable]
        public class KeyCodeEvent : UnityEvent<KeyCode> { }

        public KeyCodeEvent keyDownListener;

        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(k => ((int)k < (int)KeyCode.Mouse0))
            .ToArray();

        void Update()
        {
            if (ChatBox.Instance != null && ChatBox.Instance.field_Private_Boolean_0) return;

            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        keyDownListener?.Invoke(keyCode);

                        switch (keyCode)
                        {
                         
                        }
                    }
                }
            }
        }
    }
}
