using UnityEngine;
using System.Collections.Generic;
using Tobii.EyeTracking;
using System.Linq;


/// <summary>
/// Script that handles object swapping and object color changes
/// 
/// HOW TO USE:
/// 1. Add your texts to Texts/text1.txt file
/// </summary>
public class ChangeWords : MonoBehaviour {


    private Collider[] _changingObjColliders;
    private int _lastFocusedObjID = -1;
    private GameObject _lastFocusedObject = null;

    private List<GameObject> _changingObjects = new List<GameObject>();
    private List<List<string>> words;


    public bool mouseInput = false; //Recognises also mouse cursor position
    public bool changeGazeWord = true; //Change word with another word from list
    public bool changeLastWord = false; //Change previous word with another word
    public bool scrambleGazeWord = false; //Scrambles the word you are looking at
    public bool scrambleLastWord = false; //Scrambles previous word
    public bool scrambleMiddle = true; //Scrambles only middle of the word (leaves 1st and last character in place)

    //Word position and prefab
    public float borderSizeSides = 2f;
    public float borderSizeUpper = 2f; 
    public float spaceSize = 0.8f;
    public float spaceBetweenLines = 0.5f;
    public GameObject textPrefab = null;

    private Ray _ray;
    private RaycastHit _rayHit;

	//Init 
	void Start()
	{

        CreateText createTextScript = gameObject.GetComponent<CreateText>();
        words = createTextScript.WordsInText;

        _changingObjects = InitText(words);
        
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

    //Instantiates words of text as separate game objects
    private List<GameObject> InitText(List<List<string>> words)
    {
        List<GameObject> objectList = new List<GameObject>();

        Vector3 startWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(0,Screen.height,15f)); //Get world point of upper left corner of screen
        Vector3 startPos = new Vector3(startWorldPos.x + borderSizeSides, startWorldPos.y - borderSizeUpper, 0);
        Vector3 nextPos = startPos;

        //Init each word as game object
        foreach (var word in words)
        {
            GameObject gm = Instantiate(textPrefab); //Create new text object
            gm.GetComponent<TextMesh>().text = word[0];
            BoxCollider boxc = gm.AddComponent<BoxCollider>(); //Add box collider to gameObject

            //Get word length and height
            float wordLength = boxc.size.x;
            float wordHeight = boxc.size.y;
            boxc.size = new Vector3(wordLength, wordHeight, 1); //Change size.z of boxCollider

            //Get word end point with and without border
            Vector3 wordEndPos = new Vector3(nextPos.x + wordLength + spaceSize, nextPos.y, 0);
            Vector3 wordEndWithBorder = wordEndPos;
            wordEndWithBorder.x = wordEndWithBorder.x + borderSizeSides;
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(wordEndWithBorder); //Screen position of word ending + space and border

            Debug.Log(screenPoint + " word " + word[0]);

            if (screenPoint.x < Screen.width) //inside of screen
            {
                gm.transform.position = nextPos;
                nextPos = wordEndPos;
            }
            else //outside of screen - in this case add object to next row
            {
                nextPos = new Vector3(startPos.x, nextPos.y - wordHeight - spaceBetweenLines, 0);
                gm.transform.position = nextPos;
                nextPos = new Vector3(nextPos.x + wordLength + spaceSize, nextPos.y, 0);
            }

            objectList.Add(gm);
        }

        return objectList;
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
            if (!_lastFocusedObjID.Equals(gazeObject.GetInstanceID()))
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


                _lastFocusedObjID = gazeObject.GetInstanceID();
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
