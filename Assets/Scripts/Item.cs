using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    private bool isFlashing;
    private Color newColor;
    private SpriteRenderer mySpriteRenderer;
    private float flashingItemTime = 0.00002F;
    private bool positiveEffectsItem = true;
    private Collider2D myCollider;
    private bool enabledNegativeEffectsItem = true;//Note:The negative effect item will be disable for a period of time once the explosion collides with it.
    private Animator myAnimator;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.tag == "MinusOneBombItem" || gameObject.tag == "SkullEffectsItem")
            positiveEffectsItem = false;

        if (positiveEffectsItem)
        {
            newColor = new Color32(0xCF, 0XFF, 0x00, 0xFF);//CFFF00
        }
        else
        {
            newColor = new Color32(0XBF, 0X94, 0X94, 0xFF);//BF9494 100% alpha (opaque) //newColor = new Color32(0XBF,0X94,0X94, 230); //90% alpha (semi-opaque)
        }
        mySpriteRenderer = GetComponent<SpriteRenderer>();

        myCollider = GetComponent<Collider2D>();

        if(!positiveEffectsItem)
            myAnimator = GetComponent<Animator>();
    }                                                                                                 

    // Update is called once per frame
    void Update()
    {
        if(enabledNegativeEffectsItem)
            StartCoroutine(FlashingItem());
    }

    private IEnumerator FlashingItem()
    {
        if (!isFlashing)
        {
            isFlashing = true;//We mark that the coroutine is running
            SetColor(ref newColor);
            yield return new WaitForSeconds(flashingItemTime);
            SetColor(ref newColor);
            isFlashing = false;//We mark that the coroutine has finished
        }
    }

    private void SetColor(ref Color newColor)
    {
        //print("Switched color!");
        Color tempColor = mySpriteRenderer.material.color;
        mySpriteRenderer.material.color = newColor;
        newColor = tempColor;
    }

    private void EnableNegativeEffectsItemWithDelay()
    {
        myAnimator.Play("EmptyState");
        myCollider.enabled = true;
        enabledNegativeEffectsItem = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Explosion")
        {
            if(!positiveEffectsItem)
            {
                //print("Disable Items!");

                //Disable Negative Effects Items for a While.
                myCollider.enabled = false;

                enabledNegativeEffectsItem = false;

                myAnimator.Play("NegativeEffectsItemAnimation");
                float delayTime = 45.1F;//Note: this time must be greater or equal than the the previous animation time.
                Invoke(nameof(EnableNegativeEffectsItemWithDelay),delayTime);
            }
            else
                Destroy(this.gameObject, 0.1F);
        }
    }



}
