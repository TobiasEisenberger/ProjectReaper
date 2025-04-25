using Riptide;
using UnityEngine;

public class RoundStartState : AbstractGameState
{
    private float passedTimeSeconds = 0;
    private readonly int COUNT_DOWN_SECONDS;

    public RoundStartState(ushort gameStateId) : base(gameStateId)
    {
        var gameLogic = GameLogic.Singleton;
        COUNT_DOWN_SECONDS = gameLogic.CountDownSeconds;
    }

    public override void Enter()
    {
        Debug.Log($"Round will start in {COUNT_DOWN_SECONDS} seconds");
    }

    public override AbstractGameState Update()
    {
        passedTimeSeconds += Time.deltaTime;
        if (passedTimeSeconds >= COUNT_DOWN_SECONDS)
            return GameStateFactory.CreateGameState(nameof(RunningState));

        return null;
    }

    public override Message BuildUpdateMessage(Message message)
    {
        return base.BuildUpdateMessage(message)
            .AddInt(COUNT_DOWN_SECONDS)
            .AddFloat(passedTimeSeconds);
    }
}