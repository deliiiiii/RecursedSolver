using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public GameObject g1;
    public GameObject g2;
    // Start is called before the first frame update
    void Start()
    {
        g1.GetComponent<RecurseTest>().c.i1 = 456;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(Instantiate(g1, transform).GetComponent<RecurseTest.C1>().i1);
        }
    }
}
