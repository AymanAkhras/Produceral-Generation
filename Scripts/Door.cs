using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door
{
    public GameObject doorObj;
    public int x, y;
    public bool connected = false;

    public Door(GameObject door)
    {
        doorObj = door;
        UpdateDoorPos();  
    }

    public void UpdateDoorPos()
    {
        x = Mathf.FloorToInt(doorObj.transform.position.x);
        y = Mathf.FloorToInt(doorObj.transform.position.z); // note unity uses z where you expect y
    }

    public Vector3 AdjacentPosFloat()
    {
        return doorObj.transform.position + doorObj.transform.forward;
    }

    public Vector2Int AdjacentPosInt()
    {
        //int adj_x, adj_y;
        Vector3 adj_pos = AdjacentPosFloat();
        int adj_x = Mathf.FloorToInt(adj_pos.x);
        int adj_y = Mathf.FloorToInt(adj_pos.z); // note unity uses z where you expect y        
        return new Vector2Int(adj_x, adj_y);
    }
}
