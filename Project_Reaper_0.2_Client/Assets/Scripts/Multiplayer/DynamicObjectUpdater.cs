using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectUpdater : MonoBehaviour
{
    private static Dictionary<int, GameObject> objectDictionary = new Dictionary<int, GameObject>();
    public static Dictionary<int, GameObject> ObjectDictionary
    { get { return objectDictionary; } }

    [MessageHandler((ushort)ServerToClientId.dynamicObjectUpdate)]
    public static void DynamicObjectUpdate(Message message)
    {
        int count = message.GetInt();

        for (int i = 0; i < count; i++)
        {
            int objectId = message.GetInt();

            Vector3 objectPosition = message.GetVector3();
            Vector3 objectRotation = message.GetVector3();
            if (objectDictionary.ContainsKey(objectId))
            {
                GameObject obj = objectDictionary[objectId];
                // Update the object's transform
                obj.transform.SetPositionAndRotation(objectPosition, Quaternion.Euler(objectRotation));
            }
            else
            {
                Debug.LogWarning("Object with ID: " + objectId + " does not exist");
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.objectSpawned)]
    public static void ObjectSpawned(Message message)
    {
        int numObjects = message.GetInt(); // Get the number of objects in the group
        string prefabName = message.GetString();

        for (int i = 0; i < numObjects; i++)
        {
            int objectId = message.GetInt();

            Vector3 objectPosition = message.GetVector3();
            Vector3 objectRotation = message.GetVector3();
            Vector3 objectScale = message.GetVector3();

            // Load prefab
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found: '{prefabName}'");
                continue; // Continue to the next object
            }
            // Instantiate object
            GameObject newObj = Instantiate(prefab, objectPosition, Quaternion.Euler(objectRotation));
            newObj.name = prefabName + objectId.ToString(); // Ensure the object's name matches its ID
            newObj.transform.localScale = objectScale;
            objectDictionary.Add(objectId, newObj);
        }
    }

    [MessageHandler((ushort)ServerToClientId.objectDespawned)]
    public static void ObjectDespawned(Message message)
    {
        int objectId = message.GetInt();
        if (objectDictionary.TryGetValue(objectId, out GameObject gameObject))
        {
            //objectDictionary.Remove(objectId);

            // Generic Method is more efficient than non generic TryGetComponent (see unity manual GameObject.TryGetComponent
            if (gameObject.TryGetComponent<DespawnStrategy>(out DespawnStrategy strategy))
                strategy.DisableGameObject();
            else
                gameObject.SetActive(false);
        }
    }

    [MessageHandler((ushort)ServerToClientId.powerUp)]
    public static void PowerUpPickup(Message message)
    {

        int objectId = message.GetInt();
        GameObject powerUpParent = DynamicObjectUpdater.GetObjectById(objectId);
        GameObject powerUp = powerUpParent.transform.GetChild(0).gameObject;
        PowerUp powerUpScript = powerUp.GetComponent<PowerUp>();

        bool isExpired = message.GetBool();
        if (isExpired)
            powerUpScript.OnExpiration();
        else
            powerUpScript.PickUp();


    }

    public static GameObject GetObjectById(int objectId)
    {
        return objectDictionary.GetValueOrDefault(objectId);
    }

}