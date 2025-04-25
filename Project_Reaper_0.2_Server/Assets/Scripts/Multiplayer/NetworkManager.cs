using Riptide;
using Riptide.Utils;
using UnityEngine;

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

public enum ClientToServerId : ushort // Every information that will be send from the Client to the Server
{
    name = 1,
    input,
    playerChoseReaper, // did player click the Reaper- or the Runner-Connect button ?
    signInRequest,
    leaderboardRequest,
}

public enum MessageHandlerGroupId: byte
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

    public Server Server { get; private set; }
    public Server DatabaseAPIServer { get; private set; }
    public ushort CurrentTick { get; private set; } = 0;

    public bool HasServerDbAPISupport { get { return hasServerDbAPISupport; } }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    [SerializeField] private ushort dbAPIPort;
    [SerializeField] private bool hasServerDbAPISupport;

    private void Start()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server("Gameserver");
        Server.Start(port, maxClientCount);

        DatabaseAPIServer = new Server("Dbapi server");
        DatabaseAPIServer.Start(dbAPIPort, maxClientCount, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId);
        DatabaseAPIServer.ClientConnected += UserDatabaseAPIConnected;
        DatabaseAPIServer.ClientDisconnected += UserDatabaseAPIDisconnected;
        Server.ClientDisconnected += PlayerLeft;

        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RestartGameServer()
    {
        Server.Start(port, maxClientCount);
    }

    private void FixedUpdate()
    {
        Server.Update();
        DatabaseAPIServer.Update();
        // check if client and server tick rate still match every 5 seconds (60 frames per second * 5 seconds = 300)
        if (CurrentTick % 300 == 0)
        {
            SendSync();
        }
        CurrentTick++;
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
        DatabaseAPIServer.Stop();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Client.Id, out Player player))
            Destroy(player.gameObject);
    }

    private void UserDatabaseAPIConnected(object sender, ServerConnectedEventArgs e)
    {
        Authenticator.EnqueueUser(e.Client.Id);
        if(!HasServerDbAPISupport)
        {
            Authenticator.SendAuthenticationNotAvailable(e.Client.Id);
        }
    }

    private void UserDatabaseAPIDisconnected(object sender, ServerDisconnectedEventArgs e)
    {
        Authenticator.LogOutUser(e.Client.Id);
    }   

    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.sync);
        message.Add(CurrentTick);
        Server.SendToAll(message);
    }

}