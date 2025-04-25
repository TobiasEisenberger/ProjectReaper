using Riptide;
using TMPro;
using UnityEngine;

public class WaitingState : AbstractGameState
{

    public short MinRunnerCount { get; private set; } = -1;

    public short MinReaperCount { get; private set; } = -1;

    public short CurrentRunnerCount { get; private set; } = 0;

    public short CurrentReaperCount { get; private set; } = 0;

    public WaitingState(ushort gameStateId) : base(gameStateId) { }

    private TextMeshProUGUI counterReapers;
    private TextMeshProUGUI counterRunners;

    public override void ServerStateUpdate(Message message)
    {
        short[] minTeamSizes = message.GetShorts(4);
        MinRunnerCount = minTeamSizes[0];
        MinReaperCount = minTeamSizes[1];
        CurrentRunnerCount = minTeamSizes[2];
        CurrentReaperCount = minTeamSizes[3];
        UpdateUI();
    }

    public override void Enter()
    {
        counterReapers = GameObject.Find("PlayerRequiredCountReapers").GetComponent<TextMeshProUGUI>();
        counterRunners = GameObject.Find("PlayerRequiredCountRunners").GetComponent<TextMeshProUGUI>();
        counterReapers.enabled = true;
        counterRunners.enabled = true;
    }

    public override AbstractGameState Update()
    {
        return null;
    }

    public override void Exit()
    {
        counterReapers.enabled = false;
        counterRunners.enabled = false;
    }

    private void UpdateUI()
    {
        if (MinRunnerCount != -1 && MinReaperCount != -1)
        {
            counterReapers.text = $"Reaper: {CurrentReaperCount}/{MinReaperCount}";
            counterRunners.text = $"Runner: {CurrentRunnerCount}/{MinRunnerCount}";
        }
    }
}