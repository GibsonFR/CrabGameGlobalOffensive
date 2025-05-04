namespace CGGO
{
    public class ItemsManager : MonoBehaviour
    {
        bool init;
        public static int deleteDelay = 100;

        void Update()
        {
            if (!init)
            {
                itemToDelete.Clear();
                init = true;
            }

            var dico = SharedObjectManager.Instance.GetDictionary(); // R�cup�ration du dictionnaire partag�
            List<int> itemsToRemove = [];

            // V�rification des items � supprimer
            foreach (var item in itemToDelete)
            {
                if ((DateTime.Now - item.Value).TotalMilliseconds > deleteDelay)
                {
                    itemsToRemove.Add(item.Key);
                }
            }

            // Suppression des objets
            foreach (var key in itemsToRemove)
            {
                // On v�rifie si la cl� existe dans le dictionnaire
                if (!dico.ContainsKey(key)) continue;

                itemToDelete.Remove(key);

                // R�cup�ration de l'objet partag� � l'aide de la cl�
                var obj = SharedObjectManager.Instance.GetSharedObject(key);

                // Si l'objet n'existe pas, on continue
                if (obj == null) continue;

                foreach (var player in activePlayers) if (player.Value != null) 
                        try 
                        {
                            ServerSend.SyncObject(player.Value.steamProfile.m_SteamID, key, false);
                        }
                        catch {};
            }
        }
    }
}