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

    public int maxDepth = 4;
    public ItemInfo[] item_load;
    public ItemInfo[] item_loadCopied;
    public ItemInfo[] box_curIn;
    public ItemInfo[] box_curInCopied;
    public ItemInfo[] box_root;
    public ItemInfo[] box_rootCopied;
    public ItemInfo[] box_parent;
    public ItemInfo[] box_parentCopied;
    public string[] ans;
    public static int id_ans = 0;
    private void Awake()
    {
        instance = this;
        item_load = new ItemInfo[maxDepth];
        item_loadCopied = new ItemInfo[maxDepth];
        box_curIn = new ItemInfo[maxDepth];
        box_curInCopied = new ItemInfo[maxDepth];
        box_root = new ItemInfo[maxDepth];
        box_rootCopied = new ItemInfo[maxDepth];
        box_parent = new ItemInfo[maxDepth];
        box_parentCopied = new ItemInfo[maxDepth];

        ans = new string[maxDepth];
    }
    private void Start()
    {
        SetBox();
    }
    void SetBox()
    {
        #region level EX1
        //box_curIn[0] = new();
        //box_curIn[0].AddInitItem("Box");
        //ItemInfo boxInside = box_curIn[0].items_initial[^1];
        //boxInside.AddInitItem("Key");
        //ItemInfo.Conversion conversion = new()
        //{
        //    item_costs = new()
        //    {
        //        new(){ name = "Key" },
        //    },
        //    item_rewards = new()
        //    {   
        //        new(){ name = "Target" },
        //    },
        //};
        //boxInside.AddConversion(conversion);
        //box_root[0] = box_curIn[0];
        //box_curIn[0].RefreshItems();
        #endregion
        #region level 0-^2
        box_curIn[0] = new();
        ItemInfo.Conversion conversion = new()
        {
            item_costs = new()
            {
                ItemInfo.GenerateItem("Key"),
            },
            item_rewards = new()
            {
                ItemInfo.GenerateItem("Box"),
            },
        };
        ItemInfo.Conversion conversion2 = new()
        {
            item_costs = new()
            {

            },
            item_rewards = new()
            {
                ItemInfo.GenerateItem("Target"),
            },
        };
        conversion.item_rewards[0].AddInitConversion(conversion2);
        box_curIn[0].AddInitConversion(conversion);
        box_curIn[0].AddInitItem("Box");
        box_curIn[0].items_initial[^1].AddInitItem("Key");
        box_curIn[0].RefreshBox();
        box_root[0] = box_curIn[0];
        #endregion
    }

    bool canStepNext = true;
    bool isSolving = false;
    bool isAuto = true;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !isSolving)
        {
            isSolving = true;
            StartCoroutine(nameof(Solve));
        }
        if (Input.GetKeyDown(KeyCode.A) || isAuto)
        {
            canStepNext = true;
        }
    }
    public static int depth = 0;
    private IEnumerator Solve()
    {
        depth++;
        if (depth >= maxDepth)
        {
            depth--;
            yield break;
        }
        Debug.Log("Deeper ! " + depth);
        int localDepth = depth;
        int i = -1;
        box_parent[localDepth] = Clone.DeepCopy1(box_parent[localDepth - 1]);
        box_root[localDepth] = Clone.DeepCopy1(box_root[localDepth - 1]);
        box_curIn[localDepth] = Clone.DeepCopy1(box_curIn[localDepth - 1]);
        item_load[localDepth] = Clone.DeepCopy1(item_load[localDepth - 1]);
        foreach (var funcName in box_curIn[localDepth].movementsDict.Keys)
        {
            i++;
            #region WriteState
            Debug.Log("TryMove (Depth " + localDepth + " ) " + funcName);
            box_parentCopied[localDepth] = Clone.DeepCopy1(box_parent[localDepth]);
            box_rootCopied[localDepth] = Clone.DeepCopy1(box_root[localDepth]);
            box_curInCopied[localDepth] = Clone.DeepCopy1(box_curIn[localDepth]);
            item_loadCopied[localDepth] = Clone.DeepCopy1(item_load[localDepth]);
            ans[localDepth] = funcName;
            #endregion
            if (box_curIn[localDepth].ExecuteMove(funcName, box_curIn[localDepth].movementsDict[funcName]))
            {
                
                while (!canStepNext || localDepth != depth)
                {
                    //Debug.Log("WAIT depth" + localDepth + "-(" + i + ")" + funcName + "-1");
                    yield return new WaitForSeconds(waitInterval);
                }
                canStepNext = false;
                StartCoroutine(nameof(Solve));
                while (!canStepNext || localDepth != depth)
                {
                    //Debug.Log("WAIT depth" + localDepth + "-(" + i + ")" + funcName+"-2");
                    yield return new WaitForSeconds(waitInterval);
                }
                canStepNext = false;
            }
            #region ReadState
            Debug.Log("RevertMove(Depth " + localDepth + " ) " + funcName);
            box_parent[localDepth] = box_parentCopied[localDepth];
            box_root[localDepth] = box_rootCopied[localDepth];
            box_curIn[localDepth] = box_curInCopied[localDepth];
            item_load[localDepth] = item_loadCopied[localDepth];
            #endregion
        }
        depth--;
        if(depth == 0)
            isSolving = false;
        yield break;
    }
}
