using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MovableObject : MonoBehaviour {

    public MoveObjects main;

    public float weight = 2;
    public int hitCount = 0;
    public Vector3 startPos;
    public Color orgColor;
    public bool isFocused;

    void Start()
    {
        main = GameObject.Find("EyeTracking").GetComponent<MoveObjects>();
        startPos = transform.position;
        orgColor = GetComponent<Renderer>().material.color;
    }

        void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Obstacle")
        {
            hitCount++;
            main.onObstacleHit(this);
            reset();
        } else if (col.gameObject.tag == "Finish")
        {
            reset();
            main.onFinish(this);
        }
    }

    private void reset()
    {
        transform.position = startPos;
        GetComponent<Renderer>().material.color = orgColor;
    }

}
