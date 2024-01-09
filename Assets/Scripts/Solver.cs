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
    public static T DeepCopy<T>(T DeepCopyObject)
    {
        string _ = JsonConvert.SerializeObject(DeepCopyObject);
        return JsonConvert.DeserializeObject<T>(_);
    }
    public static T DeepCopy2<T>(T RealObject)
    {
        using Stream objectStream = new MemoryStream();
        //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(objectStream, RealObject);
        objectStream.Seek(0, SeekOrigin.Begin);
        return (T)formatter.Deserialize(objectStream);
    }
    
    public static Solver instance { get; private set; }

    //public Transform P_Box_Initial;
    //public Transform P_Box_Existing;
    //public Box prefab_box;
    public ItemInfo item_load = null;
    public ItemInfo box_curIn;
    public ItemInfo box_root;
    //public List<Box> boxs_initial = new();
    //public List<Box> boxs_existing = new();
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
        //boxs_initial = new()
        //{
        //    Instantiate(prefab_box,P_Box_Initial),
        //};
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
        //TODO boxInside.AddConversion(conversion);

        box_curIn.movementsDict.Add("F1", delegate () { Debug.Log("Execute F1"); return true; });
    }
    void RefreshBoxs()
    {
        box_curIn.RefreshItems();
        //ClearChild(P_Box_Existing);
        //boxs_existing.Clear();
        //boxs_existing.Add(Instantiate(boxs_initial[0], P_Box_Existing));
        //boxs_existing[0].RefreshItems();
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
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            box_root = Clone.DeepCopy2(box_curIn);
            //box_curIn.items_initial.Clear();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            box_root = Clone.DeepCopy3(box_curIn);
            //box_curIn.items_initial.Clear();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            box_root = Clone.DeepCopy4(box_curIn);
            //box_curIn.items_initial.Clear();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log(box_curIn.dict2.Count);
            Debug.Log(box_curIn.movementsDict.Count);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log(box_root.dict2.Count);
            Debug.Log(box_root.movementsDict.Count);
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
        int i = 0;
        foreach(var funcName in box_curIn.movementsDict.Keys)
        //for (int i=0;i< box_curIn.movementsDict.Keys.Count;i++)
        {
            #region WriteState
            i++;
            Debug.Log("move " + localDepth + " - " + i);
            //boxs_existing[0].t_curBox = box_curIn;
            //boxs_existing[0].t_item_load = item_load;
            //GameObject t = Instantiate(boxs_existing[0].gameObject, P_Box_Existing);
            //t.name = "beforeMove " + localDepth+ " - " + i.ToString();
            //Debug.Log("init count = " + boxs_existing[0].movementFuncs.Count);
            //Debug.Log("t.count = " + t.GetComponent<Box>().movementFuncs.Count);
            //if (t.GetComponent<Box>().movementFuncs.Count != 0)
            //    Debug.Log(t.GetComponent<Box>().movementFuncs[1]());
            while (true)
            {
                if (canStepNext)
                    break;
                yield return new WaitForSeconds(0.1f);
            }
            canStepNext = false;
            #endregion
            if (box_curIn.movementsDict[funcName]())
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
            }
            #region ReadState
            Debug.Log("revert " + localDepth + " - " + i);
            //box_curIn = t.transform.GetChild(0).GetComponent<Box>().t_curBox;
            //item_load = t.transform.GetChild(0).GetComponent<Box>().t_item_load;
            //DestroyImmediate(P_Box_Existing.gameObject);
            //P_Box_Existing = t.transform;
            //Debug.Log("Change P to " + P_Box_Existing.name);
            while (true)
            {
                if (canStepNext)
                    break;
                yield return new WaitForSeconds(0.1f);
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
