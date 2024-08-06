using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PressAnyKeyText : MonoBehaviour
{
    private Animator myAnimator;
    private float animationDelayTime = 1.4F;
    private Renderer myRenderer;


    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<Renderer>();

        Invoke("PlayAnimationWithDelay",animationDelayTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0))
        {
            float loadSceneDelayTime = 0.6F;
            Invoke("LoadSceneWithDelay", loadSceneDelayTime);
        }
    }

    private void PlayAnimationWithDelay()
    {
        myRenderer.enabled = true;
        myAnimator.Play("PressAnyKeyTextAnimation");
    }

    private void LoadSceneWithDelay()
    {
        SceneManager.LoadScene("Stage1");
    }

}
