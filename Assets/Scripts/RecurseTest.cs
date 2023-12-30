using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class RecurseTest : MonoBehaviour
{
    public delegate bool TestDelegate();
    public List<TestDelegate> dele = new();
    public int[,] ii;
    private void Awake()
    {
        ii = new int[2, 2] { { 10, 20 }, { 30, 40 } };

        dele.Add(delegate() {return Func1(1); });
        dele.Add(Func2);
    }
    private void Update()
    {

    }

    bool Func1(int i)
    {
        Debug.Log("func1 i = " + i);
        return true;
    }
    bool Func2()
    {
        Debug.Log("func2");
        return true;
    }
}
