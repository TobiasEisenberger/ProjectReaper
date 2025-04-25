using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGlLeaderboardReturn : MonoBehaviour
{
    [SerializeField] private GameObject loginCanvas;
    [SerializeField] private GameObject leaderboard;
    [SerializeField] private GameObject hpBar;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject noOfPlayersText;

    public void ReturnToLogIn()
    {
        loginCanvas.SetActive(true);
        leaderboard.SetActive(false);
        hpBar.SetActive(true);
        timer.SetActive(true);
        noOfPlayersText.SetActive(true);
    }
}
