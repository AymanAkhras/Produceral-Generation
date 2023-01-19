using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTypes : MonoBehaviour
{
    public int previousRoomIndex = 0;

    public GameObject[] room_types;

    public GameObject GetRoom(int i)
    {
        previousRoomIndex = i;
        return room_types[i];
    }

    public GameObject GetRandomRoom(int low = 0)
        // returns a random room, optionally excluding room indexes < low (to prevent spawning single exit rooms if necessary)
    {
        return GetRoom(Random.Range(low, room_types.Length));
    }
}
