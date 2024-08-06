using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerOriginalPositionMarker : MonoBehaviour
{
    //Note1: This Element will be created from Player script, in the method: ClearingOriginalPositionDestroyingBombsLocatedThere()
    //Note2: This will destroy bombs placed in the original position to prevent the player from being placed on top of them once restart after lost a life.

    // Start is called before the first frame update
    void Start()
    {
        DestroyMySelf();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DestroyMySelf()
    {
        float delayTime= 0.1F;
        Destroy(gameObject, delayTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bomb")
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }

}
