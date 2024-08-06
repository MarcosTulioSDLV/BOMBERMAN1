using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {


    private Rigidbody2D myRigidbody;
    private Collider2D myCollider;
    private Vector3 originalPosition;
    private bool firstTimeCollidingWithExplosion = true;
    [SerializeField] private float speed = 2.6F;
    private float origSpeed;
    private Vector2 movementVector;
    private GameObject bombPrefab;
    private Bomb bomb;
    
    private List<Bomb> bombs = new List<Bomb>();//necessary to enable set Player.CountBomb-- from the bomb, when player has not lost the current life

    [SerializeField] private int countBombs = 0;
    public int CountBombs { get { return countBombs; } set { countBombs = value; } }

    [SerializeField] private int maxNumberOfBombsRaw = 2;//defines the maximum number of simultaneous bombs the player can place (given the current algorithm, it could be negative, so an additional value is needed to store the absolute value (positive only)
    [SerializeField] private int maxNumberOfBombs;//difines the absolute value (only positive number to maxNumberOfBombsRaw), because that value could be negative in some cases.
    public int MaxNumberOfBombs { get { return Mathf.Max(0, maxNumberOfBombsRaw); } }//difines the absolute value (only positive number to maxNumberOfBombs), because that value could be negative in some cases.

    private int origMaxNumberOfBombsRaw;

    [SerializeField] float timeOfPowerUps = 40F;

    private bool isEnabledFireUpState = false;
    private float timeToResetFireUpState = 0;
    [SerializeField] private float missingTimeInFireUpState = 0;//missing time to finish state "fireUpState"

    //---repleaceble powers up
    private bool isEnabledKickBombState = false;
    private float timeToResetKickBombState = 0;
    [SerializeField] private float missingTimeInKickBombState = 0;//missing time to finish state "kickBombState"

    private bool isEnabledRemoteControlBombState = false;
    private float timeToResetRemoteControlBombState = 0;
    [SerializeField] private float missingTimeInRemoteControlBombState = 0;//missing time to finish state "kickBombState" //---repleaceble powers up

    private bool isEnabledBombPasserState = false;
    private float timeToResetBombPasserState = 0;
    [SerializeField] private float missingTimeInBombPasserState = 0;//missing time to finish state "bombPasserState"

    private bool isEnabledBlockPasserState = false;
    private float timeToResetBlockPasserState = 0;
    [SerializeField] private float missingTimeInBlockPasserState = 0;//missing time to finish state "blockPasserState"

    private bool isEnabledIndestructibleArmorState = false;
    private float timeToResetIndestructibleArmorState = 0;
    [SerializeField] private float missingTimeInIndestructibleArmorState = 0;//missing time to finish state "indestructibleArmorState"

    private bool isEnabledSkullEffectsState = false;
    private float timeToResetSkullEffectsState = 0;
    [SerializeField] private float missingTimeInSkullEffectsState = 0;//missing time to finish state "skullEffectsState"
    private float speedInSkullEffectsState;

    private Color[] flashingColorsInPowerUpState = new Color[] { Color.red, new Color32(0xFF, 0xAF, 0x00, 0xFF) };//FFAF00
    private SpriteRenderer mySpriteRenderer;

    private bool isFlashingInPowerUpState = false;

    private bool isEnabledStageStartState = false;
    public bool IsEnabledStageStartState { get => isEnabledStageStartState; set => isEnabledStageStartState = value; }

    private float timeToResetStageStartState = 0;
    [SerializeField] private float missingTimeInStageStartState = 0;
    private bool isFlashingInStageStartState = false;
    private float timeOfStageStartState = 8F; 

    [SerializeField] float speedIncreaseWithRollerShoesItem = 0.8F;

    [SerializeField] private GameController gameController;

    private SoundEffectsController soundEffectsController;
    private MovementSoundEffectsController movementSoundEffectsController;//Note: It was necessary to have this controller for the player movement sound (separate from the controller for other sound effects "soundEffectsController").
    private bool firstTimeVelocityGreaterThan0 = true;

    private Animator myAnimator;


    private void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        origSpeed = speed;
        origMaxNumberOfBombsRaw = maxNumberOfBombsRaw;

        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;

        mySpriteRenderer = GetComponent<SpriteRenderer>();

        bombPrefab = (GameObject)Resources.Load("Prefabs/bomb_0", typeof(GameObject));

        speedInSkullEffectsState = (speed - (speedIncreaseWithRollerShoesItem)) * -1;//Slow velocity and *1 for inverted controls

        soundEffectsController = GameObject.FindObjectOfType<SoundEffectsController>();
        movementSoundEffectsController = GameObject.FindObjectOfType<MovementSoundEffectsController>();

        myAnimator = GetComponent<Animator>();

        EnableStageStartState();
    }

    // Update is called once per frame
    void Update()
    {
        SetMovementVector();
        PlaceDownBomb();

        DisableState(ref isEnabledFireUpState, timeToResetFireUpState, ref missingTimeInFireUpState);

        //---repleaceble powers up
        DisableState(ref isEnabledKickBombState, timeToResetKickBombState, ref missingTimeInKickBombState);

        DisableState(ref isEnabledRemoteControlBombState, timeToResetRemoteControlBombState, ref missingTimeInRemoteControlBombState);
        DestroyRemoteControlBomb();
        //---repleaceble powers up

        DisableState(ref isEnabledBombPasserState, timeToResetBombPasserState, ref missingTimeInBombPasserState);
        DisableState(ref isEnabledBlockPasserState, timeToResetBlockPasserState, ref missingTimeInBlockPasserState);

        DisableState(ref isEnabledIndestructibleArmorState, timeToResetIndestructibleArmorState, ref missingTimeInIndestructibleArmorState);
        StartCoroutine(FlashingInPowerUpState("IndestructibleArmorState"));

        DisableState(ref isEnabledSkullEffectsState, timeToResetSkullEffectsState, ref missingTimeInSkullEffectsState);
        StartCoroutine(FlashingInPowerUpState("SkullEffectState"));

        DisableState(ref isEnabledStageStartState, timeToResetStageStartState, ref missingTimeInStageStartState, true);
        StartCoroutine(FlashingInStageStartState());
    }

    private void EnableStageStartState()
    {
        isEnabledStageStartState = true;
        timeToResetStageStartState = Time.time + timeOfStageStartState;//Note: for this state is not necessary the viriable "missingTimeInStageStartState " (only as UI info), because the state will not be cumulative (if activated mutiple times at the same time)
    }

    private IEnumerator FlashingInPowerUpState(string powerUpStateName)
    {
        bool stateActivationFlag;
        int newColorIndex;
        if (powerUpStateName == "SkullEffectState")
        {
            stateActivationFlag = isEnabledSkullEffectsState;
            newColorIndex = 0;
        }
        else if (powerUpStateName == "IndestructibleArmorState")
        {
            stateActivationFlag = isEnabledIndestructibleArmorState;
            newColorIndex = 1;
        }
        else
        {
            stateActivationFlag = isEnabledSkullEffectsState;
            newColorIndex = 0;
        }

        if (stateActivationFlag && !isFlashingInPowerUpState)
        {
            isFlashingInPowerUpState = true;//We mark that the coroutine is running
            SetColor(ref flashingColorsInPowerUpState[newColorIndex]);
            //print("SET");
            yield return new WaitForSeconds(0.06F);
            SetColor(ref flashingColorsInPowerUpState[newColorIndex]);
            //print("RESET");
            isFlashingInPowerUpState = false;//We mark that the coroutine has finished
        }
    }

    private IEnumerator FlashingInStageStartState()
    {
        Color newColor = new Color32(0, 0, 0, 130);
        if (isEnabledStageStartState && !isFlashingInStageStartState)
        {
            isFlashingInStageStartState = true;//We mark that the coroutine is running
            SetColor(ref newColor);
            //print("SET");
            yield return new WaitForSeconds(0.002F);
            SetColor(ref newColor);
            //print("RESET");
            isFlashingInStageStartState = false;//We mark that the coroutine has finished
        }
    }

    private void SetColor(ref Color newColor)
    {
        //print("Switched color!");
        Color tempColor = mySpriteRenderer.color;
        mySpriteRenderer.color = newColor;
        newColor = tempColor;
    }


    private void FixedUpdate()
    {
        Move();
    }


    private void PlaceDownBomb()
    {
        if (Input.GetKeyDown(KeyCode.X) && Time.timeScale == 1)
        {
            if (countBombs < maxNumberOfBombsRaw)
            {
                //Debug.Log("Placing down bomb");
                soundEffectsController.PlayPlaceDownBombSound();

                UpdatePlacedDownBombsList();

                bomb = GameObject.Instantiate(bombPrefab, gameObject.transform.position, bombPrefab.transform.rotation).GetComponent<Bomb>();
                bomb.Player = gameObject.GetComponent<Player>();

                if (isEnabledFireUpState)
                    bomb.TypeOfExplosion = Bomb.MyTypeOfExplosion.strongExplosion;

                //---repleaceble powers up
                if (isEnabledKickBombState)
                {
                    bomb.TypeOfBomb = Bomb.MyTypeOfBomb.kickableBomb;
                }
                else if (isEnabledRemoteControlBombState)
                {
                    bomb.TypeOfBomb = Bomb.MyTypeOfBomb.remoteControlBomb;
                }
                bombs.Add(bomb);//necessary to enable set Player.CountBomb-- from the bomb, when player has not lost the current life
                //---repleaceble powers up

                countBombs++;
            }
        }
    }

    private void UpdatePlacedDownBombsList()
    {
        int i = 0;
        while (i < bombs.Count)
        {
            if (bombs[i] == null)
                bombs.RemoveAt(i);
            else
                i++;
        }
    }

    private void DestroyRemoteControlBomb()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            bombs.Where(bomb => bomb != null).Where(bomb => bomb.TypeOfBomb == Bomb.MyTypeOfBomb.remoteControlBomb)
                .ToList().ForEach(bomb => Destroy(bomb));
        }
    }

    private void SetMovementVector()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        //Plays movement animations
        if (Time.timeScale == 1)//Note: Only when the game is running (it is not paused)
        {
            myAnimator.SetFloat("Horizontal", isEnabledSkullEffectsState ? -x : x);
            myAnimator.SetFloat("Vertical", isEnabledSkullEffectsState ? -y : y);
        }

        movementVector = new Vector2(x, y);

        if (movementVector.magnitude>0 && firstTimeVelocityGreaterThan0)
        {
            movementSoundEffectsController.PlayWalkingSoundInLoop();
            firstTimeVelocityGreaterThan0 = false;
        }
        else if (movementVector.magnitude==0 && !firstTimeVelocityGreaterThan0)
        {
            firstTimeVelocityGreaterThan0 = true;
            movementSoundEffectsController.StopWalkingSoundInLoop();//Note: movementSoundEffectsController must also be disabled with: movementSoundEffectsController.gameObject.SetActive(false), (or this line executed: StopWalkingSoundInLoop()) in GameController: GameOver(), YouWin(). and also in ExitDoor: OnTriggerStay2D().
        }
    }

    private void Move()
    {
        float tempSpeed = isEnabledSkullEffectsState ? speedInSkullEffectsState : speed;
        myRigidbody.velocity = movementVector * tempSpeed;

        myAnimator.SetFloat("Speed", myRigidbody.velocity.sqrMagnitude);//Play movement animations
    }


    private void DisableState(ref bool stateActivationFlag,float timeToResetState,ref float missingTimeInState,bool isStageStartState= false)
    {
        if (stateActivationFlag)
        {
            if (Time.time >= timeToResetState)
            {
                //print("DISABLE STATE EFFECTS");
                stateActivationFlag = false;
                missingTimeInState = 0;
                //Note1: When the state is "StageStart State" it is necessary Wake up the collider for execute OnTriggerStay2D() even when player is not moving.
                //Note2: If player is not moving, for defualt OnTriggerStay2D() will not execute in order to get a better perfomance.
                if (isStageStartState)
                {
                    if (myCollider.attachedRigidbody.IsSleeping())
                        myCollider.attachedRigidbody.WakeUp();
                }
            }
            else
                missingTimeInState = timeToResetState - Time.time;
        }
    }


    private void ResetMaxNumberOfBombsDecreasing()
    {
        maxNumberOfBombsRaw--;
    }
    private void ResetMaxNumberOfBombsIncreasing()
    {
        maxNumberOfBombsRaw++;
    }

    private void ResetSpeed()
    {
        speed -= speedIncreaseWithRollerShoesItem;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Note1: In order to Add a new Enemy, it is necessary create a the new tag, setting it with the new tag, and finally add it in this "enemy Tags Array".
        //Note2: It is also requered, add it in the Bomb script.
        string[] enemyTagsArray = new string[] { "BallomEnemy", "BirdEnemy", "PassEnemy", "WormEnemy", "GhostEnemy", "GreenWormEnemy", "DuckEnemy" };
        if (enemyTagsArray.Any(enemyTag => collision.tag == enemyTag))
        {
            if (!isEnabledStageStartState)
                HandleLoseLifeCollision();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Explosion" && firstTimeCollidingWithExplosion)//NOTE: firstTimeCollidingWithExplosion is a necessary flag because the explosion collider was created with 2 internal colliders, so it would activate twice when the player is right in the middle of that collider element.
        {
            if (!isEnabledStageStartState && !isEnabledIndestructibleArmorState)
            {
                firstTimeCollidingWithExplosion = false;
                HandleLoseLifeCollision();//NOTE: firstTimeCollidingWithExplosion will be reset in this method
            }
            else
            {
                //Debug.Log("Protected for any state (StageStartState or IndestructibleArmorState)");
            }
        }
        //For Power Up Items
        if (collider.tag == "FireUpItem")
        {
            isEnabledFireUpState = true;
            timeToResetFireUpState = Time.time + timeOfPowerUps + missingTimeInFireUpState;
            Destroy(collider.gameObject);
        }
        if (collider.tag == "BombItem")
        {
            maxNumberOfBombsRaw++;
            Invoke("ResetMaxNumberOfBombsDecreasing", timeOfPowerUps);//Note:In this way of reset with Invoke, the states are not cumulative if several are activated at the same time.
            Destroy(collider.gameObject);
        }
        if(collider.tag == "MinusOneBombItem")
        {
            maxNumberOfBombsRaw--;
            Invoke("ResetMaxNumberOfBombsIncreasing", timeOfPowerUps);
            Destroy(collider.gameObject);
        }

        // THESE POWER UP ARE REPLACED , ALWAYS THE LAST ONE PEAKED UP WILL KEEP ACTIVE
        //---repleaceble powers up
        if (collider.tag == "KickBombItem")
        {
            isEnabledKickBombState = true;
            timeToResetKickBombState = Time.time + timeOfPowerUps + missingTimeInKickBombState;
            Destroy(collider.gameObject);

            //stop other repleaceble powers up
            isEnabledRemoteControlBombState = false;
            missingTimeInRemoteControlBombState = 0;
        }
        if (collider.tag == "RemoteControlBombItem")
        {
            isEnabledRemoteControlBombState = true;
            timeToResetRemoteControlBombState = Time.time + timeOfPowerUps + missingTimeInRemoteControlBombState;
            Destroy(collider.gameObject);

            //stop other repleaceble powers up
            isEnabledKickBombState = false;
            missingTimeInKickBombState = 0;
        }
        //---repleaceble powers up

        if (collider.tag == "RollerShoesItem")
        {
            speed += speedIncreaseWithRollerShoesItem;
            Invoke("ResetSpeed", timeOfPowerUps);//Note:This way of reset, the states are not cumulative if you activate several at the same time
            Destroy(collider.gameObject);
        }
        if (collider.tag == "BombPasserItem")
        {
            isEnabledBombPasserState = true;
            timeToResetBombPasserState = Time.time + timeOfPowerUps + missingTimeInBombPasserState;
            Destroy(collider.gameObject);
        }
        if (collider.tag == "BlockPasserItem")
        {
            isEnabledBlockPasserState = true;
            timeToResetBlockPasserState = Time.time + timeOfPowerUps + missingTimeInBlockPasserState;
            Destroy(collider.gameObject);
        }
        float delayTimeToInvokeMethod = 0.0F;
        if (collider.tag == "IndestructibleArmorItem")
        {
            if (isEnabledStageStartState)
            {
                float timeIntervalBetweenStates = 0.2F;
                delayTimeToInvokeMethod = missingTimeInStageStartState + timeIntervalBetweenStates;
            }
            Invoke(nameof(EnableIndestructibleArmorState),delayTimeToInvokeMethod);
            Destroy(collider.gameObject);
        }
        if (collider.tag == "SkullEffectsItem")
        {
            delayTimeToInvokeMethod += 0.25F;
            if (isEnabledStageStartState)
            {
                delayTimeToInvokeMethod += missingTimeInStageStartState;
            }
            Invoke(nameof(EnableSkullEffectsState),delayTimeToInvokeMethod);
            Destroy(collider.gameObject);
        }
        if (collider.tag == "HeartItem")
        {
            gameController.Hearts++;
            gameController.EnableUIElements();
            Destroy(collider.gameObject);
        }

        if(collider.tag=="FireUpItem" || collider.tag=="BombItem" || collider.tag== "KickBombItem" || collider.tag== "RemoteControlBombItem" || collider.tag== "RollerShoesItem" || collider.tag== "BombPasserItem" || collider.tag== "BlockPasserItem" || collider.tag== "IndestructibleArmorItem" || collider.tag == "HeartItem")
        {
            soundEffectsController.PlayGetItemSound();
        }
        else if(collider.tag == "SkullEffectsItem" || collider.tag == "MinusOneBombItem")
        {
            soundEffectsController.PlayGetNegativeEffectsItemSound();
        }

    }

    public void HandleLoseLifeCollision()
    {
        bombs.Where(bomb => bomb != null).ToList().ForEach(bomb => bomb.IsLinkedToPlayer = false);//necessary to enable set Player.CountBomb-- from the bomb, when player has not lost the current life

        bombs.Where(bomb => bomb != null).Where(bomb => bomb.TypeOfBomb == Bomb.MyTypeOfBomb.remoteControlBomb)
            .ToList().ForEach(bomb => bomb.TypeOfBomb = Bomb.MyTypeOfBomb.basicBomb);

        this.enabled = false;
        myCollider.enabled = false;
        myRigidbody.velocity = Vector2.zero;//Note:Stop the player with velocity is necessary, because only desable the script will not stop the player's inertia
        myAnimator.SetFloat("Speed", myRigidbody.velocity.sqrMagnitude);//Stop animation updating velocity 
        
        Invoke(nameof(DisableSpriteRendererWithDelay), 0.3F);//Note:This code must be executed before the method: ResetPlayerProperties()
        Invoke(nameof(ClearingOriginalPositionDestroyingBombsLocatedThere), 0.9F);//Note:This code must be executed before the method: ResetPlayerProperties()
        StartCoroutine(ResetPlayerProperties(1F));

        gameController.LoseLife();//Note:This line should be called last, because this method could disable the player when the game over and would produce an error for not calling the coroutine code.

        soundEffectsController.PlayPlayerDiesSound();
    }

    private void DisableSpriteRendererWithDelay()
    {
        mySpriteRenderer.enabled = false;
    }

    private IEnumerator ResetPlayerProperties(float delayTime)
    {
        //Debug.Log("RESET PLAYER PROPERTIES");

        speed = origSpeed;
        bombs = new List<Bomb>();
        countBombs = 0;
        maxNumberOfBombsRaw = origMaxNumberOfBombsRaw;

        isEnabledFireUpState = false;
        timeToResetFireUpState = 0;
        missingTimeInFireUpState = 0;

        isEnabledKickBombState = false;
        timeToResetKickBombState = 0;
        missingTimeInKickBombState = 0;

        isEnabledRemoteControlBombState = false;
        timeToResetRemoteControlBombState = 0;
        missingTimeInRemoteControlBombState = 0;

        isEnabledBombPasserState = false;
        timeToResetBombPasserState = 0;
        missingTimeInBombPasserState = 0;

        isEnabledBlockPasserState = false;
        timeToResetBlockPasserState = 0;
        missingTimeInBlockPasserState = 0;

        isEnabledIndestructibleArmorState = false;
        timeToResetIndestructibleArmorState = 0;
        missingTimeInIndestructibleArmorState = 0;

        isEnabledSkullEffectsState = false;
        timeToResetSkullEffectsState = 0;
        missingTimeInSkullEffectsState = 0;

        isFlashingInPowerUpState = false;

        isEnabledStageStartState = false;
        timeToResetStageStartState = 0;
        missingTimeInStageStartState = 0;
        isFlashingInStageStartState = false;

        speedInSkullEffectsState = (speed - (speedIncreaseWithRollerShoesItem)) * -1;

        CancelInvoke("ResetSpeed");//NOTE:Necessary if reset player and there is still any invoke method going to be call
        CancelInvoke("ResetMaxNumberOfBombsDecreasing");//NOTE:Necessary if reset player and there is still any invoke method going to be call
        CancelInvoke("ResetMaxNumberOfBombsIncreasing");//NOTE:Necessary if reset player and there is still any invoke method going to be call

        CancelInvoke("EnableIndestructibleArmorState");//NOTE:Necessary if reset player and there is still any invoke method going to be call. 
                                                       //NOTE2: Necessary because there could be a time interval between StageStartState and IndestructibleArmorState, so if the player loses a life in that interval, IndestructibleArmorState can be enabled after this code and would therefore be executed after losing the life.

        CancelInvoke("EnableSkullEffectsState");//NOTE:Necessary if reset player and there is still any invoke method going to be call. 
                                                //NOTE2: Necessary because there could be a time interval between StageStartState and SkullEffectsState, so if the player loses a life in that interval, SkullEffectsState can be enabled after this code and would therefore be executed after losing the life.

        //--
        yield return new WaitForSeconds(delayTime);
        this.enabled = true;//for Stop Script
        gameObject.transform.position = originalPosition;
        myCollider.enabled = true;
        mySpriteRenderer.enabled = true;
        firstTimeCollidingWithExplosion = true;
        EnableStageStartState();//NOTE:A delay is necessary to call this method, if there is no delay it will cause errors with the colors (flashing)
    }

    //Note: This will destroy bombs placed in the original position to prevent the player from being placed on top of them once restart after lost a life.
    private void ClearingOriginalPositionDestroyingBombsLocatedThere()
    {
        GameObject playerOriginalPositionMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerOriginalPositionMarker.name = "PlayerOriginalPositionMarker";
        //Nota1: Destroy immediate all default components except the transform.
        //Note2: it is necessary DestroyImmediate() here, since we could have a conflict with the previous default collider if it is not destroyed first.
        playerOriginalPositionMarker.GetComponents<Component>().ToList()
            .ForEach(component => { if (component.GetType() != typeof(Transform)) DestroyImmediate(component); });

        //Requered copy of componets and properties to the new "playerOriginalPositionMarker" object
        playerOriginalPositionMarker.transform.position = originalPosition;
        playerOriginalPositionMarker.transform.rotation = gameObject.transform.rotation;
        playerOriginalPositionMarker.transform.localScale = gameObject.transform.localScale;

        Component origColliderComponent = gameObject.GetComponent<CircleCollider2D>();
        if (playerOriginalPositionMarker.GetComponent<CircleCollider2D>() == null)
        {
            Component copiedComponent = playerOriginalPositionMarker.AddComponent(origColliderComponent.GetType());

            copiedComponent.GetComponent<CircleCollider2D>().offset = origColliderComponent.GetComponent<CircleCollider2D>().offset;
            copiedComponent.GetComponent<CircleCollider2D>().radius = origColliderComponent.GetComponent<CircleCollider2D>().radius;
            copiedComponent.GetComponent<CircleCollider2D>().isTrigger = true;
            copiedComponent.GetComponent<CircleCollider2D>().enabled = true;

            //Note:This is necessary to the detect the collision
            copiedComponent = playerOriginalPositionMarker.AddComponent(typeof(Rigidbody2D));
            copiedComponent.GetComponent<Rigidbody2D>().gravityScale = 0F;

            copiedComponent = playerOriginalPositionMarker.AddComponent(typeof(PlayerOriginalPositionMarker));
        }
        else
        {
            //print("We couldn't copy a component because there is an existing one there.");
        }
    }

    private void EnableIndestructibleArmorState()
    {
        isEnabledIndestructibleArmorState = true;
        timeToResetIndestructibleArmorState = Time.time + timeOfPowerUps + missingTimeInIndestructibleArmorState;
    }

    private void EnableSkullEffectsState()
    {
        isEnabledSkullEffectsState = true;
        timeToResetSkullEffectsState = Time.time + (timeOfPowerUps/2) + missingTimeInSkullEffectsState;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Bomb" && isEnabledBombPasserState)
        {
            //Debug.Log("Switch bomb from solid to trigger!");
            collision.transform.GetComponent<CircleCollider2D>().isTrigger = true;
        }
        if(collision.transform.tag== "DestroyableBlock" && isEnabledBlockPasserState)
        {
            //Debug.Log("Switch block from solid to trigger!");
            collision.transform.GetComponent<Collider2D>().isTrigger = true;
        }
    }


}
