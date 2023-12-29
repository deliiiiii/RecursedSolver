using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public static Solver instance { get; private set; }

    public Box prefab_box;

    public Item item_load = null;
    public Box box_curIn;
    public List<Box> boxs_initial = new();
    public List<Box> boxs_existing = new();
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        boxs_initial.Add(Instantiate(prefab_box));
        boxs_initial[0].AddItem("Stone");
        boxs_initial[0].AddItem("Stone");
        boxs_initial[0].AddItem("Stone");
        Box.Conversion conversion = new()
        {
            item_costs = new()
            {
                ItemManager.instance.prefab_items[2],
                ItemManager.instance.prefab_items[2],
                ItemManager.instance.prefab_items[2],
            },
            item_rewards = new()
            {
                ItemManager.instance.prefab_items[1],
            },
        };
        boxs_initial[0].AddConversion(conversion);
        boxs_initial[0].RefreshExistingItems();

        box_curIn = boxs_initial[0];
    }
    bool canStepNext = true;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            StartCoroutine(nameof(Solve));
        if (Input.GetKeyDown(KeyCode.A))
        {
            canStepNext = true;
        }
    }
    int depth = 0;
    
    IEnumerator Solve()
    {
        canStepNext = false;
        depth++;
        int localDepth = depth;
        //Debug.Log("Deeper ! " + depth);
        for (int i = 0;i< boxs_initial[0].movementFuncs.Count;i++)
            if(boxs_initial[0].movementFuncs[i]() && depth <= 3)
            {
                Debug.Log("Move : " + localDepth + "-" + i);
                while (!canStepNext || localDepth != depth)
                {
                    //Debug.Log("localDepth WAIT ! " + localDepth);
                    yield return new WaitForSeconds(0.1f);
                }
                StartCoroutine(nameof(Solve));
                while(localDepth != depth)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        depth--;
        yield break;
    }
}
