using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	public float translateValue;
	public float easeTime;
	public OTween.EaseType ease;
	public float waitTime;
	public bool isSlideUp;


	//private Vector3 StartlocalPos;
	//private Vector3 endlocalPos;

	private Vector3 StartWorldPos;
	private Vector3 endWorldPos;

    bool open = false;
    int inFront = 0;
	private void Start(){
		//StartlocalPos = transform.localPosition;
		StartWorldPos = transform.position;


		gameObject.isStatic = false;
	}
		
	public void OpenDoor(){
        if (!open)
        {
            OTween.ValueTo(gameObject, ease, 0.0f, -translateValue, easeTime, 0.0f, "StartOpen", "UpdateOpenDoor", "EndOpen");
            GetComponent<AudioSource>().Play();
            open = true;
        }

        inFront++;
	}

	private void UpdateOpenDoor(float f){
		Vector3 pos;

		if (isSlideUp) {
			pos = transform.TransformDirection (new Vector3 (0, 1, 0));
		} else {
		    pos = transform.TransformDirection (new Vector3 (1, 0, 0));
		}
			
		//Vector3 pos = transform.TransformDirection(new Vector3( 1,0,0));
		//Vector3 pos = transform.forward;// TransformDirection( new Vector3( 1,0,0));
		//Debug.Log(pos);
		//Debug.Log(pos*f);
		//transform.localPosition = StartlocalPos + pos*f;
		transform.position = StartWorldPos + pos*f*transform.lossyScale.x;
        //Debug.Log(transform.position.y);
		//print(transform.lossyScale.x);
	}

	private void UpdateCloseDoor(float f){
		Vector3 pos;

		if (isSlideUp) {
			pos = transform.TransformDirection (new Vector3 (0, -f, 0));
		} else {
		    pos = transform.TransformDirection (new Vector3 (-f, 0, 0));
		}
		//Vector3 pos = transform.forward;
		//transform.localPosition = endlocalPos - pos;
		transform.position = endWorldPos - pos*transform.lossyScale.x;
	}

	private void EndOpen(){
		//endlocalPos = transform.localPosition ;
		endWorldPos = transform.position;
		StartCoroutine( WaitToClose());
	}
		
	private IEnumerator WaitToClose()
    {

        yield return new WaitForSeconds(waitTime);

        while (inFront > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        OTween.ValueTo(gameObject, ease, 0.0f, translateValue, easeTime, 0.0f, "StartClose", "UpdateCloseDoor", "EndClose");
        GetComponent<AudioSource>().Play();

        open = false;

    }

    public void NotInFrontofDoor()
    {
        inFront -= 1;
    }
		
}
