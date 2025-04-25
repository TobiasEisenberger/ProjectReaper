using System.Collections.Generic;
using UnityEngine;

public class SawTrap : Trap
{
    [SerializeField]
    private float rpm = 1;

    [SerializeField]
    private List<GameObject> sawBladeList;

    [SerializeField]
    private AudioSource sawSoundEffect;
    private bool isPlaying;

    private float anglePerSecond;

    private void Start()
    {
        anglePerSecond = 360 * rpm / 60;
        isPlaying = false;
    }

    private void Update()
    {
        if (!isPlaying && isActive)
        {
            isPlaying = true;
            sawSoundEffect.Play();
        }
        if (isActive)
        {
            foreach (GameObject sawBlade in sawBladeList)
            {
                sawBlade.transform.Rotate(Vector3.up, anglePerSecond * Time.deltaTime, Space.World);
            }
        }
    }

    public override void deactivate()
    {
        // Dont deactivate in this case, because once activated the saw trap will move between two point infinitely
    }
}