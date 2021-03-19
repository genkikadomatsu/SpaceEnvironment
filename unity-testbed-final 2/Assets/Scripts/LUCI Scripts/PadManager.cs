/* PadManager class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles managing the visibility of the object pads
 *              - Changes pad tag depending on the object above the pad
 *              - Changes score boolean depending on the object above the pad
 *              - Current object information is determined by using raycast information
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          InteractOnClick.cs
 *          ChatManager.cs
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadManager : MonoBehaviour
{
    public GameObject pad;
    public string correctObject;
    public string currentObject; 
    public bool scoreIndicator;
    public string Location;

    /* Called on each frame
     *  - Update the state of the pad each frame
     *       - Make an upwards raycast out of the center of the pad
     *       - Set the score indicator according to the raycast information 
     *       - Set the tag ofthe pad depending on the raycast information 
     *             - Possible Tags: 
     *                      - BigFullPad 
     *                      - BigEmptyPad
     *                      - SmallFullPad
     *                      - SmallEmptyPad
     *                      - Note: Big pads can hold small or big pickupables (objects) 
     *                              Small pads can only hold small pickupables (small objects)
     *                              Pad tag is important for InteractOnClick.cs
     *                              
     * Possibly TODO: Refactor conditionals 
     */
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.up);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray,out hitInfo, 100))
        {
            if (hitInfo.collider.gameObject.name == correctObject)
                scoreIndicator = true;
            else
                scoreIndicator = false;

            if (hitInfo.collider.gameObject.tag == "SmallPickupable" || hitInfo.collider.gameObject.tag == "BigPickupable")
                currentObject = hitInfo.collider.gameObject.name;
            else
                currentObject = "No Current Object"; 

            if (hitInfo.collider.gameObject.tag == "SmallPickupable" || hitInfo.collider.gameObject.tag == "BigPickupable")
            {
                if(pad.gameObject.tag == "BigEmptyPad" || pad.gameObject.tag == "BigFullPad")
                {
                    pad.gameObject.tag = "BigFullPad";
                    pad.GetComponent<Renderer>().enabled = false;
                }
                else
                {
                    pad.gameObject.tag = "SmallFullPad";
                    pad.GetComponent<Renderer>().enabled = false;
                }
            }
            else
            {
                if(pad.gameObject.tag == "BigFullPad" || pad.gameObject.tag == "BigEmptyPad")
                {
                    pad.gameObject.tag = "BigEmptyPad";
                    pad.GetComponent<Renderer>().enabled = true;
                }
                else
                {
                    pad.gameObject.tag = "SmallEmptyPad";
                    pad.GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }
}
