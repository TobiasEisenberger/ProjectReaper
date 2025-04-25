using Riptide;
using Riptide.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ServerToClientId : ushort // Every information that will be send from the Server to the Client
{
    sync = 1,
    playerSpawned,
    playerMovement,
    spinningTrapMovement,
    dynamicObjectUpdate,
    playerDeterminedRole, // what role did the Server assign to the player
    playerHealth,
    playerDeath,
    animationOrders,
    objectSpawned,
    objectDespawned,
    playerScore,
    gameStateUpdate,
    newCameraOrientation,
    powerUp,
    authenticationResponse,
    authenticationNotReachable,
    leaderboardResponse,
    serverStateNotification,
}

public enum ClientToServerId : ushort // Every information that will be send from Client to the Server
{
    name = 1,
    input,
    playerChoseReaper,// did player click the Reaper- or the Runner-Connect button ?
    signInRequest,
    leaderboardRequest,
}

public enum MessageHandlerGroupId : byte
{
    dbAPIHandlerGroupId = 1,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }
    public Client DbAPIClient { get; private set; }

    public bool HasServerDbAPISupport { get { return hasServerDbAPISupport; } set { hasServerDbAPISupport = value; } }

    // Defaults to true, cause server will tell during authentication if no db is available
    private bool hasServerDbAPISupport = true;

    private ushort _serverTick;

    public ushort ServerTick
    {
        get { return _serverTick; }
        private set
        {
            _serverTick = value;
            InterpolationTick = (ushort)(value - TicksBetweenPositionUpdates);
        }
    }

    public ushort InterpolationTick { get; private set; }
    private ushort _ticksBetweenPositionUpdates = 2;

    public ushort TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = (ushort)(ServerTick - value);
        }
    }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;
    [SerializeField] private ushort dbAPIPort;
        
    [Space(10)]
    [SerializeField] private ushort tickDivergenceTolerance = 1;
   
    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client("Gameserver client");
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        ServerTick = 2;

        DbAPIClient = new Client("Dbapi client");
        DbAPIClient.Disconnected += DbAPIDisconnected;

        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    private void FixedUpdate()
    {
        Client.Update();
        DbAPIClient.Update();
        ServerTick++;
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
        GameLogic.Singleton.enabled = true;
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnected -= DidDisconnect;
        Client.Disconnect();
        DbAPIClient.Disconnect();
    }

    public void ConnectDbAPI()
    {
        DbAPIClient.Connect($"{ip}:{dbAPIPort}", messageHandlerGroupId: (byte)MessageHandlerGroupId.dbAPIHandlerGroupId);
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        foreach (KeyValuePair<int, GameObject> entry in DynamicObjectUpdater.ObjectDictionary)
        {
            Destroy(entry.Value);
        }
        DynamicObjectUpdater.ObjectDictionary.Clear();
        foreach (Player player in Player.list.Values)
            Destroy(player.gameObject);

        StartCoroutine(ReloadSceneAsync());
    }

    IEnumerator ReloadSceneAsync()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private void DbAPIDisconnected(object sender, EventArgs e)
    {
        if (SceneManager.GetActiveScene().name != "Authentication")
            SceneManager.LoadScene("Authentication");
    }

    private void SetTick(ushort serverTick)
    {
        if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
        {
            Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    public static void Sync(Message message)
    {
        Singleton.SetTick(message.GetUShort());
    }

}