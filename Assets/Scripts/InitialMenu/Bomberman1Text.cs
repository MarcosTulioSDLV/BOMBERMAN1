using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomberman1Text : MonoBehaviour
{

    private Animator myAnimator;
    private float animationDelayTime = 1.6F;

    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        Invoke("PlayAnimationWithDelay",animationDelayTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayAnimationWithDelay()
    {
        myAnimator.Play("Bomberman1TextAnimation");
    }

}
