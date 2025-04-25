using TMPro;
using UnityEngine;

public class UICountdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] playerDisplayNames = new TextMeshProUGUI[10];

    private ushort[] playerIds = new ushort[10];

    public void AddPlayerToUIList(Player player)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == 0)
            {
                playerIds[i] = player.Id;
                playerDisplayNames[i].text = player.GetUsername();
                if (player.IsLocal)
                {
                    playerDisplayNames[i].text += " (You)";
                }
                i = playerDisplayNames.Length + 1;
            }
        }
    }

    public void RemovePlayerFromUIList(ushort playerId)
    {
        bool playerFound = false;

        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == playerId)
            {
                playerFound = true;
            }
            if (playerFound)
            {
                if (i == playerIds.Length - 1)
                {
                    playerIds[i] = 0;
                    playerDisplayNames[i].text = "";
                }
                else
                {
                    playerIds[i] = playerIds[i + 1];
                    playerDisplayNames[i].text = playerDisplayNames[i + 1].text;
                }
            }
        }
    }

    public void ListPlayerDeath(ushort playerId)
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] == playerId)
            {
                playerDisplayNames[i].text = "<s>" + playerDisplayNames[i].text + "</s>";
                i += playerIds.Length;
            }
        }
    }
}