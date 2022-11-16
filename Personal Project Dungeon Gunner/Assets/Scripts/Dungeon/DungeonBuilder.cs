using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehavior<DungeonBuilder>
{
    private Dictionary<string, Room> dungeonBuilderRoomDictoinary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictoinary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccesfull;

    protected override void awake()
    {

        base.awake();

        //load the room type list
        LoadRoomTypeList();

        //set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }


    // generate dungeon, returns true if dungeon built, returns false if failed
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // load the scriptable object room templates into the dictoinary
        LoadRoomTemplatesIntoDictoinary();

        dungeonBuildSuccesfull = false;
        int dungeonBuildAttempts = 0;

        while(!dungeonBuildSuccesfull && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            //select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccesfull = false;

            while (!dungeonBuildSuccesfull && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                //clear dungeon room gameObjects and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;
                Debug.Log("how far does code go?");
                //attempt to build a random dungeon for the selected room node graph
                dungeonBuildSuccesfull = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
                if (dungeonBuildSuccesfull)
                {
                    Debug.Log("Dungeon builds atleast");
                    //instantiate room gameobjects
                    InstantiateRoomGameobjects();
                }
            

        }

        return dungeonBuildSuccesfull;
      

    }

    private void LoadRoomTemplatesIntoDictoinary()
    {
        //clear room template dictionary
        roomTemplateDictoinary.Clear();

        //load room template list into dictoinary
        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictoinary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictoinary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template key in " + roomTemplateList);
            }
        }

    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {

        //create open room node queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        //add entrance node to room node queue from room node graph
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.IsEntrance));

        if(entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("no entrance node");
            return false; //dungeon not build
        }

        // start with no room overlaps
        bool noRoomOverlaps = true;

        //process open room nodes queue
        noRoomOverlaps = ProccesRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        if(openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            Debug.LogError("dungeon has not been build");
            return false;
        }
    }

    private bool ProccesRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        //while room nodes in open room node queue & no rooms overlap detected
        while(openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            //get next room node from queue
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            //add child nodes to queue from room node graph 
            foreach(RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            //if the room is the entrance, mark as positioned and add to room dictionary
            if (roomNode.roomNodeType.IsEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                //add room to room dictionary
                dungeonBuilderRoomDictoinary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictoinary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithoutOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithoutOverlaps(RoomNodeSO roomNode, Room parentRoom) 
    {
        //initaite and assume overlap until proven otherwise
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if(unconnectedAvailableDoorways.Count == 0)
            {
                return false;
            }

            Doorway doorwayParent = unconnectedAvailableDoorways[UnityEngine.Random.Range(0, unconnectedAvailableDoorways.Count)];

            RoomTemplateSO roomtemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomtemplate, roomNode);

            if(PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;

                room.isPositioned = true;

                dungeonBuilderRoomDictoinary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        if (roomNode.roomNodeType.IsCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.IsCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.IsCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomtemplate;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        if(doorway == null)
        {
            doorwayParent.isUnavailable = true;

            return false; 
        }

        //calculate world grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        //calculate adjustment position offset based on room doorway position that we are trying to connect
        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        //calculate room lower bounds and upper bound based on positioning to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if(overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;

            return false;
        }

    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach(Doorway doorwayToCheck in doorwayList)
        {
            if(parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictoinary)
        {
            Room room = keyvaluepair.Value;

            // skip if same room as room to test or is not positioned yet
            if(room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            if(IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }
        return null;
    }

    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverLappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);

        bool isOverLappingy = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if(isOverLappingX && isOverLappingy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if(Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // loop trough room template list
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            //add matching room templates
            if(roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        //return null if list is zero
        if(matchingRoomTemplateList.Count == 0)
        {
            return null;
        }
        else
        {
            return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
        }
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach(Doorway doorway in roomDoorwayList)
        {
            if(!doorway.isConnected && !doorway.isConnected)
            {
                yield return doorway;
            }
        }
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        //initialise room from template
        Room room = new Room();

        room.templateId = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if(roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if(roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach(Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);

        }

        return newDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach(string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    private void InstantiateRoomGameobjects()
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictoinary)
        {
            Room room = keyvaluepair.Value;

            //calculate room position
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            //instantiate room
            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            //get instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            //initialise instantiated room
            instantiatedRoom.Initialise(roomGameObject);

            //save gameobject reference
            room.instantiatedRoom = instantiatedRoom;
        }
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if(roomTemplateDictoinary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    public Room GetRoomByRoomID(string roomID)
    {
        if(dungeonBuilderRoomDictoinary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    private void ClearDungeon()
    {
        //destroy instantiated dungeon gameobjects and clear dungeon manager room dictionary
        if(dungeonBuilderRoomDictoinary.Count > 0)
        {
            foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictoinary)
            {
                Room room = keyvaluepair.Value;

                if(room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictoinary.Clear();
        }
    }
}
