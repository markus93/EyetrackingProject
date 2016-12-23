using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CommonVariables : MonoBehaviour {

    private float colliderAdjustment = 0f; //how much is collider adjusted
    public KeyCode keyNextScene = KeyCode.N;
    public KeyCode keyResetScene = KeyCode.R;
    private int levelCount = 1;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    // Use this for initialization
    void Start () {
        levelCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log("Level count: " + levelCount);
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(keyNextScene))
        {
            int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;

            Debug.Log("Index: " + currentSceneIdx);

            if (currentSceneIdx + 1 == levelCount)
            {
                Debug.Log("Quit from application!");
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(currentSceneIdx + 1);
            }
        }
        else if (Input.GetKeyDown(keyResetScene))
        {

            int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;

            if( currentSceneIdx != 0)
            {
                Debug.Log("Reseting current level.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.Log("Cannot restart initial level.");  // Causes problems with scripts do not destroy on load.
            }
        }

    }


    public float ColliderAdjustment
    {
        get
        {
            return colliderAdjustment;
        }

        set
        {
            colliderAdjustment = value;
        }
    }
}
