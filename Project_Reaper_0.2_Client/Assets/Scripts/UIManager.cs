using Riptide;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public bool choseReaper;

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private InputField usernameField;
    [SerializeField] private GameObject stateIndicator;
    [SerializeField] private TextMeshProUGUI state;

    private void Awake()
    {
        Singleton = this;
        usernameField.text = User.name;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ConnectClickedRunner()
    {
        connectUI.SetActive(false);

        choseReaper = false;

        Connect();
    }

    public void ConnectClickedReaper()
    {
        connectUI.SetActive(false);

        choseReaper = true;

        Connect();
    }

    private void Connect()
    {
        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        connectUI.SetActive(true);
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.serverStateNotification, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private static void AuthenticationNotReachable(Message message)
    {
        if (SceneManager.GetActiveScene().name == "Authentication")
            return;

        ushort gameStateId = message.GetUShort();
        AbstractGameState currentGameState = GameStateFactory.CreateGameState(gameStateId);

        Image stateIndicatorImg = Singleton.stateIndicator.GetComponent<Image>();
        if (currentGameState is WaitingState || currentGameState is RoundStartState)
        {
            stateIndicatorImg.color = Color.green;
            Singleton.state.text = "Server waiting for players";
        }
        else if (currentGameState is RunningState)
        {
            stateIndicatorImg.color = Color.yellow;
            Singleton.state.text = "Round already started";
        }
        else if (currentGameState is FinishedState)
        {
            stateIndicatorImg.color = new Color(1.0f, 0.64f, 0.0f);
            Singleton.state.text = "Server restarting";
        }
        else
        {
            stateIndicatorImg.color = Color.red;
            Singleton.state.text = "Server state unknown";
        }
    }

}