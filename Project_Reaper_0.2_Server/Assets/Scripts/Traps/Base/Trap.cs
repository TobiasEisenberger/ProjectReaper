using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    [SerializeField]
    protected bool isActive = false;

    public virtual void activate()
    {
        isActive = true;
        Debug.Log(name + " was activated!");
    }

    public virtual void deactivate()
    {
        isActive = false;
    }
}