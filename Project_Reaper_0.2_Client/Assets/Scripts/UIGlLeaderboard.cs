using Riptide;
using System;
using TMPro;
using UnityEngine;

public class UIGlLeaderboard : MonoBehaviour
{

    [SerializeField] private GameObject leaderboardEntry;
    [SerializeField] private GameObject leaderboardViewport;
    private float entryDistance = 32.9285f;
    public int noOfEntries { get; private set; } = 1;

    [SerializeField] private bool debugCreateEntry = false;
    public static Tuple<string[], string[], int[]> receivedEntries = null;

    private void Update()
    {
        if (debugCreateEntry)
        {
            debugCreateEntry = false;
            ClearList();
            CreateLeaderboardEntry(noOfEntries, "Googoogaga", "Goog", 34144);
        }
        if (receivedEntries != null)
        {
            for (int i = 0; i < receivedEntries.Item1.Length; i++)
            {
                CreateLeaderboardEntry(noOfEntries, receivedEntries.Item1[i], receivedEntries.Item2[i], receivedEntries.Item3[i]);
            }
            receivedEntries = null;
        }
    }

    public void ClearList()
    {
        noOfEntries = 1;
        leaderboardViewport.transform.DetachChildren();
    }

    public void CreateLeaderboardEntry(int no, string name, string team, int score)
    {
        GameObject entry = Instantiate(leaderboardEntry);
        entry.transform.SetParent(leaderboardViewport.transform, false);
        Vector3 newPosition = entry.transform.localPosition;
        newPosition.y -= entryDistance * (noOfEntries - 1);
        entry.transform.localPosition = newPosition;
        TextMeshProUGUI[] columns = entry.GetComponentsInChildren<TextMeshProUGUI>();
        columns[0].text = noOfEntries.ToString();
        noOfEntries++;
        columns[1].text = name;
        columns[2].text = team;
        columns[3].text = score.ToString();
    }

    [MessageHandler((ushort)ServerToClientId.leaderboardResponse, (byte)MessageHandlerGroupId.dbAPIHandlerGroupId)]
    private static void LeaderboardResponse(Message message)
    {
        string[] names = message.GetStrings();
        string[] teams = message.GetStrings();
        int[] scores = message.GetInts();

        receivedEntries = new Tuple<string[], string[], int[]>(names, teams, scores);
    }

}
