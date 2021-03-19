/* SimPadManager class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles managing the visibility of the card pad (on the cabinet panel)
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          InteractOnClick.cs
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimPadManager : MonoBehaviour
{
    public GameObject SimPad;


    /* Called on each frame
     *  - Create a ray cast upwards out of the center of of the pad
     *  - If the raycast is hitting something, determine what is being hit
     */ 
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100))
        {
            ManagePad(hitInfo.collider.gameObject.name);
        }
    }


    /* Called when the pad's raycast comes in contact with an object
     *  - If the object is a SimPad, change the name of the pad, and hide
     *  - If the object is not a SimPad, name the pad EmptySimPad, and show
     *  Note: The name of the pad allows for InteractOnClick.cs functionality
     */ 
    private void ManagePad(string SimName)
    {
        if(SimName == "BlueSim" || SimName == "RedSim" || SimName == "GreySim" || SimName == "BrownSim")
        {
            SimPad.gameObject.name = SimName + "Pad";
            SimPad.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            SimPad.gameObject.name = "EmptySimPad";
            SimPad.GetComponent<Renderer>().enabled = true;
        }
    }
}



