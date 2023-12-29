using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecurseTest : MonoBehaviour
{
    public bool canStepNext = true;
    public int depth = 0;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
            canStepNext = true;
    }
    private void Start()
    {
        //CallTest();
    }
    void CallTest()
    {
        if (!canStepNext)
        {
            Invoke(nameof(CallTest), 0.05f);
            return;
        }
        Test();
    }
    public void Test()
    {
        canStepNext = false;
        depth++;
        CallTest();
    }
}
