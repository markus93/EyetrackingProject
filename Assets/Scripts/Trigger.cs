using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour {
    public MoveObjects main;
    public int cooldown = 2;
    public bool isTriggered = false;
    // Use this for initialization
    void Start () {
        main = GameObject.Find("EyeTracking").GetComponent<MoveObjects>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider col)
    {
        if (!isTriggered)
        {
            main.onTriggerHit(col.gameObject, this.gameObject);
            this.isTriggered = true;
            Invoke("ResetTrigger", this.cooldown);
        }
    }

    private void ResetTrigger()
    {
        this.isTriggered = false;
    }
}
