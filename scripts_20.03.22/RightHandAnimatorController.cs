using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandAnimatorController : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator ani;
    private int times;
    void Start()
    {
        times = 0;
    }

    // Update is called once per frame
    
    void Update()
    {

        if (times<5)
        {
            SetFinger1();
        }
        //else
        //{
            //SetFinger2();
        //}
    }
    private void SetFinger1()
    {
        ani.SetInteger("Finger", 1);
        times++;
        Debug.Log("Set Finger 1");
    }
    private void SetFinger2()
    {
        
        ani.SetInteger("Finger", 2);
    }
}
