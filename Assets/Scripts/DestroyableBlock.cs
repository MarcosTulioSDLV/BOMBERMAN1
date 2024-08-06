using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableBlock : MonoBehaviour
{

    private Collider2D myCollider;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            //Debug.Log("Switch block from trigger to solid!");
            myCollider.isTrigger = false;
        }
    }

}
