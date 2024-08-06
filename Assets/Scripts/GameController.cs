using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField] private int lifes = 2;
    public int Lifes { get => lifes; set => lifes = value; }

    private int hearts = 0;
    public int Hearts { get => hearts; set => hearts = value; }

    [SerializeField] private TextMesh lifesText;

    [SerializeField] private GameObject UIHeart;
    [SerializeField] private TextMesh UIHeartText;
    [SerializeField] public TextMesh numBombsText;
    [SerializeField] private Player player;
    //timer
    [SerializeField] private int maxTimeSeconds = 210;
    [SerializeField] private int currentTime = 0;
    private int timerActivationTime = 0;
    private bool isTimerActive = false;
    
    [SerializeField] private TextMesh timeText;
    private Color timeTextOriginalColor;
    //---

    [SerializeField] private TextMesh scoreText;
    [SerializeField] private TextMesh gameOverText;
    [SerializeField] private TextMesh continueGameText;
    [SerializeField] private TextMesh yesContinueGameText;
    [SerializeField] private TextMesh noContinueGameText;
    [SerializeField] private TextMesh scoreContinueGameText;
    [SerializeField] private SpriteRenderer continueGameBackgroundSprite;

    [SerializeField] private TextMesh youWinText;
    [SerializeField] private TextMesh playAgainText;
    [SerializeField] private TextMesh yesPlayAgainText;
    [SerializeField] private TextMesh noPlayAgainText;
    [SerializeField] private SpriteRenderer congratulationsBackgroundSprite;
    [SerializeField] private TextMesh congratulationsText;
    [SerializeField] private TextMesh pauseText;
    [SerializeField] private GameObject creditsTextsContainer;
    [SerializeField] private ParticleSystem[] fireworks= new ParticleSystem[2];
    [SerializeField] private SpriteRenderer playAgainBackgroundSprite;

    private static int score = 0;
    public int Score { get => score; set => score = value; }

    private int origScore = 0;
    public int OrigScore { get => origScore; set => origScore = value; }

    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    public List<Enemy> Enemies { get => enemies; set => enemies = value; }

    //Start Game Timer 
    [SerializeField] private float maxTimeSeconds_StartGameTimer = 3.1F;
    [SerializeField] private float currentTime_StartGameTimer = 0;
    private int timerActivationTime_StartGameTimer = 0;
    private bool isGameStarted = false;
    //---
    private MusicController musicController;
    private SoundEffectsController soundEffectsController;
    private MovementSoundEffectsController movementSoundEffectsController;


    // Start is called before the first frame update
    void Start()
    {
        timeTextOriginalColor = timeText.color;

        origScore= score;

        //Start Game Timer 
        currentTime_StartGameTimer = maxTimeSeconds_StartGameTimer;
        timerActivationTime_StartGameTimer = (int)Time.time;//execute only first time
        //---

        musicController = GameObject.FindObjectOfType<MusicController>();
        soundEffectsController = GameObject.FindObjectOfType<SoundEffectsController>();
        movementSoundEffectsController = GameObject.FindObjectOfType<MovementSoundEffectsController>();
    }


    // Update is called once per frame
    void Update()
    {
        PauseGame();

        StartGame();
        UpdateEnemiesList();

        Timer();
        
        UpdateUITexts();
    }

    //Start Game Timer 
    private void StartGame()
    {
        if (!isGameStarted)
        {
            float timeElapsed = (int)Time.time - timerActivationTime_StartGameTimer;
            currentTime_StartGameTimer = maxTimeSeconds_StartGameTimer - timeElapsed;
            if (currentTime_StartGameTimer <= 0)
            {
                currentTime_StartGameTimer = 0;

                player.gameObject.SetActive(true);
                enemies.ForEach(e => e.gameObject.SetActive(true));

                ActivateTimer();
                isGameStarted = true;
            }
        }
    }

    private void UpdateEnemiesList()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
            }
        }
    }

    private void PauseGame()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            if (Time.timeScale == 1)//game is unpaused
            {
                //Debug.Log("Paused Game!");
                soundEffectsController.PlayPauseGameSound();

                pauseText.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
            else if (Time.timeScale == 0)//game is paused
            {
                //Debug.Log("Unpaused Game!");
                pauseText.gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }

    public void EnableUIElements()
    {
        UIHeart.gameObject.SetActive(true);
        UIHeartText.gameObject.SetActive(true);
    }

    public void DisableUIElements()
    {
        UIHeart.gameObject.SetActive(false);
        UIHeartText.gameObject.SetActive(false);
    }

    public void LoseLife()
    {
        //Debug.Log("Lost Life");
        if (hearts > 0)
        {
            hearts -= 1;
            if (hearts == 0)
                DisableUIElements();
        }
        else
        {
            lifes -= 1;
        }
        int totalLifes = lifes + hearts;
        if (totalLifes < 0)
        {
            GameOver();
        }
        else
        {
            //Debug.Log("Lost Life without Gamer Over!");
            score = origScore;
        }
    }

    public void UpdateUITexts()
    {
        UpdateNumBombs3DText();

        lifesText.text = lifes.ToString();
        if (lifes <= 0)
        {
            lifesText.color = Color.red;
        }

        UIHeartText.text = hearts.ToString();
        UpdateTimeText();
        scoreText.text = score.ToString();
    }

    private void UpdateNumBombs3DText()
    {
        if (player.MaxNumberOfBombs == 2)
            numBombsText.color = Color.white;
        else if (player.MaxNumberOfBombs > 2)
            numBombsText.color = Color.blue;
        else
            numBombsText.color = Color.red;

        numBombsText.text = player.MaxNumberOfBombs.ToString();
    }

    private void ActivateTimer()
    {
        currentTime = maxTimeSeconds;

        timerActivationTime = (int)Time.time;//execute only first time
        isTimerActive = true;

        timeText.color = timeTextOriginalColor;
        UpdateTimeText();
    }

    void Timer()
    {
        if (isTimerActive)
        {
            int timeElapsed = (int)Time.time - timerActivationTime;
            currentTime = maxTimeSeconds - timeElapsed;

            if (currentTime <= 0)
            {
                currentTime = 0;
                //Debug.Log("Time Stop");
                isTimerActive = false;

                //Debug.Log("Lose Life");
                player.HandleLoseLifeCollision();
                Invoke("ActivateTimer", 2.5F);
            }
        }
    }

    void UpdateTimeText()
    {
        int minutes = currentTime / 60;
        int seconds = currentTime % 60;
        timeText.text = string.Format("{0}:{1:00}", minutes, seconds);

        if (isTimerActive)
        {
            if (currentTime <= (maxTimeSeconds / 4))
            {
                timeText.color = Color.red;
            }
        }
    }



    private void GameOver()
    {
        //Debug.Log("Game over");
        lifes = 0;

        movementSoundEffectsController.gameObject.SetActive(false);//Note:Necessary because the sound could be still playing at this time.//OR: movementSoundEffectsController.StopWalkingSoundInLoop();

        player.gameObject.SetActive(false);
        enemies.ForEach(e => e.gameObject.SetActive(false));

        isTimerActive = false;//stop time
        gameOverText.gameObject.SetActive(true);
        Invoke(nameof(ActivateContinueGameMenu), 2.5F);//Note:This time should be less or equal than the delay time in :Invoke("ActivateTimer", 2F); in Timer() method. if it is not less or equal the delay time, when you game over because the time, you will be able to see the reset of timer before Activate ContinueGameMenu
    }


    private void ActivateContinueGameMenu()
    {
        continueGameText.gameObject.SetActive(true);
        yesContinueGameText.gameObject.SetActive(true);
        noContinueGameText.gameObject.SetActive(true);
        scoreContinueGameText.text = score.ToString();
        scoreContinueGameText.gameObject.SetActive(true);
        continueGameBackgroundSprite.gameObject.SetActive(true);

        musicController.PlayGameOverMusic();

        Time.timeScale = 0;
    }


    public void YouWin()
    {
        movementSoundEffectsController.gameObject.SetActive(false);//Note: Necessary because the sound could be still playing at this time.//OR: movementSoundEffectsController.StopWalkingSoundInLoop();

        //Debug.Log("You win");
        player.gameObject.SetActive(false);
        enemies.ForEach(e => e.gameObject.SetActive(false));

        isTimerActive = false;//stop time
        youWinText.gameObject.SetActive(true);
        Invoke(nameof(ActivateCongratulationBackground), 2.5F);
    }

    private void ActivateCongratulationBackground()
    {
        congratulationsBackgroundSprite.gameObject.SetActive(true);
        congratulationsText.gameObject.SetActive(true);//Note: This has an animation (that takes 25 seconds)
        fireworks.ToList().ForEach(firework => firework.gameObject.SetActive(true));

        musicController.PlayYouWinGameMusic();

        soundEffectsController.SetMuteValue(true);//mute any bomb explosion sound etc when we are here
        
        Invoke(nameof(PlayFireworksSoundWithDelay),2F);
        Invoke(nameof(ActivateCredits), 25F);//Note: This delay time must be greater than the CongratulationsText Animation because we need to finish the animation appropriately.
    }

    private void PlayFireworksSoundWithDelay()
    {
        soundEffectsController.SetMuteValue(false);
        soundEffectsController.PlayFireworksSound();
    }

    private void ActivateCredits()
    {
        fireworks.ToList().ForEach(firework => firework.gameObject.SetActive(false));
        soundEffectsController.StopSound();

        congratulationsBackgroundSprite.GetComponent<AnimatorController>().enabled = true;//Activate the CongratulationBackground DarkeningEffect (That animations takes 8 seconds)
        Invoke(nameof(StopSoundDescreasingVolume), 2.0F);//animation "StopSoundDescreasingVolume" takes 4 seconds  
    }

    private void StopSoundDescreasingVolume()
    {
        musicController.StopSoundDescreasingVolume();
        Invoke(nameof(ActivateCreditsMusic), 6F);
    }

    private void ActivateCreditsMusic()
    {
        musicController.PlayCreditsMusic();
        Invoke(nameof(ActivateCreditsText), 0.1F);
    }

    private void ActivateCreditsText()
    {
        creditsTextsContainer.gameObject.SetActive(true); //this container has an animation (That animations takes 94 seconds)  
        Invoke(nameof(ActivatePlayAgainMenu), 95F); //Note: this delay time must be greater than the previous animation time (currently 94 seconds)
    }

    private void ActivatePlayAgainMenu()
    {
        congratulationsText.gameObject.SetActive(false);

        playAgainText.gameObject.SetActive(true);
        yesPlayAgainText.gameObject.SetActive(true);
        noPlayAgainText.gameObject.SetActive(true);
        scoreContinueGameText.text = score.ToString();
        scoreContinueGameText.gameObject.SetActive(true);
        playAgainBackgroundSprite.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    

}
