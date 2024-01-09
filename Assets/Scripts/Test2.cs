using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class Test2 : MonoBehaviour
{
    public ObservableValue<Dictionary<string, int>, Test2> o_dic;
    public ObservableValue<List<int>, Test2> o_list;
    [Serializable]
    public class Test3
    {
        public int i = 1115;
    }
    public Test3 g0;
    public Test3 g1;
    public Test3 g2;

    private void Awake()
    {
        o_dic = new(new(), this);
        o_list = new(new(), this);

        g1 = g0;
        g2 = g0.CloneViaFakeSerialization();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            o_dic.Value.Add("1", 111);
            g0.i = 2225;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
            o_list.Value.Add(111);
    }

    public void DictChanged()
    {
        Debug.Log("ddd");
    }
}
