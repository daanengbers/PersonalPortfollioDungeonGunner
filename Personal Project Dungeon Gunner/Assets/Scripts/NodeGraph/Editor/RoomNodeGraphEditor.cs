using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle RoomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 100f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private RoomNodeTypeListSO roomNodeTypeList;


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
        RoomNodeStyle = new GUIStyle();
        RoomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        RoomNodeStyle.normal.textColor = Color.white;
        RoomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        RoomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnGUI()
    {

       if (currentRoomNodeGraph != null)
        {
            ProcessEvents(Event.current);

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }    
    
    private void ProcessEvents(Event currentEvent)
        {

            //get room node that mouse is currently over
            if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
            {
                currentRoomNode = isMouseOverRoomNode(currentEvent);
            }

            //if mouse isnt over a node
            if (currentRoomNode == null)
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
    }

    private void showContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObject)
    {
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
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in  currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(RoomNodeStyle);
        }
        GUI.changed = true;
    }
}
