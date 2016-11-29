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

        fileName = filePath + "/Test" + DateTime.Now.ToString("HH_mm_ss-dd-MM-yyyy") + ".txt";
        if (!File.Exists(fileName))
        {
            //var sr = File.CreateText(fileName);
            //sr.Close();
        }
        //Every 10ms write line to file
        InvokeRepeating("WriteLineToFile", 0f, 0.01f);
	}
	
	// Update is called once per frame
	void WriteLineToFile () {
        using (StreamWriter sw = new StreamWriter(fileName, true))
        {
            Vector2 gazePoint = EyeTracking.GetGazePoint().Screen;
            // Add line to file
            sw.WriteLine(DateTime.Now.ToString("HH_mm_ss__fff-dd-MM-yyyy") + "," + gazePoint.x + "," + gazePoint.y );
        }
    }
}
