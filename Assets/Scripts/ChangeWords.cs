using UnityEngine;
using System.Collections.Generic;
using Tobii.EyeTracking;
using System.Linq;


/// <summary>
/// Script that handles object swapping and object color changes
/// 
/// HOW TO USE:
/// 1. Add this script to (Empty) GameObject
/// 2. Add objects to that GameObject as children (Make sure that these objects have collider)
/// Note when adding 3D TEXT objects - add box collider as component and change size z from 0 to 1. Otherwise Unity can't detect collision properly.
/// </summary>
public class ChangeWords : MonoBehaviour {


    private Collider[] _changingObjColliders;
    private string _lastFocusedName = "";
    private GameObject _lastFocusedObject = null;

    private List<GameObject> _changingObjects = new List<GameObject>();
    private List<List<string>> words;


    public bool mouseInput = false; //Recognises also mouse cursor position
    public bool changeGazeWord = true; //Change word with another word from list
    public bool changeLastWord = false; //Change previous word with another word
    public bool scrambleGazeWord = false; //Scrambles the word you are looking at
    public bool scrambleLastWord = false; //Scrambles previous word
    public bool scrambleMiddle = true; //Scrambles only middle of the word (leaves 1st and last character in place)

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

        CreateText createTextScript = gameObject.GetComponent<CreateText>();

        words = createTextScript.WordsInText;
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
        if ((changeGazeWord || changeLastWord || scrambleGazeWord || scrambleLastWord) && Physics.Raycast(ray, out _rayHit))
        {

            GameObject gazeObject = _rayHit.transform.gameObject;
            int index = _changingObjects.IndexOf(gazeObject);

            if (index == -1)
            {
                Debug.Log("Object does not belong to changing objects. (" + gazeObject.name + ")");
                return;
            }
            else if ((changeLastWord || scrambleLastWord) && _lastFocusedObject) //If changing last word
            {
                //Let's changes changing object index to last object
                index = _changingObjects.IndexOf(_lastFocusedObject);
            }

            //Change objects if last focused object is not same
            if (!_lastFocusedName.Equals(gazeObject.name)) //TODO Change to ID 
            {

                int randomIndex = Random.Range(0, words[index].Count);
                TextMesh textMesh = _changingObjects[index].GetComponent<TextMesh>();
                string word = textMesh.text;

                //Change word with word in list
                if(changeGazeWord || changeLastWord)
                {
                    int wordIndex = words[index].IndexOf(word);

                    //If same word is randomly chosen, then select next word
                    if (randomIndex == wordIndex)
                    {
                        randomIndex = (wordIndex + 1) % words[index].Count;
                    }

                    string newWord = words[index][randomIndex];
                    Debug.Log(word + " is swapped with " + newWord);

                    textMesh.text = newWord;

                } //Scramble word
                else if(scrambleGazeWord || scrambleLastWord)
                {
                    string shuffled;

                    if(scrambleMiddle)
                    {
                        string subword = word.Substring(1, word.Length - 2);
                        Debug.Log(subword);
                        shuffled = Shuffle(subword);
                        shuffled = word.First() + shuffled + word.Last();
                    }
                    else
                    {
                        shuffled = Shuffle(word);
                    }

                    Debug.Log(word + " is swapped with " + shuffled);

                    textMesh.text = shuffled;
                }


                _lastFocusedName = gazeObject.name;
                _lastFocusedObject = gazeObject;

            }
        }
    }


    public static string Shuffle(string word)
    {
        char[] chars = new char[word.Length];
        int index = 0;
        while (word.Length > 0)
        { // Get a random number between 0 and the length of the word. 
            int next = Random.Range(0, word.Length); // Take the character from the random position 
                                                      //and add to our char array. 
            chars[index] = word[next];                // Remove the character from the word. 
            word = word.Substring(0, next) + word.Substring(next + 1);
            ++index;
        }
        return new string(chars);
    }
}
