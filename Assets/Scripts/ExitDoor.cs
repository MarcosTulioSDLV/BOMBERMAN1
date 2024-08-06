using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{

    [SerializeField] private bool isEnabled = false;

    private bool isFlashing= false;

    private Color newColor;

    //Note: Attribute used only as editor UI elements 
    private enum TypeOfFlashingColor {BLUE,GREEN};
    [SerializeField] private TypeOfFlashingColor typeOfFlashingColor;
    //--

    private SpriteRenderer mySpriteRenderer;

    private float flashingItemTime = 0.00002F;

    private GameController gameController;

    private Player player;
    private bool enableCompletedStagePlayerAnimation = false;

    private const int TOTAL_STAGES = 12;//Note: Update Setting to the number of stages! it must be a even number!

    private MovementSoundEffectsController movementSoundEffectsController;

    // Start is called before the first frame update
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string lastChars = currentScene.name.Substring(5);
        int currentStageNum = int.Parse(lastChars);
        if (currentStageNum >= 1 && currentStageNum <= (TOTAL_STAGES / 2))
        {
            newColor = new Color32(0x00, 0XAA, 0xFF, 0xFF);//00AAFF
            typeOfFlashingColor = TypeOfFlashingColor.BLUE;
        }
        else 
        {
            newColor = new Color32(0xCF, 0XFF, 0x00, 0xFF);//CEFF00 
            typeOfFlashingColor = TypeOfFlashingColor.GREEN;
        }

        mySpriteRenderer = GetComponent<SpriteRenderer>();

        GameObject tempObj1 = GameObject.Find("GameController");
        if (tempObj1 == null)
        {
            this.enabled = false;
            return;
        }
        gameController = tempObj1.GetComponent<GameController>();

        GameObject tempObj2 = GameObject.FindWithTag("Player");
        if (tempObj2 == null)
        {
            this.enabled = false;
            return;
        }
        player = tempObj2.GetComponent<Player>();

        movementSoundEffectsController = GameObject.FindObjectOfType<MovementSoundEffectsController>();
    }

    // Update is called once per frame
    void Update()
    {
        Enable();

        if(isEnabled)
            StartCoroutine(FlashingItem());

        CompletedStagePlayerAnimation();
    }

    private void Enable()
    {
        if (gameController.Enemies.Count <= 0)
        {
            isEnabled = true;
        }
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


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isEnabled)
        {
            if (collision.tag == "Player" && AreObjectsAtSamePosition(collision.transform,transform))
            {
                //Debug.Log("Completed Stage");

                movementSoundEffectsController.gameObject.SetActive(false);//Note: Necessary because the sound could be still playing at this time.//OR: movementSoundEffectsController.StopWalkingSoundInLoop();

                //PAUSE GAME ELEMENTS (Note: it was not possible to do it with timeScale because it would also stop the code to load a new scene:SceneManager.LoadScene
                player.enabled = false;
                Destroy(player.GetComponent<Collider2D>());//Note: disable the script does't stop the collider inside it, so this line is necessary to avoid the player collision keep working after disable the script. (if the collider is still working it could detect any explosion)
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;//Note:Stop the player with velocity is necessary, because only desable the script will not stop the player's inertia
                enableCompletedStagePlayerAnimation = true;

                gameController.enabled = false;
                //---

                Scene currentScene = SceneManager.GetActiveScene();
                string lastChars = currentScene.name.Substring(5);
                int currentStageNum = int.Parse(lastChars);
                if (currentStageNum < TOTAL_STAGES)
                {
                    StartCoroutine(LoadScene("Stage"+(currentStageNum + 1), 4F));
                }
                else if (currentStageNum >= TOTAL_STAGES)
                {
                    gameController.YouWin();
                }
            }
        }
    }

    private bool AreObjectsAtSamePosition(Transform objA,Transform objB)
    {
        Vector3 positionA = objA.position;
        Vector3 positionB = objB.position;

        const float MAX_DISTANCE_X = 0.16F;
        const float MAX_DISTANCE_Y = 0.16F;
        float AbsDistanceX = Mathf.Abs(positionA.x - positionB.x);
        float AbsDistanceY = Mathf.Abs(positionA.y - positionB.y);
        return (AbsDistanceX <= MAX_DISTANCE_X) && (AbsDistanceY <= MAX_DISTANCE_Y);
    }

    private IEnumerator LoadScene(string sceneName,float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(sceneName);
    }

    private void CompletedStagePlayerAnimation()
    {
        if (enableCompletedStagePlayerAnimation)
        {
            const float SCALING_SPEED = 0.45F;
            const float ROTATION_SPEED= 1300F;

            Vector3 newScaleVec = (player.transform.localScale - Vector3.one * Time.deltaTime * SCALING_SPEED);
            player.transform.localScale = (newScaleVec.x>0 && newScaleVec.y>0 && newScaleVec.z>0) ? newScaleVec: Vector3.zero ;

            player.transform.Rotate(Vector3.forward * Time.deltaTime * ROTATION_SPEED);
        }
    }

}
