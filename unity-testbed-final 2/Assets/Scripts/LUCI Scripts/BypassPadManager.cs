/* BypaddPadManager class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles managing the visibility of the bypass pad (on the locker panel)
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          InteractOnClick.cs
 *          SimPadManager.cs
 * 
 * Note: This script functions exactly the same as SimPadManager.cs
 *       See, SimPadManager.cs for explanation of functionality
 *       
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BypassPadManager : MonoBehaviour
{
    public GameObject BypassPad;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, 100))
        {
            ManagePad(hitInfo.collider.gameObject.name);
        }
    }

    private void ManagePad(string name)
    {
        if(name == "Bypass")
        {
            BypassPad.GetComponent<Renderer>().enabled = false;
            BypassPad.gameObject.name = "FullBypassPad";
        }
        else
        {
            BypassPad.GetComponent<Renderer>().enabled = true;
            BypassPad.gameObject.name = "EmptyBypassPad"; 
        }
    }
}
