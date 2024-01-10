using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

[Serializable]
public class ItemInfo
{
    public ItemInfo()
    {
        FakeAwake();
    }
    
    public string name;

    public bool isBox;
    //public bool canReach;
    public bool canMove;

    public int id_in_library;
    public int id_total;

    //public List<Vector2Int> checkPoints;
    [SerializeReference]
    public List<ItemInfo> items_initial;
    [SerializeReference]
    public List<ItemInfo> items_exsiting;
    public List<Conversion> conversions_initial;
    [Serializable]
    public class Conversion
    {
        [SerializeReference]
        public List<ItemInfo> item_costs = new();
        [SerializeReference]
        public List<ItemInfo> item_rewards = new();
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
    public SerializableDictionary<string, int> movementsDict;
    /*
    移动到checkPoint
    //用石头垫脚
    */
    public void FakeAwake()
    {
        items_initial = new();
        items_exsiting = new();
        conversions_initial = new();
        movementsDict = new();
        id_in_library = -1;
    }
    public bool ExecuteMove(string funcName,int idPara)
    {
        if(funcName.Contains(nameof(LoadItem)))
        {
            return LoadItem(idPara);
        }
        if(funcName.Contains(nameof(UnloadItem)))
        {
            if (Solver.instance.ans[Solver.depth - 1].Contains(nameof(LoadItem)))
                return false;
            return UnloadItem();
        }
        if (funcName.Contains(nameof(EnterBox)))
        {
            return EnterBox(idPara);
        }
        if (funcName.Contains(nameof(ExitBox)))
        {
            if (Solver.instance.ans[Solver.depth - 1].Contains(nameof(EnterBox)))
                return false;
            return ExitBox();
        }
        if (funcName.Contains(nameof(ConvertItem)))
        {
            return ConvertItem(idPara);
        }
        return false;
    }
    bool MoveTo(Vector2Int target)
    {
        return true;
    }
    bool LoadItem(int id)
    {
        if (Solver.instance.item_load[Solver.depth].id_in_library != -1)
            return false;
        movementsDict.Remove(nameof(LoadItem)+id.ToString());
        if(!movementsDict.Keys.Contains(nameof(UnloadItem)))
            movementsDict.Add(nameof(UnloadItem), -1);
        Solver.instance.item_load[Solver.depth] = Clone.DeepCopy1(items_exsiting.Find(it => it.id_total == id));
        if (Solver.instance.item_load[Solver.depth].isBox)
            movementsDict.Remove(nameof(EnterBox) + id.ToString());
        Debug.Log("LoadItem :" + id);
        return true;
    }
    bool UnloadItem()
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        if (load.id_in_library == -1)
            return false;
        int id = load.id_total;
        if (load.isBox)
            movementsDict.Add(nameof(EnterBox) + id.ToString(),id);
        movementsDict.Remove(nameof(UnloadItem));
        movementsDict.Add(nameof(LoadItem) + id.ToString(), id);
        Solver.instance.item_load[Solver.depth] = new() { id_in_library = -1 };
        Debug.Log("UnloadItem :" + id);
        return true;
    }
    bool EnterBox(int id)
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        ItemInfo newBox = Clone.DeepCopy1(items_exsiting.Find(it=>it.id_total == id));
        movementsDict.Remove(nameof(EnterBox)+id.ToString());
        newBox.RefreshBox();
        foreach (var it in newBox.movementsDict.Keys)
            Debug.Log(it);
        newBox.movementsDict.Add(nameof(ExitBox), -1);

        if (load.id_in_library != -1)
        {
            newBox.AddExistingItem(load);
            RemoveExistingItem(load);
        }
        Solver.instance.box_parent[Solver.depth] = Clone.DeepCopy1(this);
        Solver.instance.box_curIn[Solver.depth] = newBox;
        Debug.Log("EnterBox :" + newBox.id_total);
        return true;
    }
    bool ExitBox()
    {
        ItemInfo load = Solver.instance.item_load[Solver.depth];
        if (load.id_in_library != -1)
        {
            Solver.instance.box_parent[Solver.depth].AddExistingItem(load);
            //TODO -1 RemoveExistingItem(load);
        }
        Solver.instance.box_curIn[Solver.depth] = Clone.DeepCopy1(Solver.instance.box_parent[Solver.depth]);
        //TODO -1 movementsDict.Value.Remove(nameof(ExitBox));
        //TODO -1 refresh existing item
        Solver.instance.box_parent[Solver.depth] = new() { id_in_library = -1 };
        Debug.Log("ExitBox");
        return true;
    }
    bool StepOnStone()
    {
        return true;
    }
    bool ConvertItem(int index)
    {
        List<ItemInfo> ret = CanSatisfyConversion(index);
        if (ret == null)
            return false;
        movementsDict.Remove(nameof(ConvertItem)+index.ToString());
        foreach (var it in conversions_initial[index].item_costs)
            RemoveExistingItem(it);
        foreach (var it in conversions_initial[index].item_rewards)
            AddExistingItem(it);
        Debug.Log("Convert id:" + index);
        if (conversions_initial[index].item_rewards[0].name.Contains("Target"))
        {
            Debug.LogError("------------- Solution " + Solver.id_ans++);
            for (int i = 1; i <= Solver.depth; i++)
                Debug.LogError(Solver.instance.ans[i]);
        }
        return true;
    }
    List<ItemInfo> CanSatisfyConversion(int index)
    {
        List<ItemInfo> ret = new();
        foreach(var cost in conversions_initial[index].item_costs)
        {
            ItemInfo t = items_exsiting.Find(it => 
                                            it.name == cost.name
                                            //&&
                                            //it.canReach
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
    public void RefreshBox()
    {
        movementsDict.Clear();
        RefreshItems();
        RefreshConversions();
    }
    public static ItemInfo GenerateItem(string f_name)
    {
        ItemInfo item = new()
        {
            name = f_name,
        };
        switch (item.name)
        {
            case "Box":
                item.id_in_library = 0;
                item.isBox = true;
                item.canMove = true;
                break;
            case "Key":
                item.id_in_library = 1;
                item.canMove = true;
                break;
            case "Stone":
                item.id_in_library = 2;
                item.canMove = true;
                break;
            case "Target":
                item.id_in_library = 3;
                item.canMove = true;
                break;
            default:
                break;
        }
        return item;
    }
    public void AddInitItem(string f_name)//初始添加
    {
        items_initial.Add(GenerateItem(f_name));
    }
    public void AddExistingItem(ItemInfo item)
    {
        if (Solver.instance.item_load[Solver.depth] == item)
        {
            items_exsiting.Add(item);
            movementsDict.Add(nameof(UnloadItem), -1);
            return;
        }
        ItemInfo newItem = Clone.DeepCopy1(item);
        items_exsiting.Add(newItem);
        
        newItem.id_total = Solver.id_usedTotal;
        Solver.id_usedTotal++;
        if (!newItem.canMove)
            return;
        movementsDict.Add(nameof(LoadItem) + newItem.id_total.ToString(), newItem.id_total);
        if (!newItem.isBox)
            return;
        movementsDict.Add(nameof(EnterBox) + newItem.id_total.ToString(), newItem.id_total);
        //TODO newItem.RefreshItems();
    }
    public void RemoveExistingItem(ItemInfo item)
    {
        if (Solver.instance.item_load[Solver.depth] == item)
        {
            Solver.instance.item_load[Solver.depth].id_in_library = -1;
            movementsDict.Remove(nameof(UnloadItem));
        }
        int id = item.id_total;
        if (!item.canMove)
            return;
        movementsDict.Remove("Fuck");
        movementsDict.Remove(nameof(LoadItem) + id.ToString());
        if (!item.isBox)
            return;
        movementsDict.Remove(nameof(EnterBox) + id.ToString());
        items_exsiting.Remove(item);
    }
    public void RefreshItems()
    {
        //int c = 0;
        //while(items_exsiting.Count != 0)
        //{
        //    c++;
        //    Debug.Log("c = " + c); 
        //    RemoveExistingItem(items_exsiting[0]);
        //    if (c > 10)
        //        break;
        //}
        items_exsiting.Clear();
        foreach (var it in items_initial)
            AddExistingItem(it);
    }
    
    public void AddInitConversion(Conversion conversion)
    {
        conversions_initial.Add(conversion);
    }
    public void AddExistingConversion(Conversion con,int id)
    {
        movementsDict.Add(nameof(ConvertItem) + id.ToString(),id);
    }
    public void RefreshConversions()
    {
        int i = -1;
        foreach (var it in conversions_initial)
        {
            i++;
            AddExistingConversion(it,i);
        }
    }
    //public void AddTrigger()
    //{

    //}
    //int GetIndexInList<T>(List<T> list,T elem)
    //{
    //    for (int i = 0; i < list.Count; i++)
    //        if (list[i].Equals(elem))
    //            return i;
    //    return -1;
    //}
}
