using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSoundEffectsController : MonoBehaviour
{

    private AudioSource myAudioSource;
    [SerializeField] private AudioClip walkingAudioClip;

    //Note: It was necessary to have this controller for the player movement sound (separate from the controller for other sound effects "soundEffectsController").

    // Start is called before the first frame update
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayWalkingSoundInLoop()
    {
        myAudioSource.loop = true;
        myAudioSource.clip = walkingAudioClip;
        myAudioSource.Play();
    }

    public void StopWalkingSoundInLoop()
    {
        myAudioSource.loop = false;
        myAudioSource.Stop();
    }

}
