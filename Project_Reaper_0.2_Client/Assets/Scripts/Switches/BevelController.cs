using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class BevelController : MonoBehaviour
{
    private Vector3 myRotation;
    private Animator animator;
    private List<Trap> attachedTraps = new List<Trap>();
    [SerializeField] AudioSource activationAudio;

    private void Awake()
    {
        myRotation = this.transform.eulerAngles;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void toggleBevel(bool isOn)
    {
        animator.SetBool("IsOn", isOn);
        if(isOn )
        {
            activationAudio.Play();
        }
        ToggleTrapState(isOn);
    }

    private void LateUpdate()
    {
        if (transform.eulerAngles != myRotation)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, myRotation.y, myRotation.z);
        }
    }

    private void ToggleTrapState(bool isOn)
    {
        foreach (var trap in attachedTraps)
        {
            
            if (trap != null)
            {
                //var trapComponent = trap.GetComponent<Trap>();
                if (trap.GetType() != typeof(ExplosiveBarrelTrap))

                {
                    if (isOn)
                        trap.activate();
                    else
                        trap.deactivate();
                }

            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.animationOrders)]
    private static void SwitchAnimation(Message message)
    {
        int objectID = message.GetInt();
        bool isOn = message.GetBool();
        int trapId = message.GetInt();
        var bevelInstance = DynamicObjectUpdater.ObjectDictionary[objectID].GetComponent<BevelController>();

        // Check if attachedTraps is null before accessing it
        if (bevelInstance.attachedTraps == null)
        {
            bevelInstance.attachedTraps = new List<Trap>(); // Initialize the list
        }

        GameObject trap = DynamicObjectUpdater.GetObjectById(trapId);
        if (trap != null)
        {
            Trap trapComponent = trap.GetComponent<Trap>();

            if (!bevelInstance.attachedTraps.Contains(trapComponent)) // Check if trap is already attached
            {
                bevelInstance.attachedTraps.Add(trapComponent);
            }

        }

        bevelInstance.toggleBevel(isOn);
    }
}