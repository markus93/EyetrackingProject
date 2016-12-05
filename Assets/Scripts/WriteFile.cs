﻿using UnityEngine;
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
        //Every 20ms write line to file (50Hz)
        InvokeRepeating("WriteLineToFile", 0f, 0.02f);
	}
	
	// Update is called once per frame
	void WriteLineToFile () {
        using (StreamWriter sw = new StreamWriter(fileName, true))
        {
            Vector2 gazePoint = EyeTracking.GetGazePoint().Screen;
            Vector2 roundedSampleInput = new Vector2(Mathf.RoundToInt(gazePoint.x), Mathf.RoundToInt(gazePoint.y));
            string time = DateTime.Now.ToString("HH_mm_ss__fff-dd-MM-yyyy");
            // Add line to file
            sw.WriteLine(time + "," + roundedSampleInput.x + "," + roundedSampleInput.y );
        }
    } 
}
