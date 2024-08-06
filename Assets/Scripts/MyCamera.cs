using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour
{

    private Animator myAnimator;
    [SerializeField] private float animationDelayTime = 0.02F;

    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        Invoke("PlayAnimation", animationDelayTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayAnimation()
    {
        myAnimator.Play("CameraAnimation");
    }

}
