using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using System.Reflection;
using System;

public class Solver : MonoBehaviour
{
    public static Solver instance { get; private set; }

    public float waitInterval = 0.1f;
    public static int id_usedTotal = 0;
    public ItemInfo item_load = new();
    public ItemInfo box_curIn;
    public ItemInfo box_curInCopied;
    public ItemInfo box_root;
    public ItemInfo box_rootCopied;
    public ItemInfo box_parent;

    private void Awake()
    {
        instance = this;
        item_load.id_in_library = -1;
    }
    private void Start()
    {
        SetBox();
        RefreshBox(box_curIn);
    }
    void SetBox()
    {
        box_curIn = new();
        box_curIn.AddInitItem("Box");
        ItemInfo boxInside = box_curIn.items_initial[^1];
        boxInside.AddInitItem("Key");
        ItemInfo.Conversion conversion = new()
        {
            item_costs = new()
            {
                new(){ name = "Key" },
            },
            item_rewards = new()
            {   
                new(){ name = "Target" },
            },
        };
        box_root = box_curIn;
        boxInside.AddConversion(conversion);

    }
    public void RefreshBox(ItemInfo inBox)
    {
        inBox.RefreshItems();
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            box_root = Clone.DeepCopy1(box_curIn);
            //box_curIn.items_initial.Clear();
        }
    }
    int depth = 0;
    private IEnumerator Solve()
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
        //Box localBox = box_curIn.CloneViaFakeSerialization();
        
        int i = -1;
        foreach(var funcName in box_curIn.movementsDict.Keys)
        //for (int i=0;i< box_curIn.movementsDict.Keys.Count;i++)
        {
            i++;
            Debug.Log("move(Depth " + localDepth + " ) " + funcName);
            #region WriteState
            
            //boxs_existing[0].t_curBox = box_curIn;
            //boxs_existing[0].t_item_load = item_load;
            //GameObject t = Instantiate(boxs_existing[0].gameObject, P_Box_Existing);
            //t.name = "beforeMove " + localDepth+ " - " + i.ToString();
            //Debug.Log("init count = " + boxs_existing[0].movementFuncs.Count);
            //Debug.Log("t.count = " + t.GetComponent<Box>().movementFuncs.Count);
            //if (t.GetComponent<Box>().movementFuncs.Count != 0)
            //    Debug.Log(t.GetComponent<Box>().movementFuncs[1]());
            box_rootCopied = Clone.DeepCopy1(box_root);
            box_curInCopied = Clone.DeepCopy1(box_curIn);
            #endregion
            if (box_curIn.ExecuteMove(funcName, box_curIn.movementsDict[funcName]))
            {
                while (!canStepNext || localDepth != depth)
                {
                    Debug.Log("WAIT depth" + localDepth + "-(" + i + ")" + funcName + "-1");
                    yield return new WaitForSeconds(waitInterval);
                }
                canStepNext = false;
                StartCoroutine(nameof(Solve));
                while (!canStepNext || localDepth != depth)
                {
                    Debug.Log("WAIT depth" + localDepth + "-(" + i + ")" + funcName+"-2");
                    yield return new WaitForSeconds(waitInterval);
                }
                canStepNext = false;
            }
            #region ReadState
            Debug.Log("revertMove(Depth " + localDepth + " ) " + funcName);
            box_root = box_rootCopied;
            box_curIn = box_curInCopied;
            //box_curIn = t.transform.GetChild(0).GetComponent<Box>().t_curBox;
            //item_load = t.transform.GetChild(0).GetComponent<Box>().t_item_load;
            //DestroyImmediate(P_Box_Existing.gameObject);
            //P_Box_Existing = t.transform;
            //Debug.Log("Change P to " + P_Box_Existing.name);
            while (true)
            {
                if (canStepNext)
                    break;
                yield return new WaitForSeconds(waitInterval);
            }
            canStepNext = false;
            #endregion
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
