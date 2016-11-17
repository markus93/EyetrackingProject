using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tobii.EyeTracking;

public class MoveObjects : MonoBehaviour {
    public GameObject gazeAwareObject;
    public GameObject gazePointTracker;
    public Camera myCamera;
    private Vector3 origPos;
    private Color orgColor;
    private Color focusColor = Color.blue;

    private int focusTime = 0;
    private int delayFrames = 0;
    public int speed = 3;

    private GazePoint lastGazePoint = GazePoint.Invalid;


    // Use this for initialization
    void Start () {
        gazeAwareObject = GameObject.FindWithTag("gazeaware");
        gazePointTracker = GameObject.FindWithTag("gazepoint");
        myCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        EyeTracking.SetCurrentUserViewPointCamera(myCamera);
        origPos = gazeAwareObject.transform.position;
        orgColor = gazeAwareObject.GetComponent<Renderer>().material.color;
        Debug.Log("Gaze aware object " + gazeAwareObject.name);
        Debug.Log("Tracking point" + gazePointTracker.name);
    }
	
	// Update is called once per frame
	void Update () {
        GameObject focusedObject = EyeTracking.GetFocusedObject();
        GazePoint gazePoint = EyeTracking.GetGazePoint();

        //TODO: don't update every frame, instead collect points in time window and normalize or smth
        //      so tracking would be more stable?
        if (gazePoint.SequentialId > lastGazePoint.SequentialId && gazePoint.IsWithinScreenBounds)
        {
            Vector3 gazePointInWorld = getPositionInWorld(gazePoint);
            updateTrackerPosition(gazePointInWorld);
            lastGazePoint = gazePoint;
            if (null == focusedObject)
            {
                fall();
                if (delayFrames > 5)
                {
                    focusTime = 0;
                    delayFrames = 0;
                    gazeAwareObject.GetComponent<Renderer>().material.color = orgColor;
                }
                else
                {
                    delayFrames++;
                }
            }
            else
            {
                gazeAwareObject.GetComponent<Renderer>().material.color = focusColor;
                focusTime++;
                Debug.Log("Kept focus for " + focusTime + " frames");
                move(gazePointInWorld);
                //levitate();
            }
        }
    }


    Vector3 getPositionInWorld(GazePoint gazePoint)
    {
        Vector3 gazeOnScreen = new Vector3(gazePoint.Screen.x, gazePoint.Screen.y, 5);
        Vector3 gazeInWorld = myCamera.ScreenToWorldPoint(gazeOnScreen);
        Debug.Log("Screen x: " + gazePoint.Screen.x + ", y:" + gazePoint.Screen.y);
        Debug.Log("World x: " + gazeInWorld.x + ", y:" + gazeInWorld.y);
        return gazeInWorld;
    }

    void updateTrackerPosition(Vector3 moveTo)
    {
        setTransformXY(gazePointTracker, moveTo.x, moveTo.y);
    }

    void setTransformXY(GameObject gameObject, float x, float y)
    {
        gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
    }

    private void fall()
    {
        if (gazeAwareObject.transform.position.y > origPos.y)
        {
            gazeAwareObject.transform.Translate(Vector3.down * Time.deltaTime * speed );
        }
    }

    private void levitate()
    {
        gazeAwareObject.transform.Translate(Vector3.up * Time.deltaTime * speed * 2);
    }

    private void move(Vector2 moveTo)
       
    {
        Transform transform = gazeAwareObject.transform;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(moveTo.x, moveTo.y, transform.position.z), Time.deltaTime * speed);
        //gazeAwareObject.transform.Translate(new Vector2(moveTo.x, moveTo.y) * Time.deltaTime * speed);
        /*if (moveTo.x < gazeAwareObject.transform.position.x)
        {
            gazeAwareObject.transform.Translate(Vector3.left * Time.deltaTime * speed );
        } else if(moveTo.x > gazeAwareObject.transform.position.x)
        {
            gazeAwareObject.transform.Translate(Vector3.right * Time.deltaTime * speed );
        }*/
    }

    //TEMP


}
