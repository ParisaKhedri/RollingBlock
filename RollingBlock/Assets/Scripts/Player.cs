
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    private Vector2 startTouchPosition; // Start position of the swipe
    private Vector2 endTouchPosition;   // End position of the swipe
    private Vector2 swipeDelta;         // Difference between start and end positions
    public float swipeThreshold = 50f;

    public int toppleSpeed = 300;
    private bool isRolling;


    void Start()
    {
        isRolling = false;
        Debug.Log("which side are we on? " + GetTileSide(transform.position));


    }

void Update()
    {
        // swiping event hanler 
        // listen for swiping events!
        DetectSwipe();
        Debug.Log("which side are we on? " + GetTileSide(transform.position));

    }
    void DetectSwipe()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse input (useful for testing in the editor)
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            ProcessSwipe();
        }
#endif

        // Touch input (for mobile devices)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    endTouchPosition = touch.position;
                    ProcessSwipe();
                    break;
            }
        }
    }

    void ProcessSwipe()
    {
        // Calculate the swipe delta
        swipeDelta = endTouchPosition - startTouchPosition;

        // Check if the swipe is significant enough
        if (swipeDelta.magnitude > swipeThreshold)
        {
            // Determine swipe direction
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if(!isRolling)
                {
                    if (swipeDelta.x > 0)
                        OnSwipeRight();
                    else
                        OnSwipeLeft();

                }
                    
            }
            else
            {
                if (!isRolling)
                {
                    if (swipeDelta.y > 0)
                        OnSwipeUp();
                    else
                        OnSwipeDown();
                }
            }
        }
    }

    void OnSwipeRight()
    {
        Debug.Log("Swipe Right Detected!");
        StartCoroutine(topple(Vector3.right));    
        
    }

    void OnSwipeLeft()
    {
        Debug.Log("Swipe Left Detected!");
        StartCoroutine(topple(Vector3.left));

    }

    void OnSwipeUp()
    {
        Debug.Log("Swipe Up Detected!");
        StartCoroutine(topple(Vector3.forward));

    }

    void OnSwipeDown()
    {
        Debug.Log("Swipe Down Detected!");
        StartCoroutine(topple(Vector3.back));

    }
    // rolling for 1x1x1
    IEnumerator topple(Vector3 direction){


       
        Debug.Log("This is the rol functi");
        
        isRolling = true;
        float remainingAngle = 90;
        Vector3 rotationCenter = transform.position + direction / 2 + Vector3.down / 2;
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);


        Debug.DrawLine(rotationCenter, rotationCenter + Vector3.up * 2, Color.yellow, 8f); // Shows the rotation center
        Debug.DrawLine(rotationCenter, rotationCenter + rotationAxis * 2, Color.blue, 2f); // Shows the rotation axis

        while (remainingAngle > 0) 
        { 
            float rotationAngle = Mathf.Min(Time.deltaTime * toppleSpeed, remainingAngle);
            transform.RotateAround(rotationCenter, rotationAxis, rotationAngle);
            remainingAngle -= rotationAngle;
            yield return null;

        }


        isRolling = false;
    

    }

    // Detect whether you're standing on a 1x1 or 1x2 side based on the character's position
    public string GetTileSide(Vector3 position)
    {
        // Assuming your tiles are perfectly aligned and have a consistent size (e.g., 1 unit size)
        // This is based on checking the local direction of the character's position
        float tileWidth = 1f;  // Width of the tile (assumed to be square or 1x1 by default)
        float tileHeight = 2f; // Height for a 1x2 side
        Vector3 currentTilePosition = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));

        // Check if the tile is 1x2 or 1x1 based on the direction of the tile the character is standing on
        // We'll assume the tile has been placed on a larger surface like a floor or wall.
        if (Mathf.Abs(position.x - currentTilePosition.x) < tileWidth && Mathf.Abs(position.z - currentTilePosition.z) < tileWidth)
        {
            return "1x1";  // If you're standing on a standard 1x1 tile
        }
        else if (Mathf.Abs(position.x - currentTilePosition.x) < tileHeight && Mathf.Abs(position.z - currentTilePosition.z) < tileWidth)
        {
            return "1x2";  // If you're standing on a 1x2 side (likely placed horizontally)
        }
        else
        {
            return "Unknown";  // Return an unknown state if it's a different condition
        }
    }







    /* // rolling for 1x1x2 does not work propperly
    
     IEnumerator topple(Vector3 direction)
    {
        Debug.Log("Player position before rotation: " + transform.position);

        isRolling = true;

        float remainingAngle = 90f;


        // Calculate the rotation center
        Vector3 rotationCenter = transform.position
                                 + (direction / 2)
                                 - Vector3.up; // Move 1 unit down for height adjustment

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);


        Debug.DrawLine(rotationCenter, rotationCenter + Vector3.up * 2, Color.yellow, 8f); // Shows the rotation center
        Debug.DrawLine(rotationCenter, rotationCenter + rotationAxis * 2, Color.blue, 2f); // Shows the rotation axis


        // Perform the rotation
        while (remainingAngle > 0)
        {
            float rotationAngle = Mathf.Min(Time.deltaTime * toppleSpeed, remainingAngle);
            remainingAngle -= rotationAngle;
            yield return null;
        }

        // Snap the character to the grid after rotation
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            Mathf.Round(transform.position.z)
        );

        Debug.Log("Player position after rotation: " + transform.position);
        isRolling = false;

    }*

    /* // no consideration on the characters size
     IEnumerator topple(Vector3 direction){


       
        Debug.Log( "player position before rotation" + transform.position + 
            "player x position" + transform.position.x + 
            "\n" +
            "player y position" + transform.position.y +
            "\n" +
            "player z position" + transform.position.z
        );
        // Snap position before rotation
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            Mathf.Round(transform.position.z)
        );

        isRolling = true;
        float remainingAngle = 90;
        Vector3 rotatonCenter = transform.position + direction / 2 + Vector3.down / 2;
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

        while (remainingAngle > 0) 
        { 
            float rotationAngle = Mathf.Min(Time.deltaTime * toppleSpeed, remainingAngle);
            transform.RotateAround(rotatonCenter, rotationAxis, rotationAngle);
            remainingAngle -= rotationAngle;
            yield return null;

        }
        transform.position = new Vector3(
           Mathf.Round(transform.position.x),
           Mathf.Round(transform.position.y),
           Mathf.Round(transform.position.z)
        );

        
        Debug.Log("player position after rotation" + transform.position +
            "player x position" + transform.position.x +
            "\n" +
            "player y position" + transform.position.y +
            "\n" +
            "player z position" + transform.position.z
        );

        isRolling = false;

   
    }*/



}
