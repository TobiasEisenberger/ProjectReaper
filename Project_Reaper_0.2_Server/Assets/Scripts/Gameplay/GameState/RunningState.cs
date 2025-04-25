using Riptide;
using UnityEngine;

public class RunningState : AbstractGameState
{
    private float passedTimeSeconds = 0;
    private readonly int ROUNDTIME_SECONDS;

    public RunningState(ushort gameStateId) : base(gameStateId)
    {
        var gameLogic = GameLogic.Singleton;
        ROUNDTIME_SECONDS = gameLogic.RoundTimeSeconds;
    }

    public override void Enter()
    {
        foreach (Player player in Player.list.Values)
            player.EnableMovement();

        Debug.Log($"Round started with {ROUNDTIME_SECONDS} seconds until round is over");
    }

    public override AbstractGameState Update()
    {
        if (checkAllRunnersDead())
        {
            GameLogic.Singleton.reapersWon = true;
            foreach (Player player in Player.list.Values)
            {
                if (!player.isReaper)
                    player.HalfScore();
            }
            return GameStateFactory.CreateGameState(nameof(FinishedState));
        }

        if (checkAllRunnersFinished())
        {
            GameLogic.Singleton.reapersWon = false;
            foreach (Player player in Player.list.Values)
            {
                if (player.isReaper)
                    player.HalfScore();
                else if (player.dead)
                    player.HalfScore();
            }
            return GameStateFactory.CreateGameState(nameof(FinishedState));
        }

        passedTimeSeconds += Time.deltaTime;
        if (passedTimeSeconds >= ROUNDTIME_SECONDS)
        {
            GameLogic.Singleton.reapersWon = true;
            foreach (Player player in Player.list.Values)
            {
                if (!player.isReaper)
                    player.HalfScore();
            }
            return GameStateFactory.CreateGameState(nameof(FinishedState));
        }

        return null;
    }

    public override Message BuildUpdateMessage(Message message)
    {
        return base.BuildUpdateMessage(message)
            .AddInt(ROUNDTIME_SECONDS)
            .AddFloat(passedTimeSeconds);
    }

    public override void Exit()
    {
        foreach (Player player in Player.list.Values)
            player.DisableMovement();
    }

    private bool checkAllRunnersDead()
    {
        foreach (Player player in Player.list.Values)
        {
            if (!player.isReaper && !player.dead) return false;
        }
        return true;
    }

    private bool checkAllRunnersFinished()
    {
        foreach (Player player in Player.list.Values)
        {
            if (!player.isReaper && !player.dead && !player.runnerFinishedRound) return false;
        }
        return true;
    }
}