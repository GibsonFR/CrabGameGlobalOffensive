

namespace CGGO
{
    public static class Utility
    {
        /// <summary>
        /// Creates a directory if it does not already exist.
        /// </summary>
        public static void CreateFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Log(logFilePath, "Error creating folder: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a file if it does not already exist.
        /// </summary>
        public static void CreateFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using StreamWriter sw = File.CreateText(path);
                    sw.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Log(logFilePath, "Error creating file: " + ex.Message);
            }
        }

        /// <summary>
        /// Resets a file by clearing its contents if it exists.
        /// </summary>
        public static void ResetFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using StreamWriter sw = new(path, false);
                }
            }
            catch (Exception ex)
            {
                Log(logFilePath, "Error resetting file: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes a log entry to the specified log file.
        /// </summary>
        public static void Log(string path, string line)
        {
            try
            {
                using StreamWriter writer = new(path, true);
                writer.WriteLine(line.Trim());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log Error] Failed to write log: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the client is the host of the game.
        /// </summary>
        public static bool IsHost() => SteamManager.Instance.IsLobbyOwner();
        

        /// <summary>
        /// Retrieves the PlayerManager instance for a given SteamID.
        /// Searches both active players and spectators.
        /// </summary>
        public static PlayerManager GetPlayerManagerFromSteamId(ulong steamId)
        {
            foreach (var player in GameManager.Instance.activePlayers)
            {
                try
                {
                    if (player.value.steamProfile.m_SteamID == steamId)
                        return player.value;
                }
                catch (Exception ex)
                {
                    Log(logFilePath, $"Error retrieving PlayerManager from activePlayers for SteamID {steamId}: {ex.Message}");
                }
            }

            foreach (var player in GameManager.Instance.spectators)
            {
                try
                {
                    if (player.value.steamProfile.m_SteamID == steamId)
                        return player.value;
                }
                catch (Exception ex)
                {
                    Log(logFilePath, $"Error retrieving PlayerManager from spectators for SteamID {steamId}: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the current game state as a string.
        /// </summary>
        public static string GetGameState()
        {
            try
            {
                return GameManager.Instance.gameMode.modeState.ToString();
            }
            catch (Exception ex)
            {
                Log(logFilePath, $"Error retrieving game state: {ex.Message}");
                return ""; // Default fallback value
            }
        }

        /// <summary>
        /// Retrieves a list of alive players (not dead and not at position zero).
        /// </summary>
        public static List<ulong> GetPlayerAliveList()
        {
            return activePlayers
                .Where(player => player.Value != null
                              && !player.Value.dead
                              && player.Value.transform.position != Vector3.zero)
                .Select(player => player.Value.steamProfile.m_SteamID)
                .ToList();
        }

        /// <summary>
        /// Sends a private chat message to a specific player.
        /// Logs an error if message sending fails.
        /// Special thanks to github.com/lammas321 for contributions.
        /// </summary>
        public static void SendPrivateMessage(ulong clientId, string message)
        {
            try
            {
                string privateMessage = $"{message}";
                List<byte> bytes = [];
                bytes.AddRange(BitConverter.GetBytes((int)ServerSendType.sendMessage));
                bytes.AddRange(BitConverter.GetBytes((ulong)1));

                string username = SteamFriends.GetFriendPersonaName(new CSteamID(1));
                bytes.AddRange(BitConverter.GetBytes(username.Length));
                bytes.AddRange(Encoding.ASCII.GetBytes(username));

                bytes.AddRange(BitConverter.GetBytes(privateMessage.Length));
                bytes.AddRange(Encoding.ASCII.GetBytes(privateMessage));

                bytes.InsertRange(0, BitConverter.GetBytes(bytes.Count));

                Packet packet = new()
                {
                    field_Private_List_1_Byte_0 = new()
                };
                foreach (byte b in bytes)
                    packet.field_Private_List_1_Byte_0.Add(b);

                byte[] clientIdBytes = BitConverter.GetBytes(clientId);
                for (int i = 0; i < clientIdBytes.Length; i++)
                    packet.field_Private_List_1_Byte_0[i + 8] = clientIdBytes[i];

                SteamPacketManager.SendPacket(new CSteamID(clientId), packet, 8, SteamPacketDestination.ToClient);
            }
            catch (Exception ex)
            {
                Log(logFilePath, $"Error sending private message to {clientId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Forces a message to appear in the client chat only.
        /// </summary>
        public static void ForceMessage(string message)
        {
            try
            {
                ChatBox.Instance?.ForceMessage($"<color=yellow>[CGGO] {message}</color>");
            }
            catch (Exception ex)
            {
                Log(logFilePath, $"Error forcing message: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a server-wide chat message visible to all players.
        /// </summary>
        public static void SendServerMessage(string message)
        {
            foreach (var player in activePlayers) if (player.Value != null) SendPrivateMessage(player.Value.steamProfile.m_SteamID, $"{message}");
        }

            
        

        /// <summary>
        /// Creates or updates the configuration file with default values if missing.
        /// </summary>
        public static void SetConfigFile(string configFilePath)
        {
            try
            {
                Dictionary<string, string> configDefaults = new()
                {
                    {"version", "v1.0.0"},
                    {"menuKey", "insert"},
                };

                Dictionary<string, string> currentConfig = [];

                if (File.Exists(configFilePath))
                {
                    string[] lines = File.ReadAllLines(configFilePath);

                    foreach (string line in lines)
                    {
                        string[] keyValue = line.Split('=');
                        if (keyValue.Length == 2)
                        {
                            currentConfig[keyValue[0]] = keyValue[1];
                        }
                    }
                }

                foreach (KeyValuePair<string, string> pair in configDefaults)
                {
                    if (!currentConfig.ContainsKey(pair.Key))
                    {
                        currentConfig[pair.Key] = pair.Value;
                    }
                }

                using StreamWriter sw = File.CreateText(configFilePath);
                foreach (KeyValuePair<string, string> pair in currentConfig)
                {
                    sw.WriteLine(pair.Key + "=" + pair.Value);
                }
            }
            catch (Exception ex)
            {
                Log(logFilePath, $"Error setting config file: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the configuration file and applies settings to the application.
        /// </summary>
        public static void ReadConfigFile()
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    Log(logFilePath, "Config file not found.");
                    return;
                }

                string[] lines = File.ReadAllLines(configFilePath);
                Dictionary<string, string> config = [];
                CultureInfo cultureInfo = new CultureInfo("fr-FR");
                bool parseSuccess;
                bool resultBool;
                float resultFloat;

                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        config[key] = value;
                    }
                }

                if (!configOnStart)
                {
                    configOnStart = true;
                }

                menuKey = config["menuKey"];
            }
            catch (Exception ex)
            {
                Log(logFilePath, $"Error reading config file: {ex.Message}");
            }
        }

        public static void PlayMenuSound()
        {
            if (__PlayerInventory == null) return;

            __PlayerInventory.woshSfx.pitch = 5;
            __PlayerInventory.woshSfx.Play();
        }

        public static void BetterUnityParameters()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 9000;
        }

        public static int GetCurrentGameTimer()
        {
            return UnityEngine.Object.FindObjectOfType<TimerUI>().field_Private_TimeSpan_0.Seconds;
        }

        public static void CreateDummy(Vector3 position, int dummyId)
        {
            Packet packet = new((int)ServerSendType.gameSpawnPlayer); 

            packet.Method_Public_Void_UInt64_0((ulong)dummyId);
            packet.Method_Public_Void_Vector3_0(position);

            packet.Method_Public_Void_Vector3_0(Vector3.zero);
            ServerSend.Method_Private_Static_Void_ObjectPublicIDisposableLi1ByInByBoUnique_0(packet);
        }

        public static void SendPlayerIntoVoid(ulong playerId)
        {
            ServerSend.RespawnPlayer(playerId, new Vector3(0, -300, 0));
        }      
    }
}
