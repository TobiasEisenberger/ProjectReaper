using Riptide;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private Transform camTransform;
    [SerializeField] private static PlayerController controller;
    [SerializeField] private MovementAnimationBindings animBindings;
    public Transform CamTransform
    { get { return camTransform; } set { camTransform = value; } }
    [SerializeField] private Interpolator interpolator;
    [SerializeField] private GameObject footSteps;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip audioClipHurt;

    private string username;

    public bool isReaper;

    [SerializeField] public int maxHealth = 100;
    public int currHealth = 100;
    public bool dead = false;

    private static Slider healthBar;
    private static TMP_Text deathMessage;
    private static UICountdown uiCountdown;

    public int score = 0;

    public void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    private void Update() // only for testing
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.J))
        {
            if (isReaper)
            {
                Debug.Log($"{username} ist ein Reaper, Oink");
            }
            else if (!isReaper)
            {
                Debug.Log($"{username} ist ein Runner, Oink");
            }
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"{username}'s score is {score}");
        }
    }

    private void OnDestroy()
    {
        uiCountdown.RemovePlayerFromUIList(Id);
        list.Remove(Id);
    }

    private void Move(ushort tick, bool isTeleport, Vector3 newPosition, Vector3 forward)
    {
        interpolator.ReceiveTransformUpdate(tick, isTeleport, newPosition);
        //get the movement direction
        Vector3 direction = interpolator.DetermineDirection();

        if (!(direction.y > 0.1f || direction.y < -0.1f) && (direction.z > 0.1f || direction.z <= -0.1f || direction.x < 0 || direction.x > 0))
            footSteps.SetActive(true);
        else footSteps.SetActive(false);
      
        

        animator.SetBool("runningForward", direction.z > 0.1f);
        animator.SetBool("runningBackward", direction.z <= -0.1f);
        //controller.IsSprinting
        animator.SetBool("sprint", (direction.z > 0 || direction.z < 0) && Input.GetKey(KeyCode.LeftShift));
        //controller.ShouldJump);
        animator.SetBool("jump", direction.y > 0.05f && Input.GetKey(KeyCode.Space));
        Debug.DrawRay(newPosition, forward * 10, Color.yellow);

        if (!IsLocal)
        {
            camTransform.forward = forward;
            gameObject.transform.forward = new Vector3(forward.x, gameObject.transform.forward.y, forward.z);

            //changes like above
            animator.SetBool("runningForward", direction.z > 0);
            animator.SetBool("runningBackward", direction.z < 0);
            animator.SetBool("Left", direction.x < 0);
            animator.SetBool("Right", direction.x > 0);
            animator.SetBool("sprint", direction.z < 0);
            animator.SetBool("jump", direction.y > 0);
        }
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
            player.SendChosenRole();
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} (username)";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        uiCountdown = GameObject.Find("Countdown").GetComponent<UICountdown>();

        uiCountdown.AddPlayerToUIList(player);
    }

    public string GetUsername()
    {
        return username;
    }

    public void PlayHurtAudio()
    {
        if(audioSrc != null && audioClipHurt != null)
        {
            audioSrc.clip = audioClipHurt;
            audioSrc.Play();
        }
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUShort(), message.GetBool(), message.GetVector3(), message.GetVector3());
    }

    private void SendChosenRole() // informs the server what role did the player choose
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.playerChoseReaper);
        message.AddBool(UIManager.Singleton.choseReaper);
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.playerDeterminedRole)] // this line makes the SetRole() function run when the Client receives a message concering playerDeterminedRole
    private static void SetRole(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player)) // You have to GetValue() in the order that they were added to the message, so here GetUshort() first, then GetBool()
        {
            player.isReaper = message.GetBool();

            //Control script
            if (player.isReaper)
                Debug.Log($"{player.username} is a Reaper");
            else
                Debug.Log($"{player.username} is a Runner");
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerHealth)]
    private static void UpdateHealth(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            int newHealth = message.GetInt();
            if (newHealth < player.currHealth)
                player.PlayHurtAudio();
            player.currHealth = newHealth;
            healthBar.value = player.currHealth;
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerDeath)]
    private static void PlayerDeath(Message condolences)
    {
        if (list.TryGetValue(condolences.GetUShort(), out Player player))
        {
            player.dead = condolences.GetBool();
            if (player.IsLocal)
            {
                player.currHealth = 0;
                healthBar.value = player.currHealth;

                deathMessage = GameObject.Find("DeathMessage").GetComponent<TMP_Text>();
                deathMessage.enabled = true;
            }
            uiCountdown.ListPlayerDeath(player.Id);
        }
        Debug.Log($"{player.username} has died.");
    }

    [MessageHandler((ushort)ServerToClientId.playerScore)]
    private static void UpdateScore(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.score = message.GetInt();
            Debug.Log($"{player.username}'s new score is {player.score}.");
        }
    }

    [MessageHandler((ushort)ServerToClientId.newCameraOrientation)]
    private static void SetNewCameraOrientation(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))

            if (player.IsLocal)
            {
                controller = player.GetComponent<PlayerController>();
                Vector3 newOrientation = message.GetVector3();
                controller.SetCameraRotation(newOrientation);
            }
    }

    #endregion Messages
}