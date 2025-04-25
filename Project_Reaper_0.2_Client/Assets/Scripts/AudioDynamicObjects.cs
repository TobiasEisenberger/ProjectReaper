using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using UnityEngine;

public class AudioMovingObjects : MonoBehaviour
{
    private AudioSource audioSrc;
    private Vector3 lastPosition, lastRotation;
    private bool isPlaying;
    private int noMoveSince;
    private int noMoveThreshold;

    // Start is called before the first frame update
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        audioSrc.Stop();
        lastPosition = transform.position;
        lastRotation = transform.rotation.eulerAngles;
        isPlaying = false;
        noMoveSince = 0;
        noMoveThreshold = 60;
    }

    void FixedUpdate()
    {
       if(lastPosition != transform.position || lastRotation != transform.rotation.eulerAngles)
        {
            if(!isPlaying)
            {
                Debug.LogWarning("Playing Audio");
                audioSrc.Play();
                isPlaying = true;
            }
            noMoveSince = 0;
            lastPosition = transform.position;
            lastRotation = transform.rotation.eulerAngles;
           

        }
       else if(isPlaying && (lastPosition == transform.position && lastRotation == transform.rotation.eulerAngles))
        {
            // need to work with thresholds here, otherwise if the updates do not come in time the audio will stutter
            noMoveSince++;
            if(noMoveSince > noMoveThreshold)
            {
                Debug.LogWarning("Stopping Audio");
                audioSrc.Stop();
                isPlaying = false;
                noMoveSince = 0;
            }
            
        }
        
    }
}
