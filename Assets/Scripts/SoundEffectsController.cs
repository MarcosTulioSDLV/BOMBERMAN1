using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour
{

    private AudioSource myAudioSource;
    [SerializeField] private AudioClip fireworksSoundAudioClip;
    [SerializeField] private AudioClip bombExplodesAudioClip;
    [SerializeField] private AudioClip playerDiesAudioClip;
    [SerializeField] private AudioClip getItemAudioClip;
    [SerializeField] private AudioClip getNegativeEffectsItemAudioClip;
    [SerializeField] private AudioClip enemyDiesAudioClip;
    [SerializeField] private AudioClip placeDownBombAudioClip;
    [SerializeField] private AudioClip pauseGameAudioClip;


    // Start is called before the first frame update
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayFireworksSound()
    {
        myAudioSource.loop = true;
        myAudioSource.volume = 0.7F;
        myAudioSource.clip = fireworksSoundAudioClip;
        myAudioSource.Play();
    }

    public void StopSound()
    {
        myAudioSource.Stop();
    }

    public void PlayBombExplodesSound()
    {
        myAudioSource.PlayOneShot(bombExplodesAudioClip);
    }

    public void PlayPlayerDiesSound()
    {
        myAudioSource.PlayOneShot(playerDiesAudioClip);
    }

    public void PlayGetItemSound()
    {
        myAudioSource.PlayOneShot(getItemAudioClip);
    }

    public void PlayGetNegativeEffectsItemSound()
    {
        myAudioSource.PlayOneShot(getNegativeEffectsItemAudioClip);
    }

    public void PlayEnemyDiesSound()
    {
        myAudioSource.PlayOneShot(enemyDiesAudioClip);
    }

    public void PlayPlaceDownBombSound()
    {
        myAudioSource.PlayOneShot(placeDownBombAudioClip);
    }

    public void PlayPauseGameSound()
    {
        myAudioSource.PlayOneShot(pauseGameAudioClip);
    }


    public void SetMuteValue(bool muteValue)
    {
        myAudioSource.mute = muteValue;
    }

}
