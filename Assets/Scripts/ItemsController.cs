using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemsController : MonoBehaviour {


    [SerializeField] private GameObject[] destroyableBlocks = new GameObject[0];
    [SerializeField] private List<GameObject> itemsPrefabs; 
    
    List<GameObject> blocksWithItems=new List<GameObject>();
    private List<Vector3> positionOfBlocksWithItems=new List<Vector3>();

    private void Awake(){
    }

    // Use this for initialization
    void Start () {
        blocksWithItems= InitializeBlocksWithItens();

        //Save the positions of the elements of blocksWithItems (Note: this will be necessary because blocks will be destroyed)
        blocksWithItems.ForEach(block => positionOfBlocksWithItems.Add(block.transform.position));
    }

    private List<GameObject> InitializeBlocksWithItens()
    {
        List<GameObject> blocksWithItems = new List<GameObject>();
        int amountOfItems = itemsPrefabs.Count;
        List<int> randomIndexesToDestroyableBlocks = GetUniqueRandomNumbersStartingFrom0(destroyableBlocks.Length, amountOfItems);
        foreach (int randomIndex in randomIndexesToDestroyableBlocks)
        {
            GameObject randomDestroyableBlock = destroyableBlocks[randomIndex];
            blocksWithItems.Add(randomDestroyableBlock);
        }
        return blocksWithItems;
    }

    private List<int> GetUniqueRandomNumbersStartingFrom0(int maxNum,int amountOfNumbers)//Get a quantity (given as the second parameter) of random non-repeating numbers from 0 to an exclusive number (first parameter)
    {
        List<int> randomNumbersWithOutRepetitios = new List<int>();
        System.Random random = new System.Random();       
        for(int i = 1; i <= amountOfNumbers; i++)
        {
            int randomNum=random.Next(maxNum);
            while(randomNumbersWithOutRepetitios.Contains(randomNum))
                randomNum = random.Next(maxNum);
            
            randomNumbersWithOutRepetitios.Add(randomNum);
        }
        return randomNumbersWithOutRepetitios;
    }

    // Update is called once per frame
    void Update () 
    {
        CreateItemOnceBlockIsDestroyed();
    }

    private void CreateItemOnceBlockIsDestroyed()//Place down the item when the respective block is destroyed
    {
        int i = 0;
        while (i < blocksWithItems.Count)
        {
            if (blocksWithItems[i] == null)//If the block is destroyed
            {
                StartCoroutine(InstantiateWithDelay(itemsPrefabs[i],positionOfBlocksWithItems[i],0.31F));
                blocksWithItems.RemoveAt(i);
                itemsPrefabs.RemoveAt(i);
                positionOfBlocksWithItems.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }


    private IEnumerator InstantiateWithDelay(GameObject obj,Vector3 position,float delayTime)//Note: delay time must be greater than the explosion effect time, currently variable called: explosionEffect_Time with 0.3F. If this time is not greater, then the item could be destroyed immediately once created.
    {
        yield return new WaitForSeconds(delayTime);
        Instantiate(obj, position, Quaternion.identity);
    }



}
