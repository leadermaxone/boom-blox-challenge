using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    private List<Block> blockList; 
    // Start is called before the first frame update

    void Awake()
    {
        Instance = this;
        blockList = new List<Block>();
    }

    public void RegisterBlock(Block block)
    {
        blockList.Add(block);
    }

    public void ResetAllBlocks()
    {
        for(int i = 0; i < blockList.Count; i++)
        {
            blockList[i].Reset();
        }
    }

}
