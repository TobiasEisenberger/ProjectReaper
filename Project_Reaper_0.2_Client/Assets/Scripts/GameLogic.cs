using Riptide;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
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

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;

    [SerializeField] private GameObject playerPrefab;

    private AbstractGameState currentGameState;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        currentGameState = GameStateFactory.CreateGameState(nameof(WaitingState));
        currentGameState.Enter();
    }

    private void Update()
    {
        currentGameState.Update();
    }

    [MessageHandler((ushort)ServerToClientId.gameStateUpdate)]
    private static void HandleGamestateUpdateMessage(Message message)
    {
        ushort gameStateId = message.GetUShort();
        if (Singleton.currentGameState.GameStateId != gameStateId)
        {
            Singleton.currentGameState.Exit();
            AbstractGameState newGameState = GameStateFactory.CreateGameState(gameStateId);
            newGameState.Enter();
            newGameState.ServerStateUpdate(message);
            Singleton.currentGameState = newGameState;
        }
        else
        {
            Singleton.currentGameState.ServerStateUpdate(message);
        }

        Debug.Log($"Received game state update: current state id {gameStateId}");
    }
}