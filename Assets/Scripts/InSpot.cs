using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class InSpot : MonoBehaviour
{
    public bool taken = false;
    Collider coll;
    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Pixel")
        {
            if(coll.bounds.Contains(other.bounds.center)) // && coll.bounds.Contains(other.bounds.min))
            {
                taken = true;
            }
            else
            {
                taken = false;
            }
        }
    }
}
