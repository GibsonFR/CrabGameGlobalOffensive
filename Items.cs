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

            var dico = SharedObjectManager.Instance.GetDictionary(); // Récupération du dictionnaire partagé
            List<int> itemsToRemove = [];

            // Vérification des items à supprimer
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
                // On vérifie si la clé existe dans le dictionnaire
                if (!dico.ContainsKey(key)) continue;

                itemToDelete.Remove(key);

                // Récupération de l'objet partagé à l'aide de la clé
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