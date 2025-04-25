using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class FinishedState : AbstractGameState
{
    private GameObject leaderboard;
    private GameObject leaderboardHead;
    private GameObject endgameUI;

    public FinishedState(ushort gameStateId) : base(gameStateId)
    {
    }

    public override void Enter()
    {
        // TODO: Display in UI round results
        GameObject deathMessage = GameObject.Find("DeathMessage");
        if (deathMessage != null && deathMessage.activeSelf)
        {
            deathMessage.SetActive(false);
        }
        List<Player> ranking = SortCurrentPlayers();
        //leaderboardHead = GameObject.Find("EndgameHead");
        //leaderboard = GameObject.Find("EndgameLeaderboard");
        endgameUI = GameObject.Find("Endgame");
        leaderboardHead = endgameUI.transform.Find("EndgameHead").gameObject;
        leaderboard = endgameUI.transform.Find("EndgameLeaderboard").gameObject;
        leaderboardHead.SetActive(true);
        leaderboard.SetActive(true);
        foreach (Player player in ranking)
        {
            string team;
            if (!player.isReaper && !player.dead)
                team = "surviving Runner";
            else if (!player.isReaper && player.dead)
                team = "fallen Runner";
            else
                team = "Reaper";
            leaderboard.GetComponent<UIEndgameLeaderboard>().FillEntry(leaderboard.GetComponent<UIEndgameLeaderboard>().noOfEntries, player.GetUsername(), team, player.score);
        }
        Debug.Log("Round finished!");
    }

    public override AbstractGameState Update()
    {
        return null;
    }

    public static List<Player> SortCurrentPlayers()
    {
        List<Player> players = Player.list.Values.OrderByDescending(player => player.score).ToList();
        return players;
    }
}