using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDestroyableBlock : MonoBehaviour
{

    private DestroyableBlock destroyableBlockHitByExplosion;
    public DestroyableBlock DestroyableBlockHitByExplosion { get => destroyableBlockHitByExplosion; set => destroyableBlockHitByExplosion = value; }



    // Start is called before the first frame update
    private void Start()
    {

        Destroy(gameObject,0.8F);
    }

   
    // Update is called once per frame
    private void Update()
    {

    }

    private void OnDestroy()
    {
        if(gameObject.scene.isLoaded)
            Destroy(destroyableBlockHitByExplosion.gameObject);
    }

}
