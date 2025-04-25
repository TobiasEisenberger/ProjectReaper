using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public User user { get; private set; }

    public bool isReaper;
    public bool runnerFinishedRound = false;
    public bool IsRoleApproved { get; private set; }

    [SerializeField] private PlayerMovement movement;
    private SpawnManager spawnManager;

    [SerializeField] private int maxHealth = 100;
    public int currHealth;
    public bool dead = false;

    public int Score { get { return score; } }

    private float dmgCooldownTime = 1.2f;
    private float nextDamageTime = 0f;
    [SerializeField]
    private bool isInvincible = false;
    public bool IsInvincible {  get { return isInvincible; } set { isInvincible = value; } }

    public bool IsInDmgCooldown => Time.time < nextDamageTime;

    private int score = 0;
    private static int noOfRunners = 0;

    private void Start()
    {
        spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene!");
        }
        movement = GetComponent<PlayerMovement>();
        currHealth = maxHealth;
    }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab,new Vector3(-19.3579998f, 31.25f, 126.089996f), Quaternion.identity).GetComponent<Player>();
        player.user = Authenticator.GetAuthenticatedUser(username);

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        player.SendSpawned();
        list.Add(id, player);
        Debug.Log("PLAYER SPAWNED: TRYING TO SEND OBJECTS NOW");
        SpawnDynamicObjects.Singleton.StartCoroutine(SpawnDynamicObjects.Singleton.SendSpawnedObjectDataToClient(id));
    }

    public void StartDamageCooldown()
    {
        nextDamageTime = Time.time + dmgCooldownTime;
    }

    public void EnableMovement()
    {
        movement.RB.isKinematic = false;
    }

    public void DisableMovement()
    {
        movement.RB.velocity = Vector3.zero;
        movement.RB.isKinematic = true;
    }

    public void HalfScore()
    {
        if (score == 0)
            return;
        int halfScore = score / 2;
        GainScore(-halfScore);
    }

    #region Messages

    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    public void LowerHealth(int dmg) // or restore health if negative dmg
    {
        currHealth -= dmg;
        if (currHealth > maxHealth)
        {
            currHealth = 100;
        }
        if (currHealth > 0)
        {
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerHealth);
            message.AddUShort(Id);
            message.AddInt(currHealth);
            NetworkManager.Singleton.Server.Send(message, Id);
        }
        else
        {
            currHealth = 0;
            movement.RB.velocity = Vector3.zero;
            movement.RB.isKinematic = true;
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            Die();
        }
    }

    private void Die()
    {
        dead = true;
        Message condolences = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerDeath);
        condolences.AddUShort(Id);
        condolences.AddBool(dead);
        NetworkManager.Singleton.Server.SendToAll(condolences);

        if (noOfRunners == 0)
        {
            foreach (Player player in list.Values)
            {
                if (!player.isReaper)
                    noOfRunners++;
            }
        }

        foreach (Player player in list.Values)
        {
            if (player.isReaper)
            {
                if (noOfRunners <= 0)
                {
                    player.GainScore(10000 - score);
                }
                else
                {
                    player.GainScore((10000 - score) / noOfRunners);
                }
            }
        }
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
        {
            player.movement.SetInput(message.GetBools(10), message.GetVector3());
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerChoseReaper)] // this line makes the function run after if receives the message from the client
    private static void DeterminePlayerRole(ushort fromClientId, Message message)
    {
        Debug.Log($"Testing PlayerID Received Message from Client:{fromClientId}");
        if (list.TryGetValue(fromClientId, out Player player)) // finds the correct player from player list
        {
            player.isReaper = message.GetBool(); // the Server assigns the role that the player has chosen
                                                 // maybe later the Server should check if there's enough Runners or Reapers
            Debug.Log($"Testing PlayerID:{player.Id}");
            Message responseMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerDeterminedRole); // the Server sends the role it assigned to the Client
            responseMessage.AddUShort(player.Id);                                                                      // this line makes sure which player's role is assigned
            responseMessage.AddBool(player.isReaper);
            NetworkManager.Singleton.Server.SendToAll(responseMessage);
            player.movement.Teleport(player.spawnManager.FindSpawnPoint(player.isReaper)); // teleports the player to a spawnpoint
            player.IsRoleApproved = true;
        }
    }

    public void GainScore(int score)
    {
        this.score += score;
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerScore);
        message.AddUShort(Id);
        message.AddInt(this.score);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    #endregion Messages
}