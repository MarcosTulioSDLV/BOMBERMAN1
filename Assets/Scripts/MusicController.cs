using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    
    private AudioSource audioSource;
    [SerializeField] private AudioClip stageIntroAudioClip;
    [SerializeField] private AudioClip mainThemeAudioClip;
    [SerializeField] private AudioClip gameOverAudioClip;
    [SerializeField] private AudioClip youWinGameAudioClip;
    [SerializeField] private AudioClip creditsAudioClip;

    private Animator myAnimator;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        myAnimator = GetComponent<Animator>();

        Invoke(nameof(PlayStageIntroSound),0.15F);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PlayStageIntroSound()
    {
        audioSource.volume = 1F;
        audioSource.clip = stageIntroAudioClip;
        audioSource.Play();

        Invoke("PlayMainThemeMusic", 3.6F);
    }

    private void PlayMainThemeMusic()
    {
        audioSource.clip = mainThemeAudioClip;
        audioSource.volume = 0.5F;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayGameOverMusic()
    {
        audioSource.clip = gameOverAudioClip;
        audioSource.Play();
    }

    public void PlayYouWinGameMusic()
    {
        audioSource.clip = youWinGameAudioClip;
        audioSource.Play();
    }

    public void StopSoundDescreasingVolume()
    {
        myAnimator.enabled = true;
    }

    public void PlayCreditsMusic()
    {
        myAnimator.enabled = false;//Disable Animator component with previous effect

        audioSource.volume = 0.5f;
        audioSource.clip = creditsAudioClip;
        audioSource.Play();
    }

    public void StopSound()
    {
        audioSource.Stop();
    }
    
    


}
