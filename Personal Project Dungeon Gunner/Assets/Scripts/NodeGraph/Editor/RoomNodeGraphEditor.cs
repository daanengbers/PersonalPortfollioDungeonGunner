using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle RoomNodeStyle;
    private GUIStyle RoomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 100f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;


   [MenuItem("Room Node Graph Editor", menuItem ="Window/Dungeon Editor/Room Node Graph Editor" )]

   private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Editor Graph");
    }


    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;

        RoomNodeStyle = new GUIStyle();
        RoomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        RoomNodeStyle.normal.textColor = Color.white;
        RoomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        RoomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        RoomNodeSelectedStyle = new GUIStyle();
        RoomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        RoomNodeSelectedStyle.normal.textColor = Color.white;
        RoomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        RoomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);



        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void OnGUI()
    {

       if (currentRoomNodeGraph != null)
        {
            //draw line when dragged
            DrawDraggedLine();

            //process events
            ProcessEvents(Event.current);

            //draw connections between rooms
            DrawRoomConnections();

            //draw room nodes
            DrawRoomNodes();
        }  

        if (GUI.changed)
        {
            Repaint();
        }
    }   
    
    private void DrawDraggedLine()
    {
        if(currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            //draw line from node to line position
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }
    
    private void ProcessEvents(Event currentEvent)
        {

            //get room node that mouse is currently over
            if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
            {
                currentRoomNode = isMouseOverRoomNode(currentEvent);
            }

            //if mouse isnt over a node and not making a line
            if (currentRoomNode == null ||  currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            {
                processRoomNodeGraphEvents(currentEvent);
            }
            //else process room node events
            else
            {
                currentRoomNode.ProcessEvents(currentEvent);
            }

        }


    // Check if mouse is over roomnode if yes return roomnode  
    private RoomNodeSO isMouseOverRoomNode(Event currentEvent)
    {
        for(int i = currentRoomNodeGraph.roomNodeList.Count -1; i>=0; i--)
        {
            if(currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void processRoomNodeGraphEvents(Event currentEvent)
    {
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
        if(currentEvent.button == 1)
        {
            showContextMenu(currentEvent.mousePosition);
        }
        else if(currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNode();
        }
    }

    private void showContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("select all room nodes"), false, SelectAllRoomNodes);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        //if current node graph is empty, add entrance room first
        if(currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.IsEntrance));
        }

        //create a room node without a type
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.IsNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // create room node scriptable assset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // Add room node to current room node graph room node list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // Set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // Add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // refresh graph node dictionary
        currentRoomNodeGraph.OnValidate();
    }


    //clear selection from all selected roomNodes
    private void ClearAllSelectedRoomNode()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    private void SelectAllRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void processMouseUpEvent(Event currentEvent)
    {
        //if releasing the right mousebutton and is currently dragging a line
        if(currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {

            //check if over room node
            RoomNodeSO roomNode = isMouseOverRoomNode(currentEvent);

            //if so set it as a child of the parent room node if it can be added
            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    private void processMouseDragEvent(Event currentEvent)
    {
        //Check right mouse dragged event
        if(currentEvent.button == 1)
        {
            processRightMouseDragEvent(currentEvent);
        }

    }

    private void processRightMouseDragEvent(Event currentEvent)
    {
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConncetingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConncetingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void DrawRoomConnections()
    {
        //loop trough all room nodes
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            // checks if roomnode has a child
            if(roomNode.childRoomNodeIDList.Count > 0)
            {
                //loop through child room nodes
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //get child room node from dictionary
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        //get line begin and end position
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        //calculate midposition
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        //calculate direction vector
        Vector2 direction = endPosition - startPosition;

        //calculate normalised perpendicular positions from the mid point
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //calculate mid point offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        //draw arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        //draw line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth); 

        GUI.changed = true;
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in  currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(RoomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(RoomNodeStyle);
            }
        }
        GUI.changed = true;
    }

    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if(roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
