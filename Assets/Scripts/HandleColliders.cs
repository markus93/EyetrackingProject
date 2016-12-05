using UnityEngine;
using System.Collections.Generic;


public class HandleColliders:MonoBehaviour
{

    private static Texture2D backgroundTexture;
    private static GUIStyle textureStyle;
    public GUIStyle highlightStyle;
    private List<BoxCollider> colliders;
    private List<Rect> colliderRects;
    private bool isHighlightedColliders = false;
    private CommonVariables com = null;

    void Start()
    {

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            isHighlightedColliders = !isHighlightedColliders;
            //Debug.Log("Highlighted: " + isHighlightedColliders);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Move collider up
            MoveCollider(0.1f);
            if (com)
            {
                com.ColliderAdjustment += 0.1f;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Move colliders down
            MoveCollider(-0.1f);
            if (com)
            {
                com.ColliderAdjustment -= 0.1f;
            }
        }
    }

    void OnGUI()
    {
        if(isHighlightedColliders)
        {
            foreach (var colRect in colliderRects)
            {                
                GUI.Box(colRect, "", highlightStyle);
            }
        }        
    }

    public void setColliders(List<BoxCollider> cols)
    {
        colliders = cols;

        //Calculate new collider rects
        colliderRects = new List<Rect>();

        foreach (var col in colliders)
        {
            Vector3 pos = col.transform.position;
            Vector3 posOnScreen = Camera.main.WorldToScreenPoint(col.transform.position);
            Vector3 widthPos = Camera.main.WorldToScreenPoint(new Vector3(pos.x + col.size.x, pos.y, pos.z)) - posOnScreen;
            Vector3 heightPos = Camera.main.WorldToScreenPoint(new Vector3(pos.x, pos.y + col.size.y, pos.z)) - posOnScreen;
            float width = widthPos.x;
            float height = heightPos.y;

            colliderRects.Add(new Rect(posOnScreen.x, Screen.height - posOnScreen.y, width, height));

        }

        //If scriptsDontDestroy found, move colliders to right place
        GameObject gameObj = GameObject.Find("ScriptsDontDestroy"); //Change that to more viable solution?
        if (gameObj)
        {
            com = gameObj.GetComponent<CommonVariables>();

            Debug.Log("CommonVariables:" + com.name + " " + com.ColliderAdjustment);
            MoveCollider(com.ColliderAdjustment);
        }
    }

    private void MoveCollider(float changeAmount)
    {
        Vector3 pos;
        Rect rect;
        BoxCollider boxc;

        for (int i = 0; i < colliders.Count; i++)
        {
            //Move collider up
            boxc = colliders[i];
            boxc.center = new Vector3(boxc.center.x, boxc.center.y + changeAmount, boxc.center.z);
            colliders[i] = boxc;

            //Get collider's new pos
            float startPosY = boxc.size.y / 2;
            float newPosY = startPosY + boxc.center.y;
            //Calculate new position of the rectangle on screen
            pos = colliders[i].transform.position;
            pos.y = pos.y + newPosY;
            pos = Camera.main.WorldToScreenPoint(pos);
            rect = colliderRects[i];
            rect.y = Screen.height - pos.y;
            colliderRects[i] = rect;
        }
    }
}
  