using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoContinueGameText : MonoBehaviour
{

    private float scaleIncreaseFactor = 0.06F;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        gameController.Score = 0;
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    private void OnMouseEnter()
    {
        gameObject.transform.localScale += Vector3.one * scaleIncreaseFactor;
    }

    private void OnMouseExit()
    {
        gameObject.transform.localScale -= Vector3.one * scaleIncreaseFactor;
    }

}
