using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public GameObject g1;
    public GameObject g2;
    public RecurseTest g3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            g2 = Instantiate(g1,transform);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            Debug.Log(g2.GetComponent<RecurseTest>().ii[0, 0]);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            g2.transform.GetComponent<RecurseTest>().dele[0]();
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
            g3 = Instantiate(g1.transform.GetComponent<RecurseTest>(), transform);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            g3.transform.GetComponent<RecurseTest>().dele[1]();

        if (Input.GetKeyDown(KeyCode.Alpha5))
            Debug.Log(g1.transform.GetComponent<RecurseTest>().ii[0, 0]);
        
    }
}
