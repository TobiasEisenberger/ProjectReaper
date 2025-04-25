using Riptide;
using TMPro;
using UnityEngine;

public class RunningState : AbstractGameState
{
    private int roundTimeSeconds = -1;
    private float timeLeftSeconds;

    public RunningState(ushort gameStateId) : base(gameStateId)
    {
    }

    private TextMeshProUGUI countdownText;

    public override void ServerStateUpdate(Message message)
    {
        int serverRoundTime = message.GetInt();
        if (roundTimeSeconds == -1)
        {
            roundTimeSeconds = serverRoundTime;
            timeLeftSeconds = roundTimeSeconds;
        }

        // Resyncing countdown if countdown and passed time diverge
        float passedTimeSeconds = message.GetFloat();
        float remainingTimeServer = roundTimeSeconds - passedTimeSeconds;
        if (remainingTimeServer < timeLeftSeconds) // TODO consider using client RTT because of packet delay?
        {
            timeLeftSeconds = remainingTimeServer;
        }
    }

    public override void Enter()
    {
        countdownText = GameObject.Find("CountdownTime").GetComponent<TextMeshProUGUI>();
    }

    public override AbstractGameState Update()
    {
        timeLeftSeconds -= Time.deltaTime;
        if (timeLeftSeconds > 0)
        {
            int minutes = Mathf.FloorToInt(timeLeftSeconds / 60);
            int seconds = Mathf.FloorToInt(timeLeftSeconds % 60);
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        return null;
    }
}