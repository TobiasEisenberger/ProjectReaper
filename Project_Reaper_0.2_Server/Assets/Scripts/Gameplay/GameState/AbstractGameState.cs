using Riptide;

public abstract class AbstractGameState
{
    public ushort GameStateId
    { get { return gameStateId; } }

    protected readonly ushort gameStateId;

    public AbstractGameState(ushort gameStateId)
    {
        this.gameStateId = gameStateId;
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public abstract AbstractGameState Update();

    public virtual Message BuildUpdateMessage(Message message)
    {
        message.AddUShort(gameStateId);
        return message;
    }
}