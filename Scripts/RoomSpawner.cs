using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Cell
{
    public Door door = null;
    public Room room = null;
    public int type = 0;        // 0 = empty cell, 1 = room, 2 = door
}

public class RoomSpawner : MonoBehaviour
{

    HashSet<Door> unconnected_doors_set = new HashSet<Door>();
    Queue<Door> unconnected_doors_queue = new Queue<Door>();
    public RoomTypes roomtype;
    public int maxRooms = 20;
    public int gridWidth = 200;
    public int gridHeight = 200;

    public GameObject textOutput;

    int halfWidth;
    int halfHeight;

    private Cell[,] globalGrid; 

    void InitGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                globalGrid[y, x] = new Cell();
            }
        }
    }

    Room SpawnRoom(GameObject prefab, Door target_door = null, int door_index = -1, int start_index = -1)
    // spawns and returns room with a random door connected to target_door, or spawn at origin if no target supplied
    // door_index is the door index for the room to make connection, random if < 0
    // optional start_index param sets first door in the room to check validity, returns null room if checker cycles back to this room.
    {
        if (door_index >= 0 && door_index == start_index) return null;   // base case, full cycle but no valid placement

        Room room = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();
        room.Snap();
        int doorcount = room.doors.Count;

        if (door_index < 0)
        {
            door_index = Random.Range(0, room.doors.Count - 1);
            start_index = door_index;
        }

        if (target_door != null)
        {
            room.AlignDoors(room.doors[door_index], target_door);
            room.doors[door_index].connected = true;

            // if part of the room is outside of map border, redraw
            if (room.bottom + halfHeight < 1 || room.top + halfHeight >= gridHeight-1 || room.left + halfWidth < 1 || room.right + halfWidth >= gridWidth-1) 
            {
                Destroy(room.gameObject);
                return SpawnRoom(prefab, target_door, (door_index + 1) % doorcount, start_index);
            }
            Debug.Log("bottom: " + room.bottom + "  top: " + room.top + "  left: " + room.left + "  right: " + room.right);
            // check if new room has valid space
            for (int y = room.bottom; y < room.top; y++)
            {
                for (int x = room.left; x < room.right; x++)
                {
                    // cell already occupied, cancel room creation
                    Cell cell = globalGrid[y + halfHeight, x + halfWidth];
                    if (cell.type > 0)
                    {
                        if (cell.type == 2) {       // occupied cell is a door, check connectable?
                            bool door_matched = false;
                            // valid placement if a door lines up with target door
                            foreach (Door door in room.doors)
                            {
                                if (door.x == x && door.y == y)
                                {
                                    door.connected = true;
                                    door_matched = true;
                                    break;
                                }
                            }
                            if (door_matched) continue;
                        }
                        Destroy(room.gameObject);
                        return SpawnRoom(prefab, target_door, (door_index + 1) % doorcount, start_index);
                    }
                }
            }

            // Door position checker to make sure doors arent blocked by wall
            foreach (Door door in room.doors)
            {
                if (!door.connected)
                {
                    Vector2Int adj_door_pos = door.AdjacentPosInt();
                    if (globalGrid[adj_door_pos.y + halfHeight, adj_door_pos.x + halfWidth].type == 1)
                    {
                        Destroy(room.gameObject);
                        return SpawnRoom(prefab, target_door, (door_index + 1) % doorcount, start_index);
                    } 
                }
            }
        }

        // valid placement, mark cells as occupied
        for (int y = room.bottom; y < room.top; y++)
        {
            for (int x = room.left; x < room.right; x++)
            {
                globalGrid[y + halfHeight, x + halfWidth].type = 1;
                globalGrid[y + halfHeight, x + halfWidth].room = room;
            }
        }

        // mark door positions on grid
        foreach (Door door in room.doors)
        {
            Vector2Int adj_door_pos = door.AdjacentPosInt();
            if (globalGrid[adj_door_pos.y + halfHeight, adj_door_pos.x + halfWidth].type == 2)
            {
                globalGrid[adj_door_pos.y + halfHeight, adj_door_pos.x + halfWidth].door.connected = true;
                unconnected_doors_set.Remove(globalGrid[adj_door_pos.y + halfHeight, adj_door_pos.x + halfWidth].door);
            } else {
                globalGrid[adj_door_pos.y + halfHeight, adj_door_pos.x + halfWidth].type = 2;
            }
            globalGrid[door.y + halfHeight, door.x + halfWidth].type = 2;
            globalGrid[door.y + halfHeight, door.x + halfWidth].door = door;
        }
        return room;
    }

    void PrintGrid(Cell[,] grid) {
        if (textOutput) {
            string text_content = "Array:\n";
            for (int i = grid.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    text_content += (grid[i,j].type + " ");
                }
                text_content += "\n";
            }
            textOutput.GetComponent<Text>().text = text_content;
        }
    }

    void DeleteAllRooms() {
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("prefabRoom");
        foreach(GameObject room in rooms)
            GameObject.Destroy(room);
        unconnected_doors_queue.Clear();
        unconnected_doors_set.Clear();
    }

    public void GenerateRandomGrid(int roomType = 0)
    {
        DeleteAllRooms();

        globalGrid = new Cell[gridHeight, gridWidth];
        InitGrid();
        
        Room startRoom = SpawnRoom(roomtype.GetRoom(roomType));

        foreach(Door door in startRoom.doors) {
            unconnected_doors_queue.Enqueue(door);
            unconnected_doors_set.Add(door);
        }

        int room_count = 1;

        while (unconnected_doors_set.Count > 0 && room_count < maxRooms)
        {
            Door connection_door = unconnected_doors_queue.Dequeue();
            if (unconnected_doors_set.Contains(connection_door)) {
                unconnected_doors_set.Remove(connection_door);
                Room newRoom = SpawnRoom(roomtype.GetRandomRoom(1), connection_door);
                

                while (newRoom == null && roomtype.previousRoomIndex > 0)
                {
                    newRoom = SpawnRoom(roomtype.GetRoom(roomtype.previousRoomIndex - 1), connection_door);
                }
                if (newRoom != null)
                {
                    room_count++;
                    foreach (Door door in newRoom.doors)
                    {
                        if (!door.connected)
                        {
                            unconnected_doors_queue.Enqueue(door);
                            unconnected_doors_set.Add(door);
                        }
                    }
                }
            }
        }
        Debug.Log(room_count + " rooms generated.");
        PrintGrid(globalGrid);
    }


    void Awake()
    {
        if (gridHeight * gridWidth == 0) {
            Debug.LogError("Grid dimension (height/width) cannot be 0.", this);
        } else if (gridHeight % 2 == 1 || gridWidth % 2 == 1) {
            Debug.LogError("Grid dimension (height/width) must be even.", this);
        }
        halfHeight = gridHeight / 2;
        halfWidth = gridWidth / 2;


    }

    void Start()
    {
        GenerateRandomGrid(9);
    }

}
