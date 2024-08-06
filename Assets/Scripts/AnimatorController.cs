using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator myAnimator;

    [SerializeField] private float animationDelayTime = 0.0F;
    //Note: for
    //StartStageText set this value to: 1.0F , animationName: StartStageTextAnimation
    //CongratulationsText set this value to:  0.0F, animationName: CongratulationsTextAnimation
    //CongratulationBackgroundDarkeningEffect set this value to: 5F, animationName: CongratulationBackgroundDarkeningEffect

    [SerializeField] private string animationName= "StartStageTextAnimation";
    //Note: for
    //StartStageText set this value to: animationName: StartStageTextAnimation
    //CongratulationsText set this value to: animationName: CongratulationsTextAnimation
    //CongratulationBackgroundDarkeningEffect set this value to: animationName: CongratulationBackgroundDarkeningEffect


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

    private void PlayAnimation()
    {
        myAnimator.Play(animationName);
    }

}
