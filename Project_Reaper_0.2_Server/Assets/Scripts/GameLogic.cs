using Riptide;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
    [SerializeField]
    private int clientGameStateUpdateInSeconds = 3;

    private int ticksPerUpdate;

    [Header("Team Settings")]
    [SerializeField]
    private short minRunnerCount;
    [SerializeField]
    private short minReaperCount;

    [Header("Round Settings")]
    [SerializeField]
    private int countdownSeconds;
    [SerializeField]
    private int roundTimeSeconds;

    public short MinRunnerCount { get { return minRunnerCount; } }
    public short MinReaperCount { get { return minReaperCount; } }

    public int CountDownSeconds { get { return countdownSeconds; } }
    public int RoundTimeSeconds { get { return roundTimeSeconds; } }

    public bool reapersWon;

    private static GameLogic _singleton;

    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    private AbstractGameState currentGameState;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        ticksPerUpdate = (int)((1 / Time.fixedDeltaTime) * clientGameStateUpdateInSeconds);
        InitGameState();
    }

    private void InitGameState()
    {
        currentGameState = GameStateFactory.CreateGameState(nameof(WaitingState));
        currentGameState.Enter();
    }

    private void Update()
    {
        AbstractGameState nextGameState = currentGameState.Update();
        // Game state changed
        if (nextGameState != null)
        {
            currentGameState.Exit();
            currentGameState = nextGameState;
            nextGameState.Enter();
            Debug.Log("Game state has changed: sending game state update");
            SendGameState();
        }
    }

    private void FixedUpdate()
    {
        SendGameStateUpdateByInterval();
    }

    public void RestartGame()
    {
        StartCoroutine(ReloadSceneAsync());
    }

    IEnumerator ReloadSceneAsync()
    {
        yield return new WaitForSecondsRealtime(5);
        NetworkManager.Singleton.Server.Stop();
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        InitGameState();
        // GetComponent<SpawnDynamicObjects>().Init();
        SpawnDynamicObjects.Singleton.Init();
    }

    private void SendGameState()
    {
        Debug.Log($"Sending game state: {currentGameState.GameStateId} to clients");
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.gameStateUpdate);
        NetworkManager.Singleton.Server.SendToAll(currentGameState.BuildUpdateMessage(message));
        NotifyGameStateNonPlayingClients();
    }

    private void NotifyGameStateNonPlayingClients()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.serverStateNotification);
        message.AddUShort(currentGameState.GameStateId);
        // TODO: improvement would be filtering out players and send only to clients that are not connected to game server
        NetworkManager.Singleton.DatabaseAPIServer.SendToAll(currentGameState.BuildUpdateMessage(message));
    }

    private void SendGameStateUpdateByInterval()
    {
        if (NetworkManager.Singleton.CurrentTick % ticksPerUpdate == 0)
        {
            Debug.Log("Game state update interval exceeded: sending game state update");
            SendGameState();
        }
    }

}