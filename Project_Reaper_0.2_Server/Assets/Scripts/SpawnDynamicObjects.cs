using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnDynamicObjects : MonoBehaviour
{
    private static SpawnDynamicObjects _singleton;

    public static SpawnDynamicObjects Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
        }
    }

    private static Dictionary<int, GameObject> spawnedObjects = new Dictionary<int, GameObject>();

    // inverse dictonary
    public static Dictionary<GameObject, int> ObjectIds = new Dictionary<GameObject, int>();

    public Dictionary<GameObject, int> GetObjectIds()
    {
        return ObjectIds;
    }

    private List<GameObject> dynamicObjects = new List<GameObject>();
    private static int nextId = 0;

    // Add a delay between sending each message
    [SerializeField] private float messageDelaySpawn; 
    [SerializeField] private uint maxObjectsPerMessageSpawn;
    [SerializeField] private float messageDelayDynamicUpdate;
    [SerializeField] private uint maxObjMsgDynUpdate;
    private float lastMessageTime = 0f;
    private bool restartUpdateSync = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        nextId = 0;
        spawnedObjects.Clear();
        ObjectIds.Clear();
        // Find all objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Iterate through each object
        foreach (GameObject obj in allObjects)
        {
            DynamicObjectScript reqComponent = obj.GetComponent<DynamicObjectScript>();
            // Check if the object itself is tagged as "Dynamic"
            if (reqComponent || obj.CompareTag("SpawnOnClient"))
            {
                // Add the object to the dictionary with a unique ID
                AddObjectToDictionary(obj);
                if (reqComponent != null)
                {
                    dynamicObjects.Add(obj);
                }
            }
        }
        Debug.Log($"Spawned objects count: {spawnedObjects.Count}");
        StartCoroutine(SendDynamicObjectUpdate());
    }

    private void FixedUpdate()
    {
        if (restartUpdateSync)
            StartCoroutine(SendDynamicObjectUpdate());
    }

    private void AddObjectToDictionary(GameObject obj)
    {
        // Generate a unique ID for the object
        int objectId = nextId;
        // Add the object to the dictionary with its unique ID
        spawnedObjects.Add(objectId, obj);
        ObjectIds.Add(obj, objectId);
        // Increment the next ID for the next object
        nextId++;
    }

    // Method to send spawned object data to a specific client
    public IEnumerator SendSpawnedObjectDataToClient(ushort clientId)
    {
        Dictionary<string, List<GameObject>> prefabGroups = new Dictionary<string, List<GameObject>>();
        // Group objects by prefab name
        foreach (var entry in spawnedObjects)
        {
            GameObject obj = entry.Value;
            if(obj.activeSelf)
            {
                string prefabName = obj.name; // Assuming the prefab name is the same as the object's name
                try
                {
                    int spaceIndex = prefabName.IndexOf(" ");
                    if (spaceIndex != -1)
                    {
                        prefabName = prefabName.Substring(0, spaceIndex);
                    }
                }
                catch (ArgumentNullException) { }
                catch (ArgumentOutOfRangeException) { }

                if (!prefabGroups.ContainsKey(prefabName))
                {
                    prefabGroups[prefabName] = new List<GameObject>();
                }

                prefabGroups[prefabName].Add(obj);
            }
        }

        foreach (var groupEntry in prefabGroups)
        {
            string prefabName = groupEntry.Key;
            List<GameObject> objects = groupEntry.Value;

            for (int i = 0; i < objects.Count; i += (int)maxObjectsPerMessageSpawn)
            {
                // Check the time elapsed since the last message
                if (Time.time - lastMessageTime > messageDelaySpawn)
                {
                    yield return null;
                }
                lastMessageTime = Time.time;

                int count = Mathf.Min((int)maxObjectsPerMessageSpawn, objects.Count - i);
                List<GameObject> groupObjects = objects.GetRange(i, count);

                Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.objectSpawned);
                message.AddInt(count); // Number of objects in the group
                message.AddString(prefabName);

                foreach (GameObject obj in groupObjects)
                {
                    int objectId = ObjectIds[obj];
                    Vector3 position = obj.transform.position;
                    Vector3 rotation = obj.transform.rotation.eulerAngles;
                    Vector3 scale = obj.transform.localScale;

                    message.AddInt(objectId);
                    message.AddVector3(position);
                    message.AddVector3(rotation);
                    message.AddVector3(scale);
                }

                // Send spawn data to the specified client
                NetworkManager.Singleton.Server.Send(message, clientId);
                // Wait for the specified delay before sending the next message
                yield return new WaitForSeconds(messageDelaySpawn);
            }
        }
    }

    public Dictionary<int, List<Vector3>> CollectDynamicObjectUpdates()
    {
        Dictionary<int, List<Vector3>> objectChanges = new Dictionary<int, List<Vector3>>();
        foreach (var obj in dynamicObjects)
        {
            if(obj == null) continue;
            if(obj.TryGetComponent<DynamicObjectScript>(out var dynamicObjScript))
            {
                int objectId = Singleton.GetObjectIds()[obj];
                obj.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                if (dynamicObjScript.OldPos != position || dynamicObjScript.OldRot != rotation)
                {
                    dynamicObjScript.OldPos = position;
                    dynamicObjScript.OldRot = rotation;
                    Debug.LogWarning($"object moved and added to list");

                    objectChanges[objectId] = new List<Vector3>
                    {
                        position,
                        rotation.eulerAngles
                    };
                }
            }
        }

        return objectChanges;
    }

    public IEnumerator SendDynamicObjectUpdate()
    {
        Dictionary<int, List<Vector3>> objectChanges = CollectDynamicObjectUpdates();
        int objectIndex = 0;
        while (objectIndex < objectChanges.Count)
        {
            if (Time.time - lastMessageTime > messageDelayDynamicUpdate)
            {
                yield return null;
            }
            int count = (int)Mathf.Min(maxObjMsgDynUpdate, objectChanges.Count - objectIndex);
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.dynamicObjectUpdate);
            message.AddInt(count);

            for (int i = 0; i < count; i++, objectIndex++)
            {
                var item = objectChanges.ElementAt(objectIndex);
                int objectId = item.Key;
                Vector3 position = item.Value[0];
                Vector3 rotation = item.Value[1];

                message.AddInt(objectId);
                message.AddVector3(position);
                message.AddVector3(rotation);
            }

            NetworkManager.Singleton.Server.SendToAll(message);
            lastMessageTime = Time.time;
        }
        restartUpdateSync = true;
    }

    public static void DisableObject(GameObject obj)
    {
        int objId = ObjectIds[obj];
        GameObject spawnedObj = spawnedObjects[objId];

        if (spawnedObj != null)
        {   
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.objectDespawned);
            message.AddInt(objId);
            NetworkManager.Singleton.Server.SendToAll(message);
            spawnedObj.SetActive(false);
        }
    }

}