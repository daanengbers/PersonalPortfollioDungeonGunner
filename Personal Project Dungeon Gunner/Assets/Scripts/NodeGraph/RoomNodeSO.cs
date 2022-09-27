using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
   
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        // draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);

        // start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();

        // display a popup using the RoomNodeType name values that can be selected from (default to currently set roomNodeType)
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
        
            EditorUtility.SetDirty(this);

            GUILayout.EndArea();
        
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i ++)
        {
            if(roomNodeTypeList.list[i].DisplayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        // switch cases by different eventTypes and call their functions
        switch (currentEvent.type)
        {

            case EventType.MouseDown:
                processMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                processMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                processMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void processMouseDownEvent(Event currentEvent)
    {
        // if left mouse button is clicked run process
        if(currentEvent.button == 0)
        {
            processLeftClickDownEvent();
        }
    }

    private void processLeftClickDownEvent()
    {
        //selects the object in the unity editor when clicked
        Selection.activeObject = this;

        // toggle node selection
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }

    }

    private void processMouseUpEvent(Event currentEvent)
    {
        // if left mouse button goes up call function
        if (currentEvent.button == 0)
        {
            processLeftClickUpEvent();
        }
    }

    private void processLeftClickUpEvent()
    {
        // if left mouse button goes up during dragging, set dragging to false
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void processMouseDragEvent(Event currentEvent)
    {
        // if left mouse button is dragging call function
        if(currentEvent.button == 0)
        {
            processLeftMouseDragEvent(currentEvent);
        }
    }

    private void processLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        // currentEvent.delta measures the relative distance between the last event end the mouse position
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    private void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }
#endif
    #endregion Editor Code
}
