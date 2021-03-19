using UnityEngine;
using System.Collections;

public class DoorManager : MonoBehaviour {

	public Door door1;
	public Door door2;
    public bool openForPlayer = true;
	
	void OnTriggerEnter(Collider col)
    {
        if (col.tag != "SelectTag" && (col.tag != "Player" || openForPlayer || col.tag == "Robot"))
        {
            if (door1 != null)
            {
                door1.OpenDoor();
            }

            if (door2 != null)
            {
                door2.OpenDoor();
            }
        }

	}
    void OnTriggerExit(Collider col)
    {
        if (col.tag != "SelectTag" && (col.tag != "Player" || openForPlayer || col.tag == "Robot"))
        {
            if (door1 != null)
            {
                door1.NotInFrontofDoor();
            }

            if (door2 != null)
            {
                door2.NotInFrontofDoor();
            }
        }
    }
}
