using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreIncreaseEffectText : MonoBehaviour
{

    private bool isFlashing= false;
    private Color newColor;
    private TextMesh text;
    private float flashingItemTime = 0.04F;
    [SerializeField] private float speed = 0.25F;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 tempVec = transform.position;//set the z position to place this object over all elements
        tempVec.z = -1;
        transform.position = tempVec;

        newColor = new Color32(0xE3, 0X94, 0x00, 0xFF);//E39400
        
        text = GetComponent<TextMesh>(); 
        DestroyMySelf();
    }

    private void DestroyMySelf()
    {
        Destroy(gameObject, 2.1F);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up*Time.deltaTime*speed);
        StartCoroutine(FlashingItem());
    }

    private IEnumerator FlashingItem()
    {
        if (!isFlashing)
        {
            isFlashing = true;//We mark that the coroutine is running
            SetColor(ref newColor);
            //print("SET");
            yield return new WaitForSeconds(flashingItemTime);
            SetColor(ref newColor);
            //print("RESET");
            isFlashing = false;//We mark that the coroutine has finished
        }
    }

    private void SetColor(ref Color newColor)
    {
        //print("Switched color!");
        Color tempColor = text.color;
        text.color = newColor;
        newColor = tempColor;
    }

}
