using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPowerup : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RandomPosition()
    {
        transform.position = new Vector3(Random.Range(-16, 20), Random.Range(-2, 10));
    }

}
