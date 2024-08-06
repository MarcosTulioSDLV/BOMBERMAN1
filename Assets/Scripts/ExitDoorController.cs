using System.Collections.Generic;
using UnityEngine;

public class ExitDoorController : MonoBehaviour
{

    //Note: This list "destroyableBlocksWithoutItems" must be filled with blocks no used by the "ItemsController" element. They must be totally free blocks, to avoid conflicts between scripts.
    [SerializeField] private List<GameObject> destroyableBlocksWithoutItems= new List<GameObject>();
    private GameObject destroyableBlockWithExitDoor;
    private bool firstTimeDestroyableBlockWithExitDoorIsNull = true;
    private Vector3 destroyableBlockWithExitDoorPosition;
    private GameObject exitDoorPrefab;


    // Start is called before the first frame update
    void Start()
    {
        exitDoorPrefab = (GameObject )Resources.Load("Prefabs/ExitDoor",typeof(GameObject));

        int randomIndex = Random.Range(0,destroyableBlocksWithoutItems.Count);
        destroyableBlockWithExitDoor = destroyableBlocksWithoutItems[randomIndex];
        destroyableBlockWithExitDoorPosition = destroyableBlockWithExitDoor.transform.position;
    }



    // Update is called once per frame
    void Update()
    {
        CreateExitDoorOnceBlockIsDestroyed();
    }

    private void CreateExitDoorOnceBlockIsDestroyed()
    {
        if (destroyableBlockWithExitDoor==null && firstTimeDestroyableBlockWithExitDoorIsNull) 
        { 
            GameObject.Instantiate(exitDoorPrefab,destroyableBlockWithExitDoorPosition,Quaternion.identity);
            destroyableBlocksWithoutItems.Clear();
            firstTimeDestroyableBlockWithExitDoorIsNull = false;
        }
    }

}
