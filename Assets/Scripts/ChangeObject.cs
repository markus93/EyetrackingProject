using UnityEngine;
using System.Collections.Generic;
using Tobii.EyeTracking;

/// <summary>
/// Script that handles object swapping and object color changes
/// 
/// HOW TO USE:
/// 1. Add this script to (Empty) GameObject
/// 2. Add objects to that GameObject as children (Make sure that these objects have collider)
/// Note when adding 3D TEXT objects - add box collider as component and change size z from 0 to 1. Otherwise Unity can't detect collision properly.
/// </summary>
public class ChangeObject : MonoBehaviour {


    private Collider[] _changingObjColliders;
    private string _lastFocusedName = "";

    private List<GameObject> _changingObjects = new List<GameObject>();


    public bool mouseInput = false; //Recognises also mouse cursor position
    public bool changeObjects = true; //Change object position with another object
    public bool changeColors = false; //Change object color (selects from list colors) (turn changeObjects false in order to use this)
    public List<Color> colors = new List<Color> { Color.black, Color.blue, Color.green, Color.magenta, Color.red, Color.yellow };


    private Ray _ray;
    private RaycastHit _rayHit;

	//Init 
	void Start()
	{
        //TODO gets all objects with collider in children. (May use also TAG)
		_changingObjColliders = GetComponentsInChildren<Collider>();


		//Populate Changing gameobjects list.
		for (int i = 0; i < _changingObjColliders.Length; i++) 
		{
            Debug.Log("Changing object " + _changingObjColliders[i].name);
			_changingObjects.Add (_changingObjColliders [i].gameObject);
		}

		Debug.Log (_changingObjColliders.Length + " changing objects found.");

	}


	//Check every frame if object is looked at (or pointed at)
	void Update ()
	{

        //Using Gaze coordinates from Eyetracker
        Vector2 gazePosition = EyeTracking.GetGazePoint().Screen;

        if (EyeTracking.GetGazePoint().IsValid)
        {
            //Vector2 roundedSampleInput = new Vector2(Mathf.RoundToInt(gazePosition.x), Mathf.RoundToInt(gazePosition.y));
            //Debug.Log(roundedSampleInput);
            //Debug.Log(Input.mousePosition.ToString());

            _ray = Camera.main.ScreenPointToRay(gazePosition);

            CastRay(_ray);
        //Mouse input
        }else if (mouseInput)
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            CastRay(_ray);

        }
    }

    //Check whether any changing object is hit by ray.
    private void CastRay(Ray ray)
    {
        if (Physics.Raycast(ray, out _rayHit))
        {
            Debug.Log("Hit: " + _rayHit.collider.name);

            GameObject gazeObject = _rayHit.transform.gameObject;
            int index = _changingObjects.IndexOf(gazeObject);

            if(index == -1)
            {
                Debug.Log("Object does not belong to changing objects. (" + gazeObject.name + ")");
                return;
            }

            //Change objects if last focused object is not same
            if (changeObjects && !_lastFocusedName.Equals(gazeObject.name)) //TODO Change to ID 
            {

                int randomIndex = Random.Range(0, _changingObjColliders.Length);

                //If same object as chosen, then swap with next object in the list.
                if (randomIndex == index)
                {
                    randomIndex = (index + 1) % _changingObjColliders.Length;
                }

                GameObject swapObject = _changingObjects[randomIndex];
                Debug.Log(gazeObject.name + " is swapped with " + swapObject.name);

                _lastFocusedName = swapObject.name;

                Swap(gazeObject, swapObject);
            }
            //Change objects color if last focused object is not same
            else if (changeColors && !_lastFocusedName.Equals(gazeObject.name))
            {
                int randomIndex = Random.Range(0, colors.Count);

                //Check whether same color as before
                Color currentColor = gazeObject.GetComponent<Renderer>().material.color;
                int colorIdx = -1;

                if(colors.Contains(currentColor))
                {
                    colorIdx = colors.IndexOf(currentColor);
                }

                //If same color as before, then change to next color in list
                if (randomIndex == colorIdx)
                {
                    randomIndex = (index + 1) % colors.Count;
                }

                ChangeColor(_changingObjects[index], colors[randomIndex]);

                _lastFocusedName = _changingObjects[index].name;

            }


        }
    }

    //Swap object's positions
    void Swap(GameObject obj1, GameObject obj2)
	{
		Vector3 tempPos = obj1.transform.position;

		obj1.transform.position = obj2.transform.position;
		obj2.transform.position = tempPos;
	}

    void ChangeColor(GameObject obj, Color col)
    {
        obj.GetComponent<Renderer>().material.color = col;
        Debug.Log(obj.name + " color changed to " + col);
    }
}
