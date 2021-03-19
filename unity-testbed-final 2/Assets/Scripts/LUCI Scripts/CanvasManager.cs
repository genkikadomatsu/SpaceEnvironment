/* Chat Manager 
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles chat functionality and data logging.
 *              
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          PUN_Manager.cs
 *       
 *       Find in unity inspector Scene -> Managers
 *          - To set which pads are recorded 
 *       Find in unity inspector Scene -> pad (multiple) 
 *          - To change the location and correct object of a pad
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{

    public GameObject canvas;
    private bool shoppingCart = false;
    public InputField[] inputFields;
    public Button [] button;


    /* 
     * Called on first frame load
     *  - Hide the notes canvas
     */
    void Start()
    {
        canvas.SetActive(false);
    }


    /* 
     * Called on each frame 
     *  - If R key is pressed, and the user is not typing in the chatbox, switch canvas
     */
    void Update()
    {
        for(int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i].isFocused) return; 
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Switch();

        }
    }


    /* 
     * Called when switching between notes/main canvas
     *  - Show the notes screen canvas or hide depending on current state
     */
    public void Switch()
    {
        if (shoppingCart)
        {
            canvas.SetActive(false);
            shoppingCart = false;
        }
        else
        {
            canvas.SetActive(true);
            shoppingCart = true;
        }
    }
}
