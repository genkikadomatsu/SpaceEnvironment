/* TransformObject Class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles object synchronization
 *              -  Us MoveTowards/RotateTowards to syncrhonize position depending on desired speeds
 *              
 * Date: November 2020
 * 
 * Notes: 1) See 
 *              PUN_Manager.cs
 *          
 *        2) This script should be activated only from the player scene, not the spectator scene
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformObject : MonoBehaviour
{

    public Vector3 toPosition;
    public Quaternion toRotation;
    public float moveSpeed = 100;
    public float rotSpeed = 100; 

    /* Called on first frame.
     *  - Set the goal synchronized position and rotation to current values
     */ 
    void Start()
    {
        toPosition = transform.position;
        toRotation = transform.rotation; 
    }

    /* Called once per frame (of fixed frame rate) 
     *  - Move/Rotate towards the desired position/rotation with desired speed
     */ 
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, toPosition, moveSpeed);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotSpeed);

    }
}
