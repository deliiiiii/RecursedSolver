using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public static Solver instance { get; private set; }

    public Transform P_Box_Initial;
    public Transform P_Box_Existing;
    public Box prefab_box;

    public Item item_load = null;
    public Box box_curIn;
    public List<Box> boxs_initial = new();
    public List<Box> boxs_existing = new();
    public Box out_localBox;
    public Box out_curBox;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        SetBox();
        RefreshBoxs();
    }
    void SetBox()
    {
        boxs_initial = new()
        {
            Instantiate(prefab_box,P_Box_Initial),
        };
        //boxs_initial[0].AddItem("Stone");
        //boxs_initial[0].AddItem("Stone");
        //boxs_initial[0].AddItem("Stone");
        //Box.Conversion conversion = new()
        //{
        //    item_costs = new()
        //    {
        //        ItemManager.instance.GetItemByName("Stone"),
        //        ItemManager.instance.GetItemByName("Stone"),
        //        ItemManager.instance.GetItemByName("Stone"),
        //    },
        //    item_rewards = new()
        //    {
        //        ItemManager.instance.GetItemByName("Target"),
        //    },
        //};
        //boxs_initial[0].AddConversion(conversion);
        boxs_initial[0].AddInitItem_byName("Box");
        Box.Conversion conversion = new()
        {
            item_costs = new()
            {
            },
            item_rewards = new()
            {
            },
        };
        //boxs_initial[0].AddConversion(conversion);

        Box boxInside = boxs_initial[0].items_initial[^1].GetComponent<Box>();
        boxInside.AddInitItem_byName("Key");
        conversion = new()
        {
            item_costs = new()
            {
                ItemManager.instance.GetItemByName("Key"),
            },
            item_rewards = new()
            {
                ItemManager.instance.GetItemByName("Target"),
            },
        };
        boxInside.AddConversion(conversion);
    }
    void RefreshBoxs()
    {
        ClearChild(P_Box_Existing);
        boxs_existing.Clear();
        boxs_existing.Add(Instantiate(boxs_initial[0], P_Box_Existing));
        boxs_existing[0].RefreshItems();
        box_curIn = boxs_existing[0];
    }

    bool canStepNext = true;
    bool isSolving = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !isSolving)
        {
            isSolving = true;
            StartCoroutine(nameof(Solve));
        }
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
        if (depth > 3)
        {
            depth--;
            if (depth == 0)
                isSolving = false;
            yield break;
        }
        int localDepth = depth;
        Debug.Log("Deeper ! " + depth);
        //Box localBox = box_curIn;
        //Box localBox = Instantiate(box_curIn);
        Box localBox = box_curIn.CloneViaFakeSerialization();

        for (int i=0;i< localBox.movementFuncs.Count;i++)
        {
            Debug.Log("Depth " + localDepth + "try move " + i);

            #region WriteState
            //boxs_existing[0].t_localBox = localBox;
            //boxs_existing[0].t_item_load = item_load;
            //GameObject t = Instantiate(P_Box_Existing.gameObject);
            //Debug.Log(P_Box_Existing.transform.GetChild(0).GetComponent<Box>().movementFuncs.Count);
            //Debug.Log(t.transform.GetChild(0).GetComponent<Box>().movementFuncs.Count);
            //Debug.Log("d");
            #endregion
            Box t_box_curIn = box_curIn.CloneViaFakeSerialization();
            Box t_localBox = localBox.CloneViaFakeSerialization();
            Item t_item_load = item_load.CloneViaFakeSerialization();
            if (localBox.movementFuncs[i]())
            {
                while (!canStepNext || localDepth != depth)
                {
                    //Debug.Log("WAIT 1 - localDepth = " + localDepth);
                    yield return new WaitForSeconds(0.1f);
                }
                canStepNext = false;
                
                StartCoroutine(nameof(Solve));
                while (!canStepNext || localDepth != depth)
                {
                    //Debug.Log("WAIT 2 - localDepth = " + localDepth);
                    yield return new WaitForSeconds(0.1f);
                }
                canStepNext = false;

                box_curIn = t_box_curIn;
                localBox = t_localBox;
                item_load = t_item_load;
                //#region ReadState
                Debug.Log("Depth " + localDepth + " revert " + i);
                ////Destroy(P_Box_Existing.gameObject);
                //P_Box_Existing = t.transform;
                //localBox = t.transform.GetChild(0).GetComponent<Box>().t_localBox;
                //Debug.Log("localBox.moveCount = " + localBox.movementFuncs.Count);
                //Debug.Log("localBox.moveFunc[0] = " + localBox.movementFuncs[0]);

                //Debug.Log("boxs_existing[0].t_localBox.moveCount = " + boxs_existing[0].t_localBox.movementFuncs.Count);

                //item_load = t.transform.GetChild(0).GetComponent<Box>().t_item_load;
                ////Debug.Log(t.transform.GetChild(0).GetComponent<Box>().t_item_load.name);
                //#endregion
            }
        }
        depth--;
        if (depth == 0)
            isSolving = false;
        yield break;
    }
    

    void ClearChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            Destroy(p.GetChild(i).gameObject);
    }

}
