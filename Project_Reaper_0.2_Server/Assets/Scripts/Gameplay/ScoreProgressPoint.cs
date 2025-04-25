using System.Collections.Generic;
using UnityEngine;

public class ScoreProgressPoint : MonoBehaviour
{
    private List<Player> walkedThroughList = new List<Player>();

    [SerializeField] private bool isFinal;
    [SerializeField] private int scoreReward;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<Player>())
        {
            if (collider.gameObject.GetComponent<Player>().isReaper)
                return;
            if (collider.gameObject.GetComponent<Player>().dead)
                return;
            if (walkedThroughList.Contains(collider.gameObject.GetComponent<Player>()))
                return;
            else
            {
                CalculateAndGiveScore(collider.gameObject.GetComponent<Player>());
                if (isFinal)
                {
                    collider.gameObject.GetComponent<Player>().runnerFinishedRound = true;
                }
            }
        }
    }

    private void CalculateAndGiveScore(Player player)
    {
        if (walkedThroughList.Count == 0)
        {
            GiveScore(player, scoreReward, 3);
        }
        else if (walkedThroughList.Count == 1)
        {
            GiveScore(player, scoreReward, 2);
        }
        else if (walkedThroughList.Count == 2)
        {
            GiveScore(player, scoreReward, 1);
        }
        else
        {
            GiveScore(player, scoreReward, (1 / walkedThroughList.Count));
        }
    }

    private void GiveScore(Player player, int score, int multiplier)
    {
        walkedThroughList.Add(player);
        player.GainScore((score * multiplier));
    }
}