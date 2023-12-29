using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Box : Item
{
    //public int id_in_map;
    public bool canExit;
    public Box P_Box;

    public List<Vector2Int> checkPoints;
    public GameObject P_item_initial;
    public List<Item> items_initial=new();
    public GameObject P_item_existing;
    public List<Item> items_exsiting = new();

    public GameObject P_conversion;
    public GameObject prefab_conversion;
    public List<Conversion> conversions = new();
    public class Conversion
    {
        public List<Item> item_costs;
        public List<Item> item_rewards;
        public bool canRegainCost;
    }

    //#region triggerFuncs
    //public delegate void TriggerFuncs(object condition, object result);
    //public List<TriggerFuncs> triggerFuncs;
    ///*
    //移动到checkPoint -> 获取Item
    //*/
    //#endregion

    #region movementFuncs
    public delegate bool MovementFuncs();
    public List<MovementFuncs> movementFuncs = new();
    /*
    移动到checkPoint
    搬起Item(movable)
    放下Item
    进入箱子
    退出箱子
    用钥匙开锁
    用石头垫脚
    */
    bool MoveTo(Vector2Int target)
    {
        return true;
    }
    bool LoadItem(Item item)
    {
        if (Solver.instance.item_load != null)
            return false;
        Solver.instance.item_load = item;
        movementFuncs.Add(delegate () { return UnloadItem(); });
        //Debug.Log("LoadItem :" + item.name);
        return true;
        
    }
    bool UnloadItem()
    {
        if (Solver.instance.item_load == null)
            return false;
        //Debug.Log("UnloadItem :" + Solver.instance.item_load.name);
        Solver.instance.item_load = null;
        return true;
    }
    bool EnterBox(Box box)
    {
        Item load = Solver.instance.item_load;
        if (load)
        {
            box.items_exsiting.Add(load);
            items_exsiting.Remove(load);
        }
        box.P_Box = this;
        return true;
    }
    bool ExitBox(object nonSense)
    {
        return true;
    }
    bool StepOnStone(object nonSense)
    {
        return true;
    }
    bool ConvertItem(int index)
    {
        if (CanSatisfyConversion(index) == null)
            return false;
        //Debug.Log("Convert id:" + index);
        //if (conversions[index].item_rewards[0].name.Contains("Target"))
            //Debug.Log("WIN!!");
        return true;
    }
    List<Item> CanSatisfyConversion(int index)
    {
        List<Item> ret = new();
        for (int i = 0; i < conversions[index].item_costs.Count;i++)
        {
            Item t = items_exsiting.Find(it => 
                                    it.id_in_library == conversions[index].item_costs[i].id_in_library
                                    &&
                                    it.canReach
                                    &&
                                    !ret.Contains(it)
                                );
            if (t == null)
                return null;
            ret.Add(t);
        }
        return ret;
    }
    #endregion
    public void AddItem(string itemName)//初始添加
    {
        Item item = ItemManager.instance.GetItemByName(itemName);
        if (item == null)
            return;
        items_initial.Add(Instantiate(item,P_item_initial.transform));
        
    }
    public void RefreshExistingItems()
    {
        items_exsiting.Clear();
        for (int i = 0; i < items_initial.Count; i++)
        {
            items_exsiting.Add(Instantiate(items_initial[i], P_item_existing.transform));
            Item item = items_exsiting[i];
            item.id_in_box = items_exsiting.Count-1;
            if (!item.canMove)
                return;
            movementFuncs.Add(delegate() {return LoadItem(item); });
        }
       
    }
    public void AddConversion(Conversion conversion)
    {
        GameObject g = Instantiate(prefab_conversion, P_conversion.transform);
        g.name = "Conversion_" + (conversions.Count).ToString();
        for (int i = 0; i < conversion.item_costs.Count; i++)
            Instantiate(conversion.item_costs[i], P_conversion.transform.GetChild(conversions.Count).GetChild(0));
        for (int i = 0; i < conversion.item_rewards.Count; i++)
            Instantiate(conversion.item_rewards[i], P_conversion.transform.GetChild(conversions.Count).GetChild(1));
        conversions.Add(conversion);
        movementFuncs.Add(delegate () { return ConvertItem(conversions.Count-1);});
    }
    //public void AddTrigger()
    //{

    //}

}
