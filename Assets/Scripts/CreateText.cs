using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


public class CreateText : MonoBehaviour
{

    public string fileName = "Texts/text1.txt";
    private List<List<string>> wordsInText = new List<List<string>>(); //Each list contains word in given place of text,
    //ie [[Test,Text,Next],[...],[..]] Test, Text, Next are all first words of three different texts.




    void Awake()
    {
        wordsInText = ReadFile(fileName);
    }

    //Read file to suitable list
    List<List<string>> ReadFile(string fileName)
    {

        List<List<string>> resultList = new List<List<string>>();

        try
        {
            string content = System.IO.File.ReadAllText(fileName);
            List<string> texts = content.Split('-').ToList(); //Split into subtexts, separated by "-"

            bool isFirstText = true;

            foreach (string text in texts)
            {
                //Split into lines ("\r\n")
                string textTemp = text.Replace("\r\n", "\n");
                textTemp = textTemp.Replace("\r", "\n");
                List<string> lines = Regex.Split(textTemp, "\n").ToList();


                List<string> words = new List<string>();

                // Split all lines of text to words and add them to list
                foreach (string line in lines)
                {


                    if (line != "")
                    {
                        Debug.Log("Line: " + line);
                        //Split lines into words and add words to list
                        words.AddRange(line.Split().ToList());
                    }
                }

                //Fill list of lists with words - 0: contains first words of each text, 1: second and so on 
                if (isFirstText)
                {
                    foreach (string word in words)
                    {
                        resultList.Add(new List<string>() { word });
                    }
                }
                else
                {
                    for (int i = 0; i < words.Count; i++)
                    {
                        if (resultList.Count > i)
                        {
                            resultList[i].Add(words[i]);
                        }
                        else
                        {
                            Debug.Log("Word out of index: " + words[i]);
                        }
                    }
                }

                //Second time we want to append already initialized lists.
                isFirstText = false;

            }

            return resultList;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public List<List<string>> WordsInText
    {
        get
        {
            return wordsInText;
        }

        set
        {
            wordsInText = value;
        }
    }
}