using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class MouseTrackSystem : MonoBehaviour
{
    public new Camera camera;
    public World world;
    public Grid grid;

    private Vector2 previousMousePos;
    private Vector3Int previousCell;

    public void Start()
    {
        previousMousePos = MousePos();
        previousCell = GetCell(previousMousePos);
    }

    public void Update()
    {
        Vector2 mousePosCurrent = MousePos();

        if (mousePosCurrent != previousMousePos)
        {
            MouseMoved?.Invoke(mousePosCurrent);
            previousMousePos = mousePosCurrent;
        }

        Vector3Int newCell = GetCell(mousePosCurrent);

        if (newCell != previousCell)
        {
            // Debug.Log(newCell);
            MouseMovedIntoCell?.Invoke(newCell);
            MouseMovedOutOfCell?.Invoke(previousCell);

            if (latestState == true)
            {
                MouseDragHasntClickedYet?.Invoke(latestCell ?? throw new Exception("this should never happen actually"), (Vector2Int)newCell);
            }

            previousCell = newCell;
        }

        if (mouseStateChange)
        {
            SimpleClickResolution(isPressed);
            IntelligentClickResolution(isPressed);
            mouseStateChange = false;
        }
    }


    private bool isPressed;
    private bool mouseStateChange;
    public void OnClick(InputAction.CallbackContext context)
    {
        isPressed = context.ReadValueAsButton();
        mouseStateChange = true;
        // Debug.Log(isPressed);
    }

    private bool latestState;
    private Vector2Int? latestCell;
    
    private void IntelligentClickResolution(bool input)
    {
        Vector2 mousePosCurrent = MousePos();
        bool mouseState = input;
        if (latestState == false && mouseState == true)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            latestState = true;
            latestCell = (Vector2Int) GetCell(mousePosCurrent);
        } else if (latestState == false && mouseState == false)
        {
            // mouse was clicked from within the menu earlier, do nothing
            latestCell = null;
            return;
        } else if (latestState == true && mouseState == false)
        {
            // Debug.Log("IntelligntClickRes 3rd br");
            Vector2Int currentCell = (Vector2Int) GetCell(mousePosCurrent);

            if (latestCell == null)
            {
                Debug.LogWarning("Something went wrong in IntelligentClickResolution, will try to recover by invoking a non-drag click event regardless of latestCell.");
                MouseClickInCell?.Invoke(currentCell);
            } else if (currentCell == latestCell)
            {
                MouseClickInCell?.Invoke(currentCell);
            } else
            {
                MouseDragClickBetweenCells?.Invoke(latestCell ?? throw new Exception("this should never happen actually"), currentCell);
            }

            latestState = false;
            latestCell = null;

        } else if (latestState == true && mouseState == true)
        {
            // this would mean it called this twice... do nothing i guess?
            return;
        }
    }

    private void SimpleClickResolution(bool input)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        bool mouseState = input;

        Vector2 mousePos = MousePos();

        NewMouseStateInPos?.Invoke(mousePos, mouseState);
        NewMouseStateInCell?.Invoke(GetCell(mousePos), mouseState);
    }

    public static (Vector2Int start, Vector2Int end) BuildSquare(Vector2Int v1, Vector2Int v2)
    {
        Vector2Int start = new(
            Mathf.Min(v1.x, v2.x),
            Mathf.Min(v1.y, v2.y)
        );

        Vector2Int end = new(
            Mathf.Max(v1.x, v2.x),
            Mathf.Max(v1.y, v2.y)
        );

        return (start, end);
    }

    public static event Action<Vector2> MouseMoved;

    public static event Action<Vector3Int> MouseMovedIntoCell;
    public static event Action<Vector3Int> MouseMovedOutOfCell;

    public static event Action<Vector2, bool> NewMouseStateInPos;
    public static event Action<Vector3Int, bool> NewMouseStateInCell;
    
    // only triggers on 0 (0(mouse is untouched) -> 1(mouse got pressed) -> 0(stopped pressing) => Invoke!)
    // and only if the mouse stays in the same cell as when 1 was triggered
    public static event Action<Vector2Int> MouseClickInCell;
    
    // like MouseCLickInCell, but triggers if the cell in which 1 was tripped is different than
    // the one in which 0 was tripped afterwards
    public static event Action<Vector2Int, Vector2Int> MouseDragClickBetweenCells;
    public static event Action<Vector2Int, Vector2Int> MouseDragHasntClickedYet;

    public Vector2 MousePos() => Mouse.current.position.ReadValue();

    Vector3Int GetCell(Vector2 pos)
    {
        return grid.WorldToCell(camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }
}