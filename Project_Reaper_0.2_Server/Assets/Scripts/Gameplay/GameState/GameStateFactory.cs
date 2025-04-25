using System;
using System.Collections.Generic;

public class GameStateFactory
{
    private static Dictionary<string, ushort> stateIdMap = new Dictionary<string, ushort>()
    {
        {nameof(WaitingState), 1},
        {nameof(RoundStartState), 2},
        {nameof(RunningState), 3},
        {nameof(FinishedState), 4},
    };

    public static AbstractGameState CreateGameState(string gameState)
    {
        if (!stateIdMap.ContainsKey(gameState))
            throw new ArgumentException($"Game state {gameState} does not exist");

        switch (gameState)
        {
            case nameof(WaitingState):
                return new WaitingState(stateIdMap[gameState]);

            case nameof(RoundStartState):
                return new RoundStartState(stateIdMap[gameState]);

            case nameof(RunningState):
                return new RunningState(stateIdMap[gameState]);

            case nameof(FinishedState):
                return new FinishedState(stateIdMap[gameState]);

            default:
                throw new ArgumentException($"Game state {gameState} does not exist");
        }
    }
}