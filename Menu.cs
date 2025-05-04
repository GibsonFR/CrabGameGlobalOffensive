using static CGGO.MenuUtility;
using static CGGO.MenuConstants;

namespace CGGO
{
    public class MenuConstants
    {
        public const string MENU_ON_MSG = "■<color=orange>CGGOMenu <color=blue>ON</color></color>■";
        public const string MENU_OFF_MSG = "■<color=orange>CGGOMenu <color=red>OFF</color></color>■";
    }

    public class MenuButton
    {
        public string Label { get; }
        public Action Action { get; }
        public Func<string> Status { get; }
        public List<MenuButton> SubMenu { get; }

        public bool IsScrollable { get; }
        private readonly Action<int> _setter;
        private readonly Func<int> _getter;

        public int ScrollValue => _getter != null ? _getter() : 0;

        private readonly Func<int> _scrollMinFunc;
        private readonly Func<int> _scrollMaxFunc;

        private readonly Func<MenuButton, bool, string> _customFormatter;

        public MenuButton(string label, Action action = null, Func<string> status = null,
                          List<MenuButton> subMenu = null,
                          bool isScrollable = false,
                          Action<int> setter = null, Func<int> getter = null,
                          Func<int> scrollMin = null, Func<int> scrollMax = null,
                          Func<MenuButton, bool, string> customFormatter = null)
        {
            Label = label;
            Action = action;
            Status = status;
            SubMenu = subMenu;

            IsScrollable = isScrollable;
            _setter = setter;
            _getter = getter;

            _scrollMin = scrollMin;
            _scrollMax = scrollMax;
            _customFormatter = customFormatter;
        }

        public void AdjustScrollValue(int delta)
        {
            if (IsScrollable && _getter != null && _setter != null && scrollMin.HasValue && scrollMax.HasValue)
            {
                int newValue = Mathf.Clamp(_getter() + delta, scrollMin.Value, scrollMax.Value);
                _setter(newValue);
            }
        }

        public string GetFormattedLabel(bool isSelected)
        {
            if (_customFormatter != null)
                return _customFormatter(this, isSelected);

            string prefix = isSelected ? "■<color=yellow>" : "  ";
            string suffix = isSelected ? "</color>■" : "";
            string scrollableValue = IsScrollable ? $"<color=green>{_getter()}</color>" : "";
            return $"{prefix}{Label}{suffix} {Status?.Invoke()} {scrollableValue}";
        }

        public bool HasSubMenu => SubMenu != null && SubMenu.Any();

        private int? scrollMin => _scrollMin?.Invoke();
        private int? scrollMax => _scrollMax?.Invoke();

        private readonly Func<int> _scrollMin;
        private readonly Func<int> _scrollMax;
    }
    public class MenuManager : MonoBehaviour
    {
        public Text menuText;

        private int selectedIndex;
        private List<MenuButton> currentMenu;
        private Stack<(List<MenuButton> menu, int index)> menuStack;
        private bool scrollingMode = false;


        void Start()
        {
            menuStack = new Stack<(List<MenuButton>, int)>();

            currentMenu =
            [
                new("Enable CGGO", () => ToggleBoolean(ref cggoEnabled, "Enable CGGO"), () => cggoEnabled ? "<color=blue>ON</color>" : "<color=red>OFF</color>"),
            ];
        }

        void Update()
        {
            if (Input.GetKeyDown(menuKey))
            {
                menuTrigger = !menuTrigger;
                menuText.text = menuTrigger ? MENU_ON_MSG : MENU_OFF_MSG;
                PlayMenuSound();
            }

            if (menuTrigger)
            {
                HandleNavigation();
                HandleSelection();
            }
            else menuText.text = "";
        }

        void FixedUpdate()
        {
            if (menuTrigger) RenderMenu();          
        }

        void HandleNavigation()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            var selectedButton = currentMenu[selectedIndex];

            if (scrollingMode)
            {
                if (selectedButton.IsScrollable && Mathf.Abs(scroll) > 0f)
                {
                    selectedButton.AdjustScrollValue(scroll > 0f ? -1 : 1);
                    PlayMenuSound();
                }

                if (Input.GetMouseButtonDown(2)) 
                {
                    scrollingMode = false;
                    PlayMenuSound();
                }

                return;
            }

            if (scroll > 0f)
            {
                selectedIndex = (selectedIndex - 1 + currentMenu.Count) % currentMenu.Count;
                PlayMenuSound();
            }
            else if (scroll < 0f)
            {
                selectedIndex = (selectedIndex + 1) % currentMenu.Count;
                PlayMenuSound();
            }

            if (selectedButton.IsScrollable && Input.GetMouseButtonDown(1))
            {
                scrollingMode = true;
                PlayMenuSound();
            }

            if (Input.GetMouseButtonDown(2) && menuStack.Count > 0)
            {
                (currentMenu, selectedIndex) = menuStack.Pop();
                PlayMenuSound();
            }
        }



        void HandleSelection()
        {
            if (Input.GetMouseButtonDown(1))
            {
                var selectedButton = currentMenu[selectedIndex];
                if (selectedButton.HasSubMenu)
                {
                    menuStack.Push((currentMenu, selectedIndex));
                    currentMenu = selectedButton.SubMenu;
                    selectedIndex = 0;
                }
                else
                    selectedButton.Action?.Invoke();

                PlayMenuSound();
            }
        }
        void RenderMenu()
        {
            string separator = "      " + new string('_', 100);

            var menuLines = currentMenu.Select((btn, index) =>
                "      " + btn.GetFormattedLabel(index == selectedIndex));

            menuText.text = $"\n{separator}\n\n{string.Join("\n", menuLines)}";
        }

    }

    public class MenuUtility
    {
        public static void ToggleBoolean(ref bool trigger, string triggerName)
        {
            trigger = !trigger;
            ForceMessage(trigger ? $"{triggerName} <color=blue>ON</color>" : $"{triggerName} <color=red>OFF</color>");
        }
    }

}
