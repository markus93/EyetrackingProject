using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tobii.EyeTracking;
using System.Linq;

public class MoveObjects : MonoBehaviour {
    public WriteFile logger;
    //mouse support
    public bool isMouseInput = true;
    private Ray _ray;
    private RaycastHit _rayHit;
    
    public GameObject explosion;
    public GameObject gazePointTracker;
    public Camera myCamera;

    private IDataProvider<GazePoint> _gazePointProvider;
    private ITimestamped _lastHandledPoint;
    private GameObject[] gazeawareObjects;
    private Color focusColor = Color.blue;
    private GazePoint lastGazePoint = GazePoint.Invalid;

    private double focusTime;

    // Use this for initialization
    void Start () {
        _gazePointProvider = EyeTrackingHost.GetInstance().GetGazePointDataProvider();
        gazeawareObjects = GameObject.FindGameObjectsWithTag("gazeaware");
        gazePointTracker = GameObject.FindWithTag("gazepoint");
        explosion = GameObject.FindWithTag("explosion");
        //explosion.SetActive(false);
        GameObject go = GameObject.Find("Victory");
        Image img = go.GetComponent<Image>();
        img.enabled = false;

        myCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        EyeTracking.SetCurrentUserViewPointCamera(myCamera);

        foreach (GameObject obj in gazeawareObjects)
        {
            Debug.Log("Gaze aware objects " + obj.name);
        }
        Debug.Log("Tracking point" + gazePointTracker.name);

        Invoke("DoExplosion", 2);
    }      


    void DoExplosion()
    {
        int x = Random.Range(0, 500);
        int y = Random.Range(0, 500);
        logger.WriteLineToFile("EXPLOSION, " + x + "," + y);
        explosion.GetComponent<RectTransform>().position = new Vector3(x,y, 0);
        Image img = explosion.GetComponent<Image>();
        img.enabled = true;
        //explosion.SetActive(true);
        Invoke("HideExplosion", 1);
        int delay = Random.Range(1, 3);
        Debug.Log("Explosion after seconds " + delay);
        Invoke("DoExplosion", delay);
    }
    void HideExplosion()
    {
        Image img = explosion.GetComponent<Image>();
        img.enabled = false;
        //explosion.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        if (isMouseInput)
        {
            readMouseInput();
        }
        else
        {
            /*System.Collections.Generic.IEnumerable<GazePoint> pointsSinceLastHandled = _gazePointProvider.GetDataPointsSince(_lastHandledPoint);
            Vector2 sum = Vector2.zero;
            int count = 0;
            foreach (GazePoint point in pointsSinceLastHandled)
            {
                count++;
                sum += point.Screen;
                if (point == pointsSinceLastHandled.Last())
                {
                    _lastHandledPoint = point;
                }
            }
            Vector2 cent = sum / count;*/

            GazePoint gazePoint = EyeTracking.GetGazePoint();
            if (gazePoint.SequentialId > lastGazePoint.SequentialId && gazePoint.IsWithinScreenBounds)
            {
                GameObject focusedObject = EyeTracking.GetFocusedObject();
                //Vector3 gazePointInWorld = getPositionInWorld(cent);
                Vector3 gazePointInWorld = getPositionInWorld(gazePoint.Screen);
                updateTrackerPosition(gazePointInWorld);
                lastGazePoint = gazePoint;
                fall(focusedObject);                
                if(focusedObject != null)
                {
                    gazeawareFocused(gazePointInWorld, focusedObject);
                }
            }
        }
    }


    public void onObstacleHit(MovableObject mo)
    {
        logger.WriteLineToFile("HIT_OBSTACLE, " + mo.name + ", HIT_COUNT "+ mo.hitCount);
    }

    public void onFinish(MovableObject mo)
    {
        logger.WriteLineToFile("FINISH, " + mo.name);
        GameObject go = GameObject.Find("Victory");
        Image img = go.GetComponent<Image>();
        img.enabled = true;
        Invoke("HideWin", 1);
    }

    public void HideWin()
    {
        GameObject go = GameObject.Find("Victory");
        Image img = go.GetComponent<Image>();
        img.enabled = false;
    }


    private void readMouseInput()
    {       
         _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 gazePointInWorld = _ray.GetPoint(-Camera.main.transform.position.z);
        updateTrackerPosition(gazePointInWorld);
        if(!Physics.Raycast(_ray, out _rayHit))
        {
            fall(null);
            return;
        }
        GameObject focusedObject = _rayHit.transform.gameObject;
        fall(focusedObject);
        if (focusedObject.tag == "gazeaware")
        {
            gazeawareFocused(_rayHit.point, focusedObject);
        }
    }

    private void gazeawareFocused(Vector3 gazePoint, GameObject focusedObject)
    {
        focusedObject.GetComponent<Renderer>().material.color = focusColor;
        focusTime++;
        Debug.Log("Kept focus for " + focusTime + " frames");
        float weight = focusedObject.GetComponent<MovableObject>().weight;
        move(focusedObject, gazePoint, weight);
    }

    private Vector3 getPositionInWorld(Vector2 gazePoint)
    {
        Vector3 gazeOnScreen = new Vector3(gazePoint.x, gazePoint.y, 5);
        Vector3 gazeInWorld = myCamera.ScreenToWorldPoint(gazeOnScreen);
        Debug.Log("Screen x: " + gazePoint.x + ", y:" + gazePoint.y);
        Debug.Log("World x: " + gazeInWorld.x + ", y:" + gazeInWorld.y);
        return gazeInWorld;
    }

    private void updateTrackerPosition(Vector3 moveTo)
    {
        setTransformXY(gazePointTracker, moveTo.x, moveTo.y);
    }

    private void setTransformXY(GameObject gameObject, float x, float y)
    {
        gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
    }

    private void fall(GameObject focusedObject)
    {
        foreach (GameObject obj in gazeawareObjects)
        {
            if(focusedObject && focusedObject.GetInstanceID() == obj.GetInstanceID())
            {
                continue;
            }
            MovableObject movable = obj.GetComponent<MovableObject>();
            if (obj.transform.position.y > movable.startPos.y)
            {
                obj.transform.Translate(Vector3.down * Time.deltaTime * movable.weight * 3);
                obj.GetComponent<Renderer>().material.color = movable.orgColor;
            }
        }
    }

    private void levitate(GameObject focusedObject, float speed)
    {
        focusedObject.transform.Translate(Vector3.up * Time.deltaTime * speed);
    }

    private void move(GameObject focusedObject, Vector2 moveTo, float speed)
       
    {
        Transform transform = focusedObject.transform;
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
