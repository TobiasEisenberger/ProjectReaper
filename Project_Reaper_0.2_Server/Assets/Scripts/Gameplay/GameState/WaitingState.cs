using Riptide;
using UnityEngine;

public class WaitingState : AbstractGameState
{

    public short MinRunnerCount { get; private set; }

    public short MinReaperCount { get; private set; }

    public short CurrentRunnerCount { get; private set; } = 0;

    public short CurrentReaperCount { get; private set; } = 0;

    public int MinPlayerCount { get { return MinRunnerCount + MinReaperCount; } }

    public WaitingState(ushort gameStateId) : base(gameStateId)
    {
        var gameLogic = GameLogic.Singleton;
        MinRunnerCount = gameLogic.MinRunnerCount;
        MinReaperCount = gameLogic.MinReaperCount;
    }

    public override void Enter()
    {
        if (!NetworkManager.Singleton.Server.IsRunning)
            NetworkManager.Singleton.RestartGameServer();

        Debug.Log($"Waiting for {MinPlayerCount} players to join");
    }

    public override AbstractGameState Update()
    {
        CountTeamSize();
        if (IsLobbyFilled())
            return GameStateFactory.CreateGameState(nameof(RoundStartState));

        return null;
    }

    public override Message BuildUpdateMessage(Message message)
    {
        return base.BuildUpdateMessage(message)
            .AddShorts(new short[] {MinRunnerCount, MinReaperCount, CurrentRunnerCount, CurrentReaperCount}, false);
    }

    private void CountTeamSize()
    {
        short runnerCount = 0;
        short reaperCount = 0;
        foreach (Player player in Player.list.Values)
        {
            if (!player.IsRoleApproved)
                continue;

            if (player.isReaper)
                reaperCount++;
            else
                runnerCount++;
        }

        CurrentRunnerCount = runnerCount;
        CurrentReaperCount = reaperCount;
    }

    private bool IsLobbyFilled()
    {
        if (NetworkManager.Singleton.Server.ClientCount < MinPlayerCount)
            return false;

        return CurrentRunnerCount >= MinRunnerCount && CurrentReaperCount >= MinReaperCount;
    }
}