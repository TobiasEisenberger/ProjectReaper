using Riptide;
using UnityEngine;

public class BevelController : MonoBehaviour
{
    public bool isOn;
    private Animator animator;
    public Trap[] attachedTraps;
    [SerializeField]
    private bool isSingleChoiceBevel = false;
    [SerializeField]
    public BevelController[] otherBevels;
    private Vector3 myRotation;
    private Player activatingPlayer = null;

    private void Awake()
    {
        myRotation = this.transform.eulerAngles;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (transform.eulerAngles != myRotation)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, myRotation.y, myRotation.z);
        }
    }

    public void ToggleBevel()
    {
        // deactivate all other bevels as this is a single choice
        if(isSingleChoiceBevel)
        {
            foreach(BevelController bevel in otherBevels)
            {
                bevel.gameObject.SetActive(false);
                SpawnDynamicObjects.DisableObject(bevel.gameObject);

            }
        }

        // Move and activate all attached traps 
        // atm there is no reason to deactivate traps via the bevel
        foreach (Trap trap in attachedTraps)
        {
            if (trap != null)
            {
                trap.activate();
            }
        }
        //start the animation
        isOn = !isOn;
        animator.SetBool("IsOn", isOn);
        SendAnimationOrder();
    }

    public void AssignPlayer(Player player)
    {
        activatingPlayer = player;
    }

    #region Messages

    private void SendAnimationOrder()
    {
        foreach (Trap trap in attachedTraps)
        {
            if (trap != null)
            {
                Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.animationOrders);
                message.AddInt(SpawnDynamicObjects.Singleton.GetObjectIds()[this.gameObject]);
                message.AddBool(isOn);
                message.AddInt(SpawnDynamicObjects.Singleton.GetObjectIds()[trap.gameObject]);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }
    }

    #endregion Messages
}