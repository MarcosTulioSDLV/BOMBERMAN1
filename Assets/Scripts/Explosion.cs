using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Explosion : MonoBehaviour {


    private float explosionEffect_Time=0.29f;
    private AnimatedDestroyableBlock animatedDestroyableBlockPrefab;
    private const int TOTAL_STAGES = 12;//Note: Update Setting to the number of stages!


    // Use this for initialization
    void Start () {

        Scene currentScene = SceneManager.GetActiveScene();
        string lastChars= currentScene.name.Substring(5);
        int currentStageNum = int.Parse(lastChars);
        if (currentStageNum >= 1 && currentStageNum <= (TOTAL_STAGES/2))
        {
            animatedDestroyableBlockPrefab = (AnimatedDestroyableBlock)Resources.Load("Prefabs/AnimatedDestroyableBlock1", typeof(AnimatedDestroyableBlock));
        }
        else 
        {
            animatedDestroyableBlockPrefab = (AnimatedDestroyableBlock)Resources.Load("Prefabs/AnimatedDestroyableBlock2", typeof(AnimatedDestroyableBlock));
        }

        Destroy(this.gameObject, explosionEffect_Time);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DestroyableBlock")
        {
            //Debug.Log("Destroyed: "+collision.name);

            collision.gameObject.SetActive(false);//disable DestroyableBlock before be detroyed by animatedDestroyableBlock
            //Note: Disable the gameobject doesn't stop the collider inside it, so this line is necessary to avoid the collision keep working after disable. (if the collider is still working it could produce errors)
            collision.GetComponent<Collider2D>().enabled = false;

            //instatiate the new AnimatedDestroyable Block for create the animation effect (that element will be responsable of destroying(after play animation) the DestroyableBlock passed as parameter)
            AnimatedDestroyableBlock animatedDestroyableBlock = GameObject.Instantiate(animatedDestroyableBlockPrefab, collision.transform.position, Quaternion.identity);
            animatedDestroyableBlock.DestroyableBlockHitByExplosion = collision.GetComponent<DestroyableBlock>();
        }
        if (collision.tag == "Bomb")
        {
            Destroy(collision.gameObject,0.2F);
        }
    }

}
