using UnityEngine;
using System.Collections.Generic;
using Tobii.EyeTracking;
using System.Linq;


/// <summary>
/// Script that handles object swapping and object color changes
/// 
/// HOW TO USE:
/// 1. Via unity editor, click gameObject "Scripts" and type texts path (e.g "Texts/text1.txt")
/// </summary>
public class ChangeWords : MonoBehaviour {


    private List<BoxCollider> _changingObjColliders;
    private int _lastFocusedObjID = -1;
    private GameObject _lastFocusedObject = null;

    private List<GameObject> _changingObjects = new List<GameObject>();
    private List<List<string>> words;


    public bool mouseInput = false; //Recognises also mouse cursor position
    public bool changeWordToInitial = false; //changes word back to initial text, 
    //meaning that when we read in the text, we don't read words only from first text, but from every other text except first one 
    public bool changeWord = true; //Change word with another word from list
    public bool changeLastWord = false; //Change previous word with another word
    public bool scrambleWord = false; //Scrambles the word you are looking at
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
        HandleColliders handleCollidersScript = gameObject.GetComponent<HandleColliders>();
        words = createTextScript.WordsInText;

        InitText(words);

        handleCollidersScript.setColliders(_changingObjColliders);
    }


    //Check every frame if object is looked at (or pointed at)
    void Update ()
	{

        //Using Gaze coordinates from Eyetracker
        Vector2 gazePosition = EyeTracking.GetGazePoint().Screen;

        if (EyeTracking.GetGazePoint().IsValid)
        {
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
    private void InitText(List<List<string>> words)
    {
        List<GameObject> objectList = new List<GameObject>();
        List<BoxCollider> colliderList = new List<BoxCollider>();


        Vector3 startWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(0,Screen.height,15f)); //Get world point of upper left corner of screen
        Vector3 startPos = new Vector3(startWorldPos.x + borderSizeSides, startWorldPos.y - borderSizeUpper, 0);
        Vector3 nextPos = startPos;

        int wordIndex = 0; //from which text word is read

        //Init each word as game object
        foreach (var word in words)
        {
            //Scramble initial text, so it won't contain any words from first text (usually contains only first text)
            if(changeWordToInitial)
            {
                wordIndex = Random.Range(1, word.Count);
            }


            GameObject gm = Instantiate(textPrefab); //Create new text object
            gm.GetComponent<TextMesh>().text = word[wordIndex];
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

            //Debug.Log(screenPoint + " word " + word[0]);

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
            colliderList.Add(boxc);
        }

        //Add new lists to class variables
        _changingObjects = objectList;
        _changingObjColliders = colliderList;
    }

    //Check whether any changing object is hit by ray.
    private void CastRay(Ray ray)
    {
        if ((changeWordToInitial || changeWord || changeLastWord || scrambleWord || scrambleLastWord) && Physics.Raycast(ray, out _rayHit))
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

                int randomIndex = Random.Range(0, words[index].Count); //used to choose randomly another word
                TextMesh textMesh = _changingObjects[index].GetComponent<TextMesh>();
                string word = textMesh.text;

                //Change word with word in list
                if(changeWord || changeWordToInitial || changeLastWord)
                {
                    int wordIndex = words[index].IndexOf(word);

                    // change back to word from initial text
                    if( changeWordToInitial)
                    {
                        randomIndex = 0;
                    }
                    //If same word is randomly chosen, then select next word
                    else if (randomIndex == wordIndex)
                    {
                        randomIndex = (wordIndex + 1) % words[index].Count;
                    }

                    string newWord = words[index][randomIndex];
                    Debug.Log(word + " is swapped with " + newWord);

                    textMesh.text = newWord;

                } //Scramble word
                else if(scrambleWord || scrambleLastWord)
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
