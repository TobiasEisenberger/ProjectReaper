using TMPro;
using UnityEngine;

public class UIEndgameLeaderboard : MonoBehaviour
{

    [SerializeField] private GameObject leaderboard;
    [SerializeField] private GameObject leaderboardHead;
    [SerializeField] private GameObject leaderboardViewport;
    [SerializeField] private GameObject leaderboardEntries;
    public int noOfEntries { get; private set; } = 1;
    private float lowerAmount = 23.0f;

    private void Start()
    {
        leaderboardHead.SetActive(false);
        leaderboard.SetActive(false);
    }

    public void FillEntry(int no, string name, string team, int score)
    {
        GameObject entry = Instantiate(leaderboardEntries);
        entry.transform.SetParent(leaderboardViewport.transform, false);
        Vector3 newPosition = entry.transform.localPosition;
        newPosition.y -= lowerAmount * (noOfEntries - 1);
        entry.transform.localPosition = newPosition;
        TextMeshProUGUI[] columns = entry.GetComponentsInChildren<TextMeshProUGUI>();
        columns[0].text = noOfEntries.ToString();
        noOfEntries++;
        columns[1].text = name;
        columns[2].text = team;
        columns[3].text = score.ToString();
    }

}
