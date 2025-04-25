using System.Linq;
using UnityEngine;

public class FinishedState : AbstractGameState
{

    private HighscoreRepository ScoreRepository { get; set; }
    private bool isReadyToClose = false;

    public FinishedState(ushort gameStateId) : base(gameStateId)
    {
        ScoreRepository = new HighscoreRepository();
    }

    public override void Enter()
    {
        Debug.Log("Round is over!");
        if (NetworkManager.Singleton.HasServerDbAPISupport)
        {
            var playerList = Player.list.Values.Where(x => x.user != null && x.user.IsAuthenticated).ToList();
            if (playerList.Count > 0)
                ScoreRepository.StorePlayerScores(playerList);
        }
        Debug.Log("Db score insert done: ready to close server");
        isReadyToClose = true;
    }

    public override AbstractGameState Update()
    {
        Debug.Log("server not ready to close");
        if (isReadyToClose)
        {
            Debug.Log("server ready to close now, reloading scene");
            GameLogic.Singleton.RestartGame();
            isReadyToClose = false;
        }
        return null;
    }

}