/* Player_Controller class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script controls player movement
 *              
 * Date: November 2020
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Controller : MonoBehaviour
{
    CharacterController controller;
    Vector3 motion, rotation;
    public InputField [] guiFields;
    public float movementSpeed, decayRate, rotationSpeed;
    private Vector3 startingPosition;
    private Quaternion startingRotation;


    /* Called on first frame.
     *  - Get the character controller's CharacterController
     *  - Set the starting position to the current starting position
     */ 
    void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        startingPosition = transform.position;
    }

    /* Called once each frame
     *  - If none of the GUI input fields are in use, read for movement inputs
     *  - If the position is out of range of the spacecraft, reset to start
     *      Note: Reset() is used for "Mars Glitch" 
     */
    void Update()
    {
        for(int i = 0; i < guiFields.Length; i++)
        {
            
            if (guiFields[i].isFocused)
                return;
        }

        Move();
        if (transform.position.y < -50)
            Reset();
    }


    /* Called on each frame where GUI is not in use
     *  - Depending on which key is being pressed, move the robot accordingly
     */ 
    void Move()
    {
        if (motion.magnitude > 0)
            motion *= (1 - (decayRate * Time.deltaTime));
        if (rotation.magnitude > 0)
            rotation *= (1 - (decayRate * Time.deltaTime));

        if (Input.GetKey(KeyCode.W))
            motion.z = movementSpeed;
        if (Input.GetKey(KeyCode.S))
            motion.z = -movementSpeed;

        if (Input.GetKey(KeyCode.A))
            motion.x = -movementSpeed;
        if (Input.GetKey(KeyCode.D))
            motion.x = movementSpeed;

        if (Input.GetKey(KeyCode.Q))
            rotation.y = -rotationSpeed;
        if (Input.GetKey(KeyCode.E))
            rotation.y = rotationSpeed;

        transform.localEulerAngles += rotation * Time.deltaTime;
        controller.SimpleMove(transform.forward * motion.z + transform.right * motion.x);
    }


    /* Called if the robot is out of range of possible locations to move to
     *  - Reset the robot position to the starting position
     *  - Note: This only occurs in the case of the "Mars Glitch" 
     */
    public void Reset()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;       
    }
}
