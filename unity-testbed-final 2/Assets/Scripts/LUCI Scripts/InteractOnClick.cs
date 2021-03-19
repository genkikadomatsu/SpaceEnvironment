/* InteractOnClick class
 * LUCI HuRDL Environment (Main Task) 
 * Purpose: This script handles client/player interactions with objects (clicks)
 *              
 *              
 * Date: November 2020
 * 
 * Note: See 
 *          PadManager.cs (manages blue pads) 
 *          SimPadManager.cs (manages cabinet switch panel) 
 *          BypassPadManager.cs (manages bypass pad) 
 *       
 *       Find in unity inspector Player Scene -> MainCamera
 *       
 *       Script Flow:
 *                                -> PickupObject -> DisplayInventory
 *        Get Click  -> Interact  -> Place Object -> DisplayInventory
 *                                -> Interact with button/switch/door
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InteractOnClick : MonoBehaviour
{
    //Camera whose point of view clicks are used
    public Camera cam;
    public LayerMask raycastMask;

    //Relevant to mouse click ray cast
    public Ray mouseRay;
    Transform lastHit;

    //Inventory image sprites
    public GameObject[] inventorySprites;

    //The gameobject that is in the user inventory 
    private GameObject inventoryObject = null;

    //OnClick animators 
    public Animator slideSwitchAnim;
    public Animator flipSwitchAnim;
    public Animator RedLockerAnim;
    public Animator GreyLockerAnim;
    public Animator YellowLockerAnim;
    public Animator[] CabinetAnim;
    public Animator[] CrateAnim;

    //Passcode panel for crates 
    private int[] passcode = { 999, 999, 999, 999 };
    public Text[] codeText;
    private bool flipOn = false;
    private string slideStatus = "grey1";

    //Red on screen message 
    public Text message;

    //Notes screen canvas 
    public GameObject shoppingCart;

    //Pad game objects 
    public GameObject[] SimPads;
    public GameObject BypassPad;

    //Chat box input field 
    public InputField[] inputFields;

    //Double click threshold (seconds) 
    private float dclick_threshold = 0.5f;
    private double timerdclick = 0;



    /* 
     * Called on first frame load
     *  - Create ray cast
     *  - Clear on screen message 
     */
    void Start()
    {
        mouseRay = new Ray();
        mouseRay.origin = new Vector3(9999, 9999, 9999);
        message.text = null;
    }


    /* 
     * Called on each frame 
     *  - Do not interact with environment if chatbox is in use or notes screen is in use
     *  - Update raycast and detect clicks
     *      - If click is valid, call Interact(), to process the click
     *          - Note: valid clicks in Update() are
     *               In range 
     *               Not double click
     *               On interactable object 
     */
    void Update()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i].isFocused) return;
        }

        if (shoppingCart.activeSelf)
            return;

        RaycastHit hitInfo;
        Physics.queriesHitTriggers = true;
        mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        //If a mouse click registered, is in range, and is not a double click, process the click
        if (Physics.Raycast(mouseRay, out hitInfo, 100, raycastMask))
        {
            if (lastHit != hitInfo.transform && lastHit != null)
                lastHit.transform.SendMessage("UnHighlight", SendMessageOptions.DontRequireReceiver);

            hitInfo.transform.SendMessage("Highlight", SendMessageOptions.DontRequireReceiver);
            lastHit = hitInfo.transform;

            if (Input.GetMouseButtonDown(0))
            {
                if ((Time.time - timerdclick) > dclick_threshold)
                {
                    if (Physics.Raycast(mouseRay, out hitInfo, 5, raycastMask))
                    {
                        hitInfo.transform.SendMessage("InteractWith", SendMessageOptions.DontRequireReceiver);
                        Interact(hitInfo);
                    }
                    else
                    {
                        StartCoroutine(ShowMessage("Too far to interact with object.", 2));
                    }
                }
                timerdclick = Time.time;
            }
        }
        else
        {
            if (lastHit != null)
            {
                lastHit.transform.SendMessage("UnHighlight", SendMessageOptions.DontRequireReceiver);
                lastHit = null;
            }
        }
        Physics.queriesHitTriggers = false;
        
    }


    /* 
     * Called to display red on screen message
     *  - If click is out of range
     *  - If currently holding an object, cannot pick up another
     *  - Display message for delay seconds
     */
    IEnumerator ShowMessage(string annoucement, float delay)
    {
        message.text = annoucement;
        message.enabled = true;
        yield return new WaitForSeconds(delay);
        message.enabled = false;
    }



    /* 
     * Called on valid Update() click
     *  - Depending on tag/name of clicked object, call correct function
     *      - Possibilities are
     *          Picking up object 
     *          Placing object 
     *          Flipping switch 
     *          Entering pascode
     *          Opening cabinet door
     *          
     *          Note: invalid clicks in Interact(): 
     *              Currently holding object and trying to place
     *              Currently holding nothing and trying to place
     */
    private void Interact(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.tag == "BigPickupable" || hitInfo.collider.gameObject.tag == "SmallPickupable")
        {
            if (inventoryObject == null)
            {
                PickupObject(hitInfo);
                return;
            }
            else
            {
                StartCoroutine(ShowMessage("Currently holding an object.", 2));
            }
        }
        else if (hitInfo.collider.gameObject.tag == "BigEmptyPad" || hitInfo.collider.gameObject.tag == "SmallEmptyPad")
        {
            if (inventoryObject != null)
            {
                PlaceObject(hitInfo);
                return;
            }
            else
            {
                StartCoroutine(ShowMessage("Inventory is empty.", 2));

            }
        }
        else if (hitInfo.collider.gameObject.tag == "Switch")
        {
            FlipSwitch(hitInfo);
        }
        else if (hitInfo.collider.gameObject.tag == "Passcode")
        {
            PressButton(hitInfo);
        }
        if (hitInfo.collider.gameObject.tag != "IgnoreClick")
        {
            if (hitInfo.collider.gameObject.GetComponent<Animator>() != null)
            {
                if (hitInfo.collider.gameObject.name != "Pad")
                {
                    PlayAnimation(hitInfo.collider.gameObject.GetComponent<Animator>());
                    return;
                }
            }
        }
    }


    /* 
     * Called when clicking on cabinet door
     *  - Open door (by triggering OpenClose animation) 
     *  - Note: only expected use is on cabinet door
     */
    private void PlayAnimation(Animator anim)
    {
        if (anim != null) anim.SetTrigger("OpenClose");
    }


    /* 
     * Called when clicking on object and inventory is empty
     *  - If object is tagged as a pickupable, hide object from scene
     *  - Display correct inventory image sprite of object
     */
    private void PickupObject(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.tag == "BigPickupable" || hitInfo.collider.gameObject.tag == "SmallPickupable")
        {
            inventoryObject = hitInfo.collider.gameObject;
            inventoryObject.transform.position = Vector3.up * 50;
            inventoryObject.SetActive(false);
            DisplayInventory(inventoryObject.name);
        }
    }


    /* 
     * Called when picking up / placing object to update inventory image
     *  - Iterate through sprites and display sprite of matching name / empty inventory
     */
    private void DisplayInventory(string objectName)
    {
        for (int i = 0; i < inventorySprites.Length; i++)
        {
            if (inventorySprites[i].gameObject.name == objectName)
            {
                inventorySprites[i].SetActive(true);
            }
            else
            {
                inventorySprites[i].SetActive(false);
            }
        }
    }


    /* 
     * Called when placing object
     *  - Places if clicking on empty pad and holding fitting object
     *  - Depending on pad's tag, permit/prevent object placement
     *  - Call RepositionObject to reposition object on pad
     *  - Display red on screen message if object cannot be placed
     */
    private void PlaceObject(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.tag == "BigEmptyPad")
        {
            inventoryObject.SetActive(true);
            RepositionObject(hitInfo);
        }
        else if (hitInfo.collider.gameObject.tag == "SmallEmptyPad")
        {
            if (hitInfo.collider.gameObject.name == "EmptySimPad")
            {
                if (inventoryObject.name == "BlueSim" || inventoryObject.name == "GreySim" || inventoryObject.name == "RedSim" || inventoryObject.name == "BrownSim")
                {
                    inventoryObject.SetActive(true);
                    RepositionObject(hitInfo);
                }
                else
                {
                    Debug.Log("Object is too large to fit on this pad.");
                    StartCoroutine(ShowMessage("Object too large to place.", 2));

                }
            }
            else if (hitInfo.collider.gameObject.name == "EmptyBypassPad")
            {
                if (inventoryObject.name == "Bypass")
                {
                    inventoryObject.SetActive(true);
                    RepositionObject(hitInfo);
                }
                else
                {
                    StartCoroutine(ShowMessage("Object does not fit there.", 2));
                }
            }
            else if (inventoryObject.tag == "SmallPickupable")
            {
                inventoryObject.SetActive(true);
                RepositionObject(hitInfo);
            }
            else
            {
                StartCoroutine(ShowMessage("Object too large to place.", 2));
            }
        }
    }


    /* 
     * Called placing object to reposition object over pad
     *  - Depending on GameObject's name/tag and pad name/tag
     *      - Reposition object over pad
     *  - Call DisplayInventory to show empty inventory
     *  - TODO: Refactor this function
     */
    private void RepositionObject(RaycastHit hitInfo)
    {
        Vector3 objectPosition = hitInfo.collider.gameObject.transform.position;
        Quaternion objectRotation = hitInfo.collider.gameObject.transform.rotation;

        if (inventoryObject.name == "GalvanicSynthesizer")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.6f;
            inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(0, 89.5f, 0);
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "ModularSynthesizer")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.37f;
            inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(0, 89.5f, 0);
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "OptimizedSynthesizer")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.5f;
            inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(0, 89.5f, 0);
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "SonicOptimizer" || inventoryObject.name == "AdvancedOptimizer" || inventoryObject.name == "QuantumOptimizer")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.75f;
            inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(0, 180f, 0);
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "MegabandModule" || inventoryObject.name == "HyperbandModule")
        {

            inventoryObject.transform.position = objectPosition + Vector3.up * 0.6f;
            inventoryObject.transform.rotation = objectRotation;
            inventoryObject = null;
            DisplayInventory("x");

        }
        else if (inventoryObject.name == "UltrabandModule")
        {

            inventoryObject.transform.position = objectPosition + Vector3.up * 0.75f;
            inventoryObject.transform.rotation = objectRotation;
            inventoryObject = null;
            DisplayInventory("x");

        }
        else if (inventoryObject.name == "ElectroCapacitor" || inventoryObject.name == "AdaptiveCapacitor")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.4f;
            inventoryObject.transform.rotation = objectRotation;
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "TeslaCapacitor" || inventoryObject.name == "redObject")
        {
            if (hitInfo.collider.gameObject.name == "HPad")
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.4f;
                inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(0, 90f, 90f); ;
            }
            else if (hitInfo.collider.gameObject.name == "DPad")
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.4f;
                inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(90f, 0f, 0f); ;
            }
            else
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.5f;
                inventoryObject.transform.rotation = objectRotation;
            }
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "MechanoCalibrator" || inventoryObject.name == "CryoCalibrator" || inventoryObject.name == "DeltaCalibrator")
        {
            if (hitInfo.collider.gameObject.name == "VPad")
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.5f;
                inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(60f, 90f, 0f);
            }
            else
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.2f;
                inventoryObject.transform.rotation = objectRotation;
            }

            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "TemporalEmitter")
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.2f;
            inventoryObject.transform.rotation = objectRotation;
            inventoryObject = null;
            DisplayInventory("x");
        }
        else if (inventoryObject.name == "BlueSim" || inventoryObject.name == "GreySim" || inventoryObject.name == "RedSim" || inventoryObject.name == "BrownSim")
        {
            if (hitInfo.collider.gameObject.name == "EmptySimPad")
            {
                inventoryObject.transform.position = objectPosition + Vector3.right * 0.05f;
                inventoryObject.transform.rotation = objectRotation;
            }
            else
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.25f;
                inventoryObject.transform.rotation = objectRotation * Quaternion.Euler(-90f, 0, 90f);
            }
            inventoryObject = null;
            DisplayInventory("x");

        }
        else if (inventoryObject.name == "Bypass")
        {
            if (hitInfo.collider.gameObject.name == "EmptyBypassPad")
            {
                inventoryObject.transform.position = objectPosition + Vector3.forward * -0.1f;
                inventoryObject.transform.rotation = objectRotation;

            }
            else
            {
                inventoryObject.transform.position = objectPosition + Vector3.up * 0.3f;
                inventoryObject.transform.rotation = objectRotation;
            }
            inventoryObject = null;
            DisplayInventory("x");
        }
        else
        {
            inventoryObject.transform.position = objectPosition + Vector3.up * 0.3f;
            inventoryObject.transform.rotation = objectRotation;
            inventoryObject = null;
            DisplayInventory("x");
        }
    }


    /* 
     * Called when interacting with locker switch panel
     *  - Shift state of switches depending on current state
     *  - If button is pressed and states are correct, open correct locker (animation) 
     */
    private void FlipSwitch(RaycastHit hitInfo)
    {
        //Slide switch
        if (hitInfo.collider.gameObject.name == "SlideSwitch")
        {
            slideSwitchAnim.SetTrigger("OpenClose");
            if (slideStatus == "grey1") slideStatus = "yellow";
            else if (slideStatus == "yellow") slideStatus = "grey2";
            else if (slideStatus == "grey2") slideStatus = "red";
            else if (slideStatus == "red") slideStatus = "grey1";
        }
        //Flip switch
        else if (hitInfo.collider.gameObject.name == "FlipSwitch")
        {
            flipSwitchAnim.SetTrigger("OpenClose");
            if (flipOn) flipOn = false;
            else if (!flipOn) flipOn = true;
        }
        //Red Button
        else if (hitInfo.collider.gameObject.name == "RedButton")
        {
            if (flipOn && BypassPad.name == "FullBypassPad" && slideStatus == "red")
                RedLockerAnim.SetTrigger("OpenClose");
        }
        //Grey Button
        else if (hitInfo.collider.gameObject.name == "GreyButton")
        {
            if (flipOn && BypassPad.name == "FullBypassPad")
            {
                if (slideStatus == "grey1" || slideStatus == "grey2")
                    GreyLockerAnim.SetTrigger("OpenClose");
            }
        }
        //Yellow Button
        else if (hitInfo.collider.gameObject.name == "YellowButton")
        {
            if (flipOn && BypassPad.name == "FullBypassPad" && slideStatus == "yellow")
                YellowLockerAnim.SetTrigger("OpenClose");
        }
        else if (hitInfo.collider.gameObject.name == "SimButton")
        {
            if (SimPads[0].name == "BlueSimPad" && SimPads[1].name == "BrownSimPad" && SimPads[2].name == "GreySimPad")
            {
                CabinetAnim[0].SetTrigger("OpenClose");
            }
            else if (SimPads[0].name == "RedSimPad" && SimPads[1].name == "BlueSimPad" && SimPads[2].name == "BrownSimPad")
            {
                CabinetAnim[1].SetTrigger("OpenClose");
            }
        }
    }


    /* 
     * Called when interacting with passcode panel
     *  - Depending on button/current code,
     *      - Add code number/display (AssignCode)
     *      - Attempt to open crate (CheckCode)
     *      - ResetCode
     *  - Display correct code
     */
    private void PressButton(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.name == "CheckButton")
        {
            CheckCode();
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (passcode[i] == 999)
            {
                AssignCode(hitInfo.collider.gameObject.name, i);
                DisplayCode();
                return;
            }
        }

        ResetCode();
        DisplayCode();

    }


    /* 
     * Called when interacting with passcode panel
     *  - Depending on button/current code,
     *      - Add code number/display (AssignCode)
     *      - Attempt to open crate (CheckCode)
     *      - ResetCode
     *  - Display correct code
     */
    private void CheckCode()
    {
        if (passcode[0] == 1 && passcode[1] == 2 && passcode[2] == 3 && passcode[3] == 4)
        {
            CrateAnim[0].SetTrigger("OpenClose");
            print("setting tigger");
            return;
        }
        else if (passcode[0] == 5 && passcode[1] == 6 && passcode[2] == 7 && passcode[3] == 8)
        {
            CrateAnim[1].SetTrigger("OpenClose");
            return;
        }
        else if (passcode[0] == 2 && passcode[1] == 5 && passcode[2] == 8 && passcode[3] == 0)
        {
            CrateAnim[2].SetTrigger("OpenClose");
            return;
        }
    }


    /* 
     * Called by PressButton(), each time passcode button is pressed
     *  - Display blank value if the value is 999
     *  - Display the correct code (change text of text object) 
     */
    private void DisplayCode()
    {
        for (int i = 0; i < 4; i++)
        {
            if (passcode[i] == 999) codeText[i].text = "-";
            else if (passcode[i] == 0) codeText[i].text = "0";
            else if (passcode[i] == 1) codeText[i].text = "1";
            else if (passcode[i] == 2) codeText[i].text = "2";
            else if (passcode[i] == 3) codeText[i].text = "3";
            else if (passcode[i] == 4) codeText[i].text = "4";
            else if (passcode[i] == 5) codeText[i].text = "5";
            else if (passcode[i] == 6) codeText[i].text = "6";
            else if (passcode[i] == 7) codeText[i].text = "7";
            else if (passcode[i] == 8) codeText[i].text = "8";
            else if (passcode[i] == 9) codeText[i].text = "9";

        }
    }



    /* 
     * Called after last passcode value is entered
     *  - Resets passcode values to 999 (blank value)
     */
    private void ResetCode()
    {
        for (int i = 0; i < 4; i++)
        {
            passcode[i] = 999;
        }
    }


    /* 
     * Called when entering passcode values
     *  - Depending on name of clicked button, add value to passcode
     */
    private void AssignCode(string name, int i)
    {
        if (name == "Button1")
        {
            passcode[i] = 1;
            return;
        }
        else if (name == "Button2")
        {
            passcode[i] = 2;
            return;
        }
        else if (name == "Button3")
        {
            passcode[i] = 3;
            return;
        }
        else if (name == "Button4")
        {
            passcode[i] = 4;
            return;
        }
        else if (name == "Button5")
        {
            passcode[i] = 5;
            return;
        }
        else if (name == "Button6")
        {
            passcode[i] = 6;
            return;
        }
        else if (name == "Button7")
        {
            passcode[i] = 7;
            return;
        }
        else if (name == "Button8")
        {
            passcode[i] = 8;
            return;
        }
        else if (name == "Button9")
        {
            passcode[i] = 9;
            return;
        }
        else if (name == "Button0")
        {
            passcode[i] = 0;
            return;
        }
    }
}

