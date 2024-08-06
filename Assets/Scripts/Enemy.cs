using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour {


    private Player player;
    [SerializeField] private float movimentDistancePerStep = 1f;//distance of each step
    [SerializeField] private float speed = 0.022f;
    [SerializeField] private float collisionDistance = 1f;//distance to where the enemy detects the collision, distance from the LinecastAll line
    private enum TypeOfEnemy { normalEnemy, smartEnemy,simpleEnemyWithHorizontalMovement, simpleEnemyWithVerticalMovement};
    [SerializeField] private TypeOfEnemy typeOfEnemy;
    private Rigidbody2D myRigidbody;

    private Vector2 direction = Vector2.zero; 
    private Vector2 origPosition;
    private List<Vector2> allPosibleDirections = new List<Vector2>();
    private Vector2 targetPosition = Vector2.zero;
    private bool isCollidingWithBomb= false;
    private bool isCollidingWithBlock = false;
    //Note: Necessary for avoid enemy keep blocked when any bomb is placed near it's position (at the same position or very nearly)
    private bool firstFrameCollidingWithBombOrBlock = true;

    private GameController gameController;
    private GameObject scoreIncreaseEffectTextPrefab;
    private Animator myAnimator;
    [SerializeField] private bool missingWalkToLeftSprite = false;//Note: If the sprite sheet does not have the 'walk to left' sprite, then we need to flip the image with code.
    private SpriteRenderer mySpriteRenderer;
    private bool isGhostEnemy = false;
    private Color origColor;
    private bool isDuckEnemy = false;
    private SoundEffectsController soundEffectsController;


    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Use this for initialization
    void Start() { 

        origPosition = transform.position;

        myRigidbody = GetComponent<Rigidbody2D>();

        myAnimator = GetComponent<Animator>();
        //Plays movement animations
        if(myAnimator!=null)
            myAnimator.SetBool("IsMoving", true);

        direction = GetOriginalRandomDirection();

        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        scoreIncreaseEffectTextPrefab = (GameObject)Resources.Load("Prefabs/ScoreIncreaseEffectText", typeof(GameObject));

        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        isGhostEnemy = gameObject.tag == "GhostEnemy";
        if (isGhostEnemy)
        {
            origColor = mySpriteRenderer.material.color; 
        }
        isDuckEnemy = gameObject.tag == "DuckEnemy";

        soundEffectsController = GameObject.FindObjectOfType<SoundEffectsController>();
    }

    private Vector2 GetOriginalRandomDirection()//used with "Simple Enemy Horizonatal" or "Simple Enemy Vertical"  (it doesn't do anything with "Normal Enemy" or "Smart Enemy")
    {
        Vector2 tempDir=Vector2.zero;
        Vector2 finalDir= Vector2.zero;
        if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithHorizontalMovement)
            tempDir = Vector2.right;
        else if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithVerticalMovement)
            tempDir = Vector2.up;

        int randomVal = Random.Range(0, 2);
        switch (randomVal)
        { 
            case 0: finalDir = tempDir; break;
            case 1: finalDir = -tempDir; break;
        }
        return finalDir;
    }

    // Update is called once per frame
    void Update() {
        DrawLines();
    }

    private void FixedUpdate()
    {
        //Code to Move Enemy

        if (!isGhostEnemy && !isDuckEnemy)
        { 
            if ((isCollidingWithBomb || isCollidingWithBlock) && firstFrameCollidingWithBombOrBlock)//When the enemy has not yet finished his route. (has not completed the full step) and is colliding with the bomb or block, he must turn to go in the opposite position
            {
                SetDirectionAndPositionWhenIsCollidingBeforeCompleteTheStep();//when colliding on the route before complete the step
            }
        }

        targetPosition = origPosition + (direction * movimentDistancePerStep);//destination position. Note: origPosition and direction are updated each time the method SetDirectionAndPositionToNextStep is executed to make the enemy move one more step.
        if ((Vector2)gameObject.transform.position == targetPosition)//if the destination position is reached (the step is completed), a new direction for the next step should be obtained.
        {
            SetDirectionAndPositionToNextStep();
        }
        if (!isGhostEnemy && !isDuckEnemy)
        {
            if (!isCollidingWithBomb && !isCollidingWithBlock)
                firstFrameCollidingWithBombOrBlock = true;
        }

        //print("Velocity: " + myRigidbody.velocity);

        Move();
    }

    private void SetDirectionAndPositionWhenIsCollidingBeforeCompleteTheStep()//when colliding on the route before complete the step
    {
        if (!isGhostEnemy && !isDuckEnemy)
        {
            //Note1: It's necessary to prevent the execution of this method when the enemy is initially blocked (by blocks) and any bomb is placed inside the enemy.
            //Note2: Without this validation, the method would update the targetPosition and direction, causing the enemy to move through the blocks as if they didn't exist.
            if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithHorizontalMovement || typeOfEnemy == TypeOfEnemy.simpleEnemyWithVerticalMovement)
            {
                float remainingDistanceToTarget;
                if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithHorizontalMovement)
                {
                    float remainingDistanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);
                    remainingDistanceToTarget = remainingDistanceToTargetX;
                }
                else //if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithVerticalMovement)
                {
                    float remainingDistanceToTargetY = Mathf.Abs(targetPosition.y - transform.position.y);
                    remainingDistanceToTarget = remainingDistanceToTargetY;
                }
                if (remainingDistanceToTarget == 0)
                {
                    return;
                }
            }
        }

        origPosition = targetPosition;
        direction = -direction;

        firstFrameCollidingWithBombOrBlock = false;
    }

    private void SetDirectionAndPositionToNextStep()//Update the new direction and update the position to move the enemy to the next position. 
    {
        string[] collisionTags = { "DestroyableBlock", "AnimatedDestroyableBlock", "UnDestroyableBlock", "Bomb" };//Tags used to validate the collision with the method IsColliding

        if (isGhostEnemy)
            collisionTags = new string[] { "UnDestroyableBlock" };
        else if (isDuckEnemy)
            collisionTags = new string[] { "DestroyableBlock", "AnimatedDestroyableBlock", "UnDestroyableBlock" };

        if (typeOfEnemy == TypeOfEnemy.simpleEnemyWithHorizontalMovement || typeOfEnemy == TypeOfEnemy.simpleEnemyWithVerticalMovement)
        {
            bool isCollidingInDirection = IsColliding(transform.position, direction, collisionTags);
            bool isCollidingInContraryDirection = IsColliding(transform.position, -direction, collisionTags);

            if (!isCollidingInDirection)
            {
                //go to the current direction
                origPosition = transform.position;
            }
            else if(!isCollidingInContraryDirection)
            {
                //go to the contrary direction
                direction = -direction; 
                origPosition = transform.position;
            }
            else
            {
                //print("BLOCKED");
                if (!isGhostEnemy && !isDuckEnemy)
                {
                    //Note: Necessary for avoid enemy keep blocked when any bomb is placed near it's position (at the same position or very nearly)
                    if (isCollidingWithBomb || isCollidingWithBlock)
                    {
                        string[] innerCollisionTags = new string[] { "DestroyableBlock", "AnimatedDestroyableBlock", "UnDestroyableBlock" };//same original tags excluding bomb
                        bool isCollidingWithBlockInDirection = IsColliding(transform.position, direction, innerCollisionTags);
                        bool isCollidingWithBlockInContraryDirection = IsColliding(transform.position, -direction, innerCollisionTags);
                        if (!isCollidingWithBlockInDirection)
                        {
                            origPosition = transform.position;
                        }
                        else if (!isCollidingWithBlockInContraryDirection)
                        {
                            direction = -direction;
                            origPosition = transform.position;
                        }
                        else
                        {
                            //print("Blocked by any Block");
                        }
                    }
                }
            }
        } //normalEnemy or smartEnemy
        else
        {
            allPosibleDirections = GetNoCollidingDirections(collisionTags);//List used to store the possible free directions (directions not blocked by blocks or bombs)

            if (allPosibleDirections.Count == 1)//There is only one direction in which it can move; in this case, it should go in that direction.
            {
                direction = allPosibleDirections[0];
                origPosition = transform.position;
            }
            else if (allPosibleDirections.Count > 1)//There are several directions in which it can move.
            {
                if (typeOfEnemy == TypeOfEnemy.normalEnemy)//The enemy moves randomly through the stage (does not chase the player).
                {
                    direction = GetRandomDirection();
                }
                else if (typeOfEnemy == TypeOfEnemy.smartEnemy)
                {
                    int randomNum = Random.Range(0, 5);
                    bool smartEnemy = false;

                    switch (randomNum)
                    {
                        case 0:
                        case 1:
                        case 2: smartEnemy = true; break; //60% probability
                        case 3:
                        case 4: smartEnemy = false; break; //40% probability
                    }

                    if (smartEnemy)//The enemy pursues the player and avoids returning to a position with a bomb.
                    {
                        RemoveContraryDirection(allPosibleDirections);//Avoid returning to a position with a bomb! Remove the opposite direction when necessary (that is, when the enemy is colliding with a bomb on the opposite side, or when colliding with a bomb in the previous position).
                        direction = allPosibleDirections[IndexOfClosestDirectionToPlayer()];
                    }
                    else //The enemy behaves the same way as: TypeOfEnemy.normalEnemy
                        direction = GetRandomDirection();
                }
                origPosition = transform.position;
            }
            else//There is no direction to move.
            {
                //print("BLOCKED");
                if (!isGhostEnemy && !isDuckEnemy)
                {
                    //Note: Necessary for avoid enemy keep blocked when any bomb is placed near it's position (at the same position or very nearly)
                    if (isCollidingWithBomb || isCollidingWithBlock)
                    {
                        string[] innerCollisionTags = new string[] { "DestroyableBlock", "AnimatedDestroyableBlock", "UnDestroyableBlock" };//same original tags excluding bomb
                        allPosibleDirections = GetNoCollidingDirections(innerCollisionTags);
                        if (allPosibleDirections.Count > 0)
                        {
                            direction = GetRandomDirection();
                            origPosition = transform.position;
                        }
                        else
                        {
                            //print("Blocked by any Block");
                        }
                    }
                }
            }
        }
    
    }

    private List<Vector2> GetNoCollidingDirections(params string[] collisionTags)//get directions that are free (without any collision)
    {
        List<Vector2> allDirections = new List<Vector2>() { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        List<Vector2> noCollidingDirections= allDirections.Where(dir=>!IsColliding(transform.position,dir,collisionTags)).ToList(); 
        return noCollidingDirections;
    }

    private void RemoveContraryDirection(List<Vector2> allPosibleDirections)//Avoid returning to a position with a bomb!, Remove the opposite direction when necessary (that is, when an enemy is colliding with a bomb on the opposite side, or even when in the previous position he is colliding with a bomb)
    {
        //METHOD1 and METHOD2 decide whether to remove the opposite direction or not, depending on the need to allow or prevent the enemy from returning on its path.

        //METODO1: allows the enemy to return on his path, when the player is closer to the other side
        Vector2 contraryDirection = -direction;
        Vector2 previousPos = (Vector2)transform.position + (contraryDirection * movimentDistancePerStep);
        List<Vector2> allDirectionsExceptTheCurrentDir = GetAllDirectionsExcept(direction);

        //METODO2: enemy never returns from his path
        //allPosibleDirections.Remove(contraryDirectionCode);

        //If the enemy is currently colliding with a bomb (for the contraryDirection), or collided with a bomb in the previous position
        if (IsColliding(transform.position, contraryDirection, "Bomb") || IsCollidingInAnyDirection(previousPos, allDirectionsExceptTheCurrentDir, "Bomb"))
        {
            //Debug.LogError("doesn't return");
            allPosibleDirections.Remove(contraryDirection);
        }
    }

    private List<Vector2> GetAllDirectionsExcept(Vector2 direction)
    {
        List<Vector2> allDirectionsExceptDirection = new List<Vector2>() { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        allDirectionsExceptDirection.Remove(direction);
        return allDirectionsExceptDirection;
    }

    private bool IsCollidingInAnyDirection(Vector2 pos, List<Vector2> directions, params string[] tags)
    {
        System.Func<Vector2, bool> isColliding = (dir) => IsColliding(pos, dir, tags);
        return directions.Any(isColliding);
    }


    private Vector2 GetRandomDirection()//Note: although it generates random direction most of the time (90%), there is also a (10%) probability of returning via the previous path
    {
        //Note: This validation is necessary for using this method after call: allPosibleDirections = GetNoCollidingDirections(innerCollisionTags); because with innerCollisionTags it could get even 1 direction (not serveral directions, and then get an error index out of range) 
        if (allPosibleDirections.Count > 1)
        {
            int randomVal = Random.Range(0, 10);
            bool enemyCanComeBack = true;
            switch (randomVal)
            {
                case 0: enemyCanComeBack = true; break;//10% probability of returning
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9: enemyCanComeBack = false; break;//90% probability of not returning
            }

            if (!enemyCanComeBack)
            {
                Vector2 contraryDirection = -direction;
                allPosibleDirections.Remove(contraryDirection);
            }
        }

        int randomIndex = Random.Range(0, allPosibleDirections.Count);
        return allPosibleDirections[randomIndex];
    }


    private int IndexOfClosestDirectionToPlayer()
    {
        System.Func<Vector2, float> distanceToPlayer = (dir) =>
        {
            Vector2 nextPosInDir = (Vector2)transform.position + (dir * movimentDistancePerStep);
            return Vector2.Distance(nextPosInDir, player.transform.position);
        };

        System.Func<Vector2, Vector2, Vector2> closestDirectionToPlayer = (dir1, dir2) =>
        {
            float distanceDir1ToPlayer = distanceToPlayer(dir1);
            float distanceDir2ToPlayer = distanceToPlayer(dir2);
            if (distanceDir1ToPlayer <= distanceDir2ToPlayer)
                return dir1;
            else
                return dir2;
        };

        Vector2 closestDir = allPosibleDirections.Aggregate((dir1, dir2) => closestDirectionToPlayer(dir1, dir2));
        return allPosibleDirections.IndexOf(closestDir);
    }

    private void Move()
    {   
        Vector2 p = Vector2.MoveTowards(gameObject.transform.position,targetPosition, speed);
        myRigidbody.MovePosition(p);

        PlayAnimations();
    }

    private void PlayAnimations()
    {
        //Plays movement animations
        if (myAnimator != null)
        {
            myAnimator.SetFloat("Horizontal", direction.x);
            myAnimator.SetFloat("Vertical", direction.y);

            //Note: If the sprite sheet does not have the 'walk to left' sprite, then we need to flip the image with code.
            if (missingWalkToLeftSprite)
            {
                int directionXSign = (int)Mathf.Sign(direction.x);
                if (directionXSign < 0)
                    SetSpriteRotation(180);//Flip the image to create the missing sprite to "walk to left"
                else 
                    SetSpriteRotation(0);//Set the image to the normal
            }
        }
    }

    private void SetSpriteRotation(float rotationY)
    {
        float originalRotationX = transform.rotation.eulerAngles.x;
        float originalRotationZ = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(originalRotationX, rotationY, originalRotationZ);
    }

    private bool IsColliding(Vector2 position,Vector2 direction, params string[] tags)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(position, position + (direction * collisionDistance));
        foreach (RaycastHit2D hit in hits)
        {
            if (tags.Any(tag => tag == hit.collider.tag))
                return true;
        }
        return false;
    }

    private void DrawLines()
    {
        Vector2 position = gameObject.transform.position;
        Debug.DrawLine(position, position + (Vector2.right * collisionDistance), Color.red);
        Debug.DrawLine(position, position + (Vector2.left * collisionDistance), Color.red);
        Debug.DrawLine(position, position + (Vector2.up * collisionDistance), Color.red);
        Debug.DrawLine(position, position + (Vector2.down * collisionDistance), Color.red);
    }

    private void OnDestroy()
    {
        //Note1: Necessary because the error: "Some objects were not cleaned up when closing the scene", if we change the scene without destroy this element
        //Note2: It's necessary because it also destroys enemies (and calls this line) when the scene reloads again after the game over and the player selects continue, so in that case we don't want to add any scores.
        if (gameObject.scene.isLoaded)
        {
            soundEffectsController.PlayEnemyDiesSound();

            GameObject scoreIncreaseEffectText = GameObject.Instantiate(scoreIncreaseEffectTextPrefab, transform.position, Quaternion.identity);
            int score = 0;
            switch (gameObject.tag)
            {
                case "BallomEnemy": score = 100; break;
                case "WormEnemy": score = 100; break;
                case "GreenWormEnemy": score = 200; break;
                case "PassEnemy": score = 300; break;
                case "DuckEnemy": score = 300; break;
                case "BirdEnemy": score = 400; break;
                case "GhostEnemy": score = 500; break;
            }
            scoreIncreaseEffectText.GetComponent<TextMesh>().text = score.ToString();
            gameController.Score += score;

        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDuckEnemy && collision.tag == "Bomb" && collision.GetComponent<Rigidbody2D>().velocity.magnitude == 0)
        {
            //Note: The Duck enemy can Deactivate bombs when it collides with them
            //print("Duck Ate the Bomb!");
            collision.GetComponent<Bomb>().IsBombDeactivatedForEnemy = true;
            Destroy(collision.gameObject);
        }

        if (collision.tag == "Bomb")
        {
            isCollidingWithBomb = true;
        }
        if (collision.tag == "Explosion")
        {
            Destroy(gameObject,0.1f);
        }
        //Note: If you want to add a new Tag element here, analyze if it is also necessary to add it in the "collisionTags" inside the method: "SetDirectionAndPositionToNextStep()"
        if (collision.tag == "DestroyableBlock" || collision.tag == "AnimatedDestroyableBlock" || collision.tag == "UnDestroyableBlock")
        {
            isCollidingWithBlock = true;
        }

        if (isGhostEnemy)
        {
            if (collision.tag == "Bomb" || (collision.tag == "DestroyableBlock" || collision.tag == "AnimatedDestroyableBlock"))
            {
                Color newColor = mySpriteRenderer.material.color;
                newColor.a = 0.7F;//Change color to transparent
                mySpriteRenderer.material.color = newColor;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Bomb")
        {
            //Debug.Log("EXIT COLLISION ENEMY WITH BOMB");
            isCollidingWithBomb = false;
        }
        //Note: If you want to add a new Tag element here, analyze if it is also necessary to add it in the "collisionTags" inside the method: "SetDirectionAndPositionToNextStep()"
        if (collision.tag == "DestroyableBlock" || collision.tag == "AnimatedDestroyableBlock" || collision.tag == "UnDestroyableBlock")
        {
            isCollidingWithBlock = false;
        }

        if (isGhostEnemy)
        {
            if (collision.tag == "Bomb" || (collision.tag == "DestroyableBlock" || collision.tag == "AnimatedDestroyableBlock"))
            {
                mySpriteRenderer.material.color = origColor;
            }
        }
    }

}
