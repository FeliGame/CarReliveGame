using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private List<int> arr = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        arr.Add(2);
        JSONRegister.Launch();
        Debug.Log(GiftManager.giftPool);
        Debug.Log(arr.Find((int target) => target == 1));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
