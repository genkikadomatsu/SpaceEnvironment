using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollow : MonoBehaviour
{
    public Transform player; 

    void Start()
    {
        transform.position = player.position + Vector3.down * 2;
        transform.rotation = player.rotation; 
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + Vector3.down * 2;
        transform.rotation = player.rotation;
    }
}
