using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour {


    private GameObject basicExplosionPrefab;
    private GameObject leftStrongExplosionPrefab;
    private GameObject rightStrongExplosionPrefab;
    private GameObject upStrongExplosionPrefab;
    private GameObject downStrongExplosionPrefab;

    [SerializeField] private float explosionDistance= 1F;
    [SerializeField] private float explosionTime = 4F;
    private Player player;
    public Player Player{ get{return player;}set { player = value; }}
    private bool isLinkedToPlayer= true;//necessary to enable set Player.CountBomb-- when player has not lost the current life
    public bool IsLinkedToPlayer { get => isLinkedToPlayer; set => isLinkedToPlayer = value; }


    public enum MyTypeOfExplosion { basicExplosion, strongExplosion };
    public MyTypeOfExplosion TypeOfExplosion { get; set; }

    [SerializeField] private Sprite strongExplosionBomb;

    public enum MyTypeOfBomb { basicBomb , kickableBomb , remoteControlBomb};//basicBomb means bomb with no special effects
    public MyTypeOfBomb TypeOfBomb { get; set; }

    private Rigidbody2D myRigidbody;

    private SoundEffectsController soundEffectsController;

    private bool isBombDeactivatedForEnemy = false;//Note:The Duck enemy can Deactivate bombs when it collides with them
    public bool IsBombDeactivatedForEnemy { get => isBombDeactivatedForEnemy; set => isBombDeactivatedForEnemy = value; }


    private void Awake(){
   
    }

    // Use this for initialization
    void Start ()
    {
        SetBombProperties();
        basicExplosionPrefab = (GameObject)Resources.Load("Prefabs/explosion_1", typeof(GameObject));
        leftStrongExplosionPrefab = (GameObject)Resources.Load("Prefabs/explosionBig_left", typeof(GameObject));
        rightStrongExplosionPrefab = (GameObject)Resources.Load("Prefabs/explosionBig_right", typeof(GameObject));
        upStrongExplosionPrefab = (GameObject)Resources.Load("Prefabs/explosionBig_up", typeof(GameObject));
        downStrongExplosionPrefab = (GameObject)Resources.Load("Prefabs/explosionBig_down", typeof(GameObject));

        myRigidbody = GetComponent<Rigidbody2D>();

        soundEffectsController = GameObject.FindObjectOfType<SoundEffectsController>();
    }

    // Update is called once per frame
    void Update () {

        DestroyMySelf();

        DrawLines();
    }

    private void DestroyMySelf()//Note:This method is called in update because we can set the "Type of Bomb" from Player script (in HandleLoseLifeCollision), so we need that be updated properly.
    {
        if (TypeOfBomb != MyTypeOfBomb.remoteControlBomb)
            Destroy(this.gameObject, explosionTime);
    }

    private void SetBombProperties()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (TypeOfExplosion == MyTypeOfExplosion.strongExplosion)
        {
            spriteRenderer.sprite = strongExplosionBomb;
        }

        if (TypeOfBomb == MyTypeOfBomb.kickableBomb)
        { 
            Rigidbody2D myRigidbody = GetComponent<Rigidbody2D>();
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;
            spriteRenderer.color = Color.blue;
        }
        else if(TypeOfBomb == MyTypeOfBomb.remoteControlBomb)
        {
            spriteRenderer.color = new Color32(0xFF, 0x06, 0x00, 0xFF);//FF0600
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Note1: In order to Add a new Enemy, it is necessary create a the new tag, setting it with the new tag, and finally add it in this "enemy Tags Array". 
        //Note2: It is also requered, add it in the Player script.
        //Note3: "GhostEnemy" must not be in this array because it is invincible (it won't collide with bomb)  
        string[] enemyTagsArray = new string[] { "BallomEnemy", "BirdEnemy", "PassEnemy", "WormEnemy", "GreenWormEnemy", "DuckEnemy" };
        if (enemyTagsArray.Any(enemyTag => collision.tag == enemyTag) && (TypeOfBomb == MyTypeOfBomb.kickableBomb) && (myRigidbody.velocity.magnitude > 0))
        {
            //Debug.Log("BOMB COLLISION WITH ENEMY");
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
  
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Note: bomb initially starts as a trigger so that the player can be inside it when it is placed down
        //This method makes the bomb no longer trigger once the player leaves it, that is, this makes it turn to a solid state.
        //Additionally, this method is also responsible for making the bombs return to solid ground after the player has converted them to trigger. (whenever state "bombPasserState" is active)
        if (collision.tag == "Player")
        {
            //Debug.Log("Switch bomb from trigger to solid!");
            gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
        }
    }

    private void OnDestroy()
    {
        if (isLinkedToPlayer)
            player.CountBombs--;

        if (isBombDeactivatedForEnemy)//Note: The Duck enemy can Deactivate bombs when it collides with them
            return;

        if (gameObject.scene.isLoaded)//Note: Necessary because the error: "Some objects were not cleaned up when closing the scene", if we change the scene without destroy this element
            ActivateExplosionEffect();
     
        if (gameObject.scene.isLoaded)
            soundEffectsController.PlayBombExplodesSound();
    }

    private void ActivateExplosionEffect()
    {
        if (TypeOfExplosion == MyTypeOfExplosion.basicExplosion)//normal explosion type(normal pump range)
        {
            GameObject.Instantiate(basicExplosionPrefab, gameObject.transform.position, gameObject.transform.rotation);
        }
        else if (TypeOfExplosion == MyTypeOfExplosion.strongExplosion)//large explosion type (dual pump range)
        {
            GameObject leftExplosion = GameObject.Instantiate(leftStrongExplosionPrefab, transform.position, transform.rotation);
            SetExplosionProperties(leftExplosion, Vector2.left);//change position and scale (when necessary and depending on whether it is colliding)

            GameObject rightExplosion = GameObject.Instantiate(rightStrongExplosionPrefab, transform.position, transform.rotation);
            SetExplosionProperties(rightExplosion, Vector2.right);
            
            GameObject upExplosion = GameObject.Instantiate(upStrongExplosionPrefab, transform.position, transform.rotation);
            SetExplosionProperties(upExplosion, Vector2.up);
       
            GameObject downExplosion = GameObject.Instantiate(downStrongExplosionPrefab, transform.position, transform.rotation);
            SetExplosionProperties(downExplosion, Vector2.down);
        }
    }

    private void SetExplosionProperties(GameObject explosion, Vector2 direction)//change position and scale (when necessary and depending on whether it is colliding)
    {
        string[] tags = { "UnDestroyableBlock" };
        float distance = 0.7f; 
        float scale = 0.5f;

        if (IsCollidingTheBomb(direction, tags))
        {
            SetExplosionPosition(explosion,direction, distance * 0.6f);
            SetExplosionScale(explosion,scale);
        }
        else
        {
            SetExplosionPosition(explosion,direction, distance);
        }
    }

    private void SetExplosionPosition(GameObject obj,Vector2 direction, float distance)
    {
        Vector2 tempPos = obj.transform.position;
        if (direction == Vector2.right)
            tempPos.x += distance;
        else if (direction == Vector2.left)
            tempPos.x -= distance;
        else if (direction == Vector2.up)
            tempPos.y += distance;
        else if (direction == Vector2.down)
            tempPos.y -= distance;

        obj.transform.position = tempPos;
    }

    private void SetExplosionScale(GameObject obj, float scaleFactor)
    {
        Vector3 newScale = obj.transform.localScale;
        newScale.x = newScale.x * scaleFactor;
        newScale.y = newScale.y * scaleFactor;
        obj.transform.localScale = newScale;
    }

    private void DrawLines()
    {
        Vector2 pos = transform.position;
        Debug.DrawLine(pos, pos + (Vector2.left * explosionDistance),Color.red);
        Debug.DrawLine(pos, pos + (Vector2.right * explosionDistance), Color.red);
        Debug.DrawLine(pos, pos + (Vector2.up * explosionDistance), Color.red);
        Debug.DrawLine(pos, pos + (Vector2.down * explosionDistance), Color.red);
    }

    private bool IsCollidingTheBomb(Vector2 direction,params string[] collisionTags)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, (Vector2)transform.position + (direction * explosionDistance));
        System.Func<RaycastHit2D, bool> hasCollisionTag = (hit) => collisionTags.Any((tag)=>tag==hit.transform.tag); 
        return hits.Any(hasCollisionTag);
    }


}
