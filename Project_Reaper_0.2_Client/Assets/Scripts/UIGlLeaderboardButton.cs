using Riptide;
using UnityEngine;
using UnityEngine.UI;

public class UIGlobalLeaderboard : MonoBehaviour
{

    [SerializeField] private GameObject globalLeaderboard;
    [SerializeField] private GameObject loginCanvas;

    [SerializeField] private GameObject hpBar;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject noOfPlayersText;

    void Start()
    {
        if (gameObject.TryGetComponent<Button>(out Button button))
        {
            button.interactable = NetworkManager.Singleton.HasServerDbAPISupport;
        }

        globalLeaderboard.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (globalLeaderboard.TryGetComponent<UIGlLeaderboard>(out var leaderboard))
        {
            leaderboard.ClearList();
        }
        SendLeaderboardRequest();
        globalLeaderboard.SetActive(true);
        loginCanvas.SetActive(false);
        hpBar.SetActive(false);
        timer.SetActive(false);
        noOfPlayersText.SetActive(false);
    }

    private void SendLeaderboardRequest()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.leaderboardRequest);
        NetworkManager.Singleton.DbAPIClient.Send(message);
    }

}
