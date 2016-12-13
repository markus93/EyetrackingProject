using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tobii.EyeTracking;
using System.Linq;
using System;

public class MoveObjects : MonoBehaviour {
    public WriteFile logger;
    //mouse support
    public bool isMouseInput = true;
    public bool resetOnHitObstacle = true;
    public bool debugEnabled = false;

    public GameObject gazeawareObject;
    public GameObject[] images;
    public GameObject[] words;
    public GameObject[] distractorSpawns;
    public GameObject canvas;
    public Text timer;

    public int distractorMinDistance = 0;
    public int distractorMaxDistance = 300;
    public int distractorShowTime = 2;

    public GameObject gazePointTracker;
    public Camera myCamera;

    private Ray _ray;
    private RaycastHit _rayHit;

    private IDataProvider<GazePoint> _gazePointProvider;
    private ITimestamped _lastHandledPoint;
    private Color focusColor = Color.blue;
    private GazePoint lastGazePoint = GazePoint.Invalid;

    private bool started;
    private bool ended;
    private bool isFocused;
    private double focusTime;
    private float startTime;
    private int lostFocusCount;

    // Use this for initialization
    void Start () {
        _gazePointProvider = EyeTrackingHost.GetInstance().GetGazePointDataProvider();
        gazePointTracker = GameObject.FindWithTag("gazepoint");
        gazeawareObject = GameObject.FindWithTag("gazeaware");
        canvas = GameObject.Find("Canvas");
        timer = GameObject.Find("Timer").GetComponent<Text>();
        myCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        EyeTracking.SetCurrentUserViewPointCamera(myCamera);
        logger.contWrite = false;


        //foreach (GameObject obj in gazeawareObjects)
        //{
        //Debug.Log("Gaze aware objects " + obj.name);
        //}
        //Debug.Log("Tracking point" + gazePointTracker.name);
    }      

    // Update is called once per frame
    void Update () {
        if (ended)
        {
            return;
        }
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
                fall();                
                if(focusedObject != null)
                {
                    gazeawareFocused(gazePointInWorld, focusedObject);
                }
            }
        }

        if (this.started)
        {
            updateTimer();
        }
    }
   

    private void readMouseInput()
    {       
         _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 gazePointInWorld = _ray.GetPoint(-Camera.main.transform.position.z);
        updateTrackerPosition(gazePointInWorld);

        if (!Physics.Raycast(_ray, out _rayHit))
        {
            fall();
            return;
        }
        GameObject focusedObject = _rayHit.transform.gameObject;
        if (focusedObject && focusedObject.tag == "gazeaware")
        {
            gazeawareFocused(_rayHit.point, focusedObject);
        }  else
        {
            fall();
        }
    }

    private void gazeawareFocused(Vector3 gazePoint, GameObject focusedObject)
    {
        if (!started)
        {
            startTimer();
        }
        focusedObject.GetComponent<Renderer>().material.color = focusColor;
        focusTime++;
        float weight = focusedObject.GetComponent<MovableObject>().weight;
        move(focusedObject, gazePoint, weight);

        if (!this.isFocused)
        {
            this.isFocused = true;
            logFoundFocus(gazeawareObject.transform.position.x, gazeawareObject.transform.position.y);
        }
    }

    private Vector3 getPositionInWorld(Vector2 gazePoint)
    {
        Vector3 onScreen = new Vector3(gazePoint.x, gazePoint.y, 5);
        Vector3 inWorld = myCamera.ScreenToWorldPoint(onScreen);
        //Debug.Log("Screen x: " + gazePoint.x + ", y:" + gazePoint.y);
        //Debug.Log("World x: " + inWorld.x + ", y:" + inWorld.y);
        return inWorld;
    }

    private Vector3 getPositionOnScreen(Vector3 inWorld)
    {
        Vector2 onScreen = myCamera.WorldToScreenPoint(inWorld);
        //Debug.Log("World x: " + inWorld.x + ", y:" + inWorld.y);
        //Debug.Log("Screen x: " + onScreen.x + ", y:" + onScreen.y);
        return onScreen;
    }

    private void startTimer()
    {
        this.started = true;
        this.startTime = Time.time;
        logger.WriteLineToFile(getTime() + " STARTED");
    }

    private void updateTimer()
    {
        timer.text = Math.Round(Time.time - this.startTime).ToString();
    }

    private void updateTrackerPosition(Vector3 moveTo)
    {
        setTransformXY(gazePointTracker, moveTo.x, moveTo.y);
    }

    private void setTransformXY(GameObject gameObject, float x, float y)
    {
        gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
    }

    private void fall()
    {
        MovableObject movable = gazeawareObject.GetComponent<MovableObject>();
        if (gazeawareObject.transform.position.y > movable.startPos.y)
        {
            gazeawareObject.transform.Translate(Vector3.down * Time.deltaTime * movable.weight * 3);
            gazeawareObject.GetComponent<Renderer>().material.color = movable.orgColor;
        }

        if (this.isFocused)
        {
            this.isFocused = false;
            this.lostFocusCount++;
            logLostFocus(this.lostFocusCount, gazeawareObject.transform.position.x, gazeawareObject.transform.position.y);
        }
    }

    private void move(GameObject focusedObject, Vector2 moveTo, float speed)
       
    {
        Transform transform = focusedObject.transform;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(moveTo.x, moveTo.y, transform.position.z), Time.deltaTime * speed);
    }


    //TRIGGER EVENTS

    public void onObstacleHit(GameObject movable, GameObject obs)
    {
        MovableObject mo = movable.GetComponent<MovableObject>();
        mo.reset();
        Vector2 pos = getPositionOnScreen(movable.transform.position);
        logObstacleHit(mo.hitCount, pos.x, pos.y);
    }

    public void onTriggerHit(GameObject movable, GameObject trig)
    {
        if(trig.tag == "distractor_word")
        {
            ShowDistractor(trig.tag, this.words, movable.transform.position);
        } else if (trig.tag == "distractor_image")
        {
            ShowDistractor(trig.tag, this.images, movable.transform.position);
        }            
    }

    void ShowDistractor(string type, GameObject[] distractors, Vector3 location)
    {
        Vector2 screenPosition = getPositionInWorld(location);
        int x = UnityEngine.Random.Range(this.distractorMinDistance, this.distractorMaxDistance);
        int y = UnityEngine.Random.Range(this.distractorMinDistance, this.distractorMaxDistance);
        GameObject distractor = getRandomEelement(distractors);
        GameObject inst = Instantiate(distractor);
        inst.GetComponent<RectTransform>().position = new Vector3(screenPosition.x + x, screenPosition.y + y, 0);
        inst.transform.SetParent(canvas.transform);
        logDistractorHit(type, distractor.name, inst.transform.position.x, inst.transform.position.y, screenPosition.x, screenPosition.y);
        StartCoroutine(HideDistractor(inst));
    }

    private IEnumerator HideDistractor(GameObject inst)
    {
        yield return new WaitForSeconds(2);
        GameObject.Destroy(inst);
    }

    public void onFinish(MovableObject mo)
    {
        this.ended = true;
        GameObject go = GameObject.Find("FinishText");
        string time = Math.Round(Time.time - this.startTime).ToString();
        logger.WriteLineToFile(getTime() + " FINISH, LOST_FOCUS_COUNT:" + this.lostFocusCount + " , OBSTACLE_HIT_COUNT: " + mo.hitCount + ", TIME: " + time);

        string text = "You did it!\n\nTime: " + time + "\nCollisions: " +mo.hitCount+ "\nFocus lost: "+this.lostFocusCount;
        Text textHolder = go.GetComponent<Text>();
        textHolder.text = text;
    }

    public void HideWin()
    {
        GameObject go = GameObject.Find("Victory");
        Image img = go.GetComponent<Image>();
        img.enabled = false;
    }

    private GameObject getRandomEelement(GameObject[] array)
    {
        var ind = UnityEngine.Random.Range(0, (array.Length - 1));
        return array[ind];
    }

    //LOGGING

    private void logFoundFocus(float trackedX, float trackedY)
    {
        logger.WriteLineToFile(getTime() + " FOUND_FOCUS, CUBE_X: " + trackedX + ", CUBE_Y: " + trackedY);
    }

    private void logLostFocus(int count, float trackedX, float trackedY)
    {
        logger.WriteLineToFile(getTime() + " LOST_FOCUS, COUNT:" + count + " , CUBE_X: " + trackedX + ", CUBE_Y: " + trackedY);
    }

    private void logObstacleHit(int count, float trackedX, float trackedY)
    {
        logger.WriteLineToFile(getTime() + " HIT_OBSTACLE, COUNT:" + count + " , CUBE_X: " + trackedX + ", CUBE_Y: " + trackedY);
    }

    private void logDistractorHit(string type, string id, float distX, float distY, float trackedX, float trackedY)
    {
        logger.WriteLineToFile(getTime() + " " + type + ", " + id + ", DISTRACTOR_X: " + distX + " , DISTRACTOR_Y: " + distY + " , CUBE_X: " + trackedX + ", CUBE_Y: " + trackedY);
    }

    private string getTime()
    {
        return DateTime.Now.ToString("HH_mm_ss__fff-dd-MM-yyyy");
    }

    //TEMP

    /*
        private void levitate(GameObject focusedObject, float speed)
        {
            focusedObject.transform.Translate(Vector3.up * Time.deltaTime * speed);
        }

        private void move(GameObject focusedObject, Vector2 moveTo, float speed)

        {
            Transform transform = focusedObject.transform;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(moveTo.x, moveTo.y, transform.position.z), Time.deltaTime * speed);
            //gazeAwareObject.transform.Translate(new Vector2(moveTo.x, moveTo.y) * Time.deltaTime * speed);
            if (moveTo.x < gazeAwareObject.transform.position.x)
            {
                gazeAwareObject.transform.Translate(Vector3.left * Time.deltaTime * speed );
            } else if(moveTo.x > gazeAwareObject.transform.position.x)
            {
                gazeAwareObject.transform.Translate(Vector3.right * Time.deltaTime * speed );
            }
        }

        private void fall(GameObject focusedObject)
        {
            foreach (GameObject obj in gazeawareObjects)
            {
                if (focusedObject && focusedObject.GetInstanceID() == obj.GetInstanceID())
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
         */
}
