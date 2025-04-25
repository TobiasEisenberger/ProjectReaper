using UnityEngine;

public class DynamicObjectScript : MonoBehaviour
{
    private Vector3 oldPosition;
    private Quaternion oldRotation;
    public Vector3 OldPos { get { return oldPosition; }  set { oldPosition = value; } }
    public Quaternion OldRot { get {  return oldRotation; } set { oldRotation = value; } }

    private void Start()
    {
        oldPosition = transform.position;
        oldRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
       // could add postion changes event based and then add to list
    }

}