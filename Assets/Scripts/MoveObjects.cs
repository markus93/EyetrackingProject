using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tobii.EyeTracking;

public class MoveObjects : MonoBehaviour {
    public GameObject explosion;
    public GameObject gazeAwareObject;
    public GameObject gazePointTracker;
    public Camera myCamera;
    private Vector3 origPos;
    private Color orgColor;
    private Color focusColor = Color.blue;

    private int focusTime = 0;
    private int delayFrames = 0;
    //public int speed = 3;

    private GazePoint lastGazePoint = GazePoint.Invalid;


    // Use this for initialization
    void Start () {
        gazeAwareObject = GameObject.FindWithTag("gazeaware");
        gazePointTracker = GameObject.FindWithTag("gazepoint");
        explosion = GameObject.FindWithTag("explosion");
        explosion.SetActive(false);
        myCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        EyeTracking.SetCurrentUserViewPointCamera(myCamera);
        origPos = gazeAwareObject.transform.position;
        orgColor = gazeAwareObject.GetComponent<Renderer>().material.color;
        Debug.Log("Gaze aware object " + gazeAwareObject.name);
        Debug.Log("Tracking point" + gazePointTracker.name);

        Invoke("DoExplosion", 2);
    }      


    void DoExplosion()
    {
        explosion.GetComponent<RectTransform>().position = new Vector3(Random.Range(-200, 200), Random.Range(-200, 200), 0);
        explosion.SetActive(true);
        Invoke("HideExplosion", 1);
        //Object newExp = Instantiate(explosion, new Vector3(Random.Range(-1, 5), Random.Range(-1,5), 0), Quaternion.identity);
        //Destroy(newExp, 5);
        int delay = Random.Range(2, 6);
        Debug.Log("Explosion after seconds " + delay);
        Invoke("DoExplosion", delay);
    }
    void HideExplosion()
    {
        explosion.SetActive(false);
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
            float weight = focusedObject.GetComponent<MovableObject>().weight;
            if (null == focusedObject)
            {
                fall(weight);
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
                move(gazePointInWorld, weight);
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

    private void fall(float speed)
    {
        if (gazeAwareObject.transform.position.y > origPos.y)
        {
            gazeAwareObject.transform.Translate(Vector3.down * Time.deltaTime * speed * 3 );
        }
    }

    private void levitate(float speed)
    {
        gazeAwareObject.transform.Translate(Vector3.up * Time.deltaTime * speed);
    }

    private void move(Vector2 moveTo, float speed)
       
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
