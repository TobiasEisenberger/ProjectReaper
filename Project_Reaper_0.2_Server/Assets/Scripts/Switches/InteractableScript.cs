using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableScript : MonoBehaviour
{
    [SerializeField] private bool isInRange;
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private UnityEvent interactAction;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private SphereCollider interactableCollider;

    private void Awake()
    {
        interactableCollider = GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out playerMovementScript))
            {
                isInRange = true;
                Debug.Log($"interactable: {playerMovementScript.IsInteracting}");
                if (playerMovementScript.IsInteracting)
                {
                    interactAction.Invoke();
                    DisableInteractable();
                    StartCoroutine(EnableInteractable());
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out playerMovementScript))
            {
                isInRange = true;
                if (playerMovementScript.IsInteracting)
                {
                    interactAction.Invoke();
                    DisableInteractable();
                    StartCoroutine(EnableInteractable());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out playerMovementScript))
            {
                isInRange = false;
            }
        }
    }

    private void DisableInteractable()
    {
       if(interactableCollider != null)
        {
            interactableCollider.enabled = false;
        }
    }

    private IEnumerator EnableInteractable()
    {
        yield return new WaitForSeconds(cooldownDuration);
        if (interactableCollider !=null)
        {
            interactableCollider.enabled = true;
        }
    }
}