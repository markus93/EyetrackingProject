using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Tobii.EyeTracking;

//Test writing info to file
public class WriteFile : MonoBehaviour {

    private string filePath = "Info";
    private string fileName;

	// Use this for initialization
	void Start () {

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        fileName = filePath + "/Test" + DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss") + ".txt";

        //Every 20ms write line to file (50Hz)
        InvokeRepeating("WriteLineToFile", 0f, 0.02f);
	}
	
	// Update is called once per frame
	void WriteLineToFile () {
        using (StreamWriter sw = new StreamWriter(fileName, true))
        {
            //Only log if eyetracker active, meaning screen values are not NaN
            Vector2 gazePoint = EyeTracking.GetGazePoint().Screen;
            if (!gazePoint.x.Equals(float.NaN))
            {
                Debug.Log("New line to text" + " " + gazePoint.x);
                Vector2 roundedSampleInput = new Vector2(Mathf.RoundToInt(gazePoint.x), Mathf.RoundToInt(gazePoint.y));
                string time = DateTime.Now.ToString("HH_mm_ss__fff-dd-MM-yyyy");
                // Add line to file
                sw.WriteLine(time + "," + roundedSampleInput.x + "," + roundedSampleInput.y);
            }
            else
            {
                //Debug.Log("Eyetracker inactive");
            }
        }
    }


    public void WriteLineToFile(String line)
    {
        using (StreamWriter sw = new StreamWriter(fileName, true))
        {
            string time = DateTime.Now.ToString("HH_mm_ss__fff-dd-MM-yyyy");
            sw.WriteLine(time + "," + line);
        }
    }
}
