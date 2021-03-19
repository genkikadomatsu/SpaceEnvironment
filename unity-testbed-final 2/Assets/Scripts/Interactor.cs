using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Camera cam;
    public LayerMask raycastMask;

    public Ray mouseRay;

    Transform lastHit;

    // Start is called before the first frame update
    void Start()
    {
        mouseRay = new Ray();
        mouseRay.origin = new Vector3(9999, 9999, 9999);
    }

    // Update is called once per frame
    void Update()
    {
        
            RaycastHit hitInfo;
            Physics.queriesHitTriggers = true;
            mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        
            if(Physics.Raycast(mouseRay,out hitInfo,5,raycastMask))
            {                               
                if(lastHit != hitInfo.transform && lastHit != null)

                    lastHit.transform.SendMessage("UnHighlight", SendMessageOptions.DontRequireReceiver);
                    hitInfo.transform.SendMessage("Highlight", SendMessageOptions.DontRequireReceiver);
                    lastHit = hitInfo.transform;          

                if (Input.GetMouseButtonDown(0))
                {
                    hitInfo.transform.SendMessage("InteractWith", SendMessageOptions.DontRequireReceiver);
                    //print("Clicked on " + hitInfo.transform.name);
                   // Interact();

                }
            }
            else
            {
                if(lastHit != null)
                {
                    lastHit.transform.SendMessage("UnHighlight", SendMessageOptions.DontRequireReceiver);
                    lastHit = null;
                }
            }

            Physics.queriesHitTriggers = false;       
    }



  // private void Interact()
   // {

  //  }
}
