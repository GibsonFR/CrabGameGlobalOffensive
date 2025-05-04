namespace CGGO
{
    public class CommandPatchs
    {
        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPrefix]
        public static bool ServerSendSendChatMessagePre(ulong __0, string __1)
        {
            if (!IsHost()) return true; 

            string msg = __1.ToLower();
            if (__0 == clientId && IsCommand(msg))
            {
                string[] parts = msg.Split(' '); // Split the command and its arguments

                // Admin commands
                switch (parts[0])
                {
                    case "!help":
                        HandleAdminHelpCommand();
                        return false;
                    case "!start":
                        HandleStartCommand();
                        return false;
                    case "!reset":
                        HandleResetCommand();
                        return false;
                    case "!cggo":
                        HandleGenericToggleCommand(ref cggoEnabled, CommandMessages.CGGO_TOGGLE);
                        GameEndingPhaseUtility.ResetGameVariables();
                        return false;
                    case "!afk":
                        HandleGenericToggleCommand(ref hostAfk, CommandMessages.AFK_TOGGLE);
                        return false;
                    default:
                        return false;
                }
            }
            else if (IsCommand(msg)) return false;
            else return true;
        }

        [HarmonyPatch(typeof(ServerSend), nameof(ServerSend.SendChatMessage))]
        [HarmonyPostfix]
        public static void ServerSendSendChatMessagePost(ulong __0, string __1)
        {
            if (!IsHost()) return;

            string msg = __1.ToLower();

            if (IsCommand(msg))
            {
                string[] parts = msg.Split(' ');
                var player = CGGOPlayer.GetCGGOPlayer(__0);

                switch (parts[0])
                {
                    case "!help" or "/help" or "!h" or "/h":
                        HandlePlayerHelpCommand(__0);
                        break;
                    case "!discord" or "/discord" or "!d" or "/d" or "!disc" or "/disc" or "!cord" or "/cord":
                        HandleDiscordCommand(__0);
                        break;
                    case "!dev" or "/dev":
                        HandleDevCommand(__0);
                        break;
                    case "/vandal" or "!vandal" or "!v" or "/v":
                        HandleWeaponPurchase(__0, player, WeaponsConstants.RIFLE_PRICE, WeaponsId.RIFLE_ID, rifleList);
                        break;
                    case "/classic" or "!classic" or "!c" or "/c":
                        HandleWeaponPurchase(__0, player, WeaponsConstants.PISTOL_PRICE, WeaponsId.PISTOL_ID, pistolList);
                        break;
                    case "/revolver" or "!revolver" or "!r" or "/r":
                        HandleWeaponPurchase(__0, player, WeaponsConstants.REVOLVER_PRICE, WeaponsId.REVOLVER_ID, revolverList);
                        break;
                    case "/shorty" or "!shorty" or "!s" or "/s":
                        HandleWeaponPurchase(__0, player, WeaponsConstants.SHOTGUN_PRICE, WeaponsId.SHOTGUN_ID, shotgunList);
                        break;
                    case "/katana" or "!katana" or "!k" or "/k":
                        HandleWeaponPurchase(__0, player, WeaponsConstants.KATANA_PRICE, WeaponsId.KATANA_ID, katanaList);
                        break;
                    case "/shield25" or "!shield25" or "!25" or "/25":
                        HandleShieldPurchase(player, ShieldConstants.SMALL_SHIELD_PRICE, ShieldConstants.SMALL_SHIELD_VALUE);
                        break;
                    case "/shield50" or "!shield50" or "!50" or "/50":
                        HandleShieldPurchase(player, ShieldConstants.BIG_SHIELD_PRICE, ShieldConstants.BIG_SHIELD_VALUE);
                        break;
                }
            }
        }

        static void HandleResetCommand()
        {
            GameEndingPhaseUtility.ResetGameVariables();
            ServerSend.LoadMap(6, 0);
        }

        static bool IsCommand(string msg) => msg.StartsWith("!") || msg.StartsWith("/");

        static void HandleAdminHelpCommand()
        {
            SendServerMessage(CommandMessages.ADMIN_HELP_1);
            SendServerMessage(CommandMessages.ADMIN_HELP_2);
        }

        static void HandleModoHelpCommand()
        {
            SendServerMessage(CommandMessages.MODO_HELP_1);
            SendServerMessage(CommandMessages.MODO_HELP_2);
        }

        static void HandlePlayerHelpCommand(ulong userId)
        {
            SendPrivateMessage(userId, CommandMessages.PLAYER_HELP_1);
            SendPrivateMessage(userId, CommandMessages.PLAYER_HELP_2);
        }

        static void HandleGenericToggleCommand(ref bool flag, string msg)
        {
            flag = !flag;
            SendServerMessage(flag ? $"{msg} ON" : $"{msg} OFF");
        }

        static void HandleStartCommand() => GameLoop.Instance.StartGames();

        static void HandleDiscordCommand(ulong userId) => SendPrivateMessage(userId, CommandMessages.DISCORD);

        static void HandleDevCommand(ulong userId) => SendPrivateMessage(userId, CommandMessages.DEV);
    }
}
