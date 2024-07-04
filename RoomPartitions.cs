using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPartitions : MonoBehaviour
{
    public int chunk_size_x = 22;
    public int chunk_size_y = 22;
    int CHUNK_SIZE_X, CHUNK_SIZE_Y;

    [Range(0.1f, 1f)]
    public float division_amount = 0.1f;

    [Header("Controlls for testing")]
    public bool ADD_PARTITION_KEYCODE_SPACE;
    public bool BUILD_PARTITIONS_KEYCODE_P;
    public bool RESET_KEYCODE_R;

    //Data
    GameObject[] tiles;
    List<MaterialPropertyBlock> mprops = new();
    List<GameObject> room_collection = new();
    private int[] row = new int[0];
    private int room_index = 0;
    private int room_count_old = -1;
    private int y_stock = 0;
    private int tile_stock = 0;

    void Start()
    {
        Initialize();
    }
    void Generate_Chunk()
    {
        if (room_count_old > 0) { ResetChunk(); }

        int tile_count = CHUNK_SIZE_X * CHUNK_SIZE_Y;
        //Generate partitions until no more tiles are left.
        while (tile_stock < tile_count)
        {
            Generate_Room_Partition();
        }
        //Store new partition amount if its greater then last one, so we can reuse objects/properties.
        if (room_count_old < room_index) { room_count_old = room_index; };
    }
    void New_Partition(List<GameObject> tile_list)
    {
        GameObject room;
        MaterialPropertyBlock propertyBlock;
        //Only instantiate new objects if we generate more rooms than last time!
        if (room_count_old <= room_index)
        {
            //New room as new Object.
            room = new() { name = "room_" + room_index };
            room_collection.Add(room);
            //New random color.
            Color room_color = Random.ColorHSV();
            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", room_color);
            mprops.Add(propertyBlock);
        }
        else
        {
            //Using old objects/colors.
            room = room_collection[room_index];
            propertyBlock = mprops[room_index];
            room.SetActive(true);
        }
        
        room.transform.position = tile_list[0].transform.position;
        room.transform.SetParent(transform);

        //Modify room tiles (from stored list).
        for (int i = 0; i < tile_list.Count; i++)
        {
            tile_list[i].transform.SetParent(room.transform);
            tile_list[i].GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
        }

        room_index++;
    }
    private void Update()
    {
        //Controlls for testing...
        //Partition step.
        if (Input.GetKeyDown(KeyCode.Space) || ADD_PARTITION_KEYCODE_SPACE) { Generate_Room_Partition(); ADD_PARTITION_KEYCODE_SPACE = false; }
        //Partition complete.
        if (Input.GetKeyDown(KeyCode.P) || BUILD_PARTITIONS_KEYCODE_P) { Generate_Chunk(); BUILD_PARTITIONS_KEYCODE_P = false; }
        //Reset chunk.
        if (Input.GetKeyDown(KeyCode.R) || RESET_KEYCODE_R) { ResetChunk(); RESET_KEYCODE_R = false; }
    }
    private void ResetChunk()
    {
        y_stock = 0;
        for (int i = 0; i < row.Length; i++)
        {
            row[i] = 0;
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            //material.SetColor("_Color", Color.gray)
            tiles[i].GetComponent<MeshRenderer>().SetPropertyBlock(mprops[0]);
            tiles[i].name = "t";
            tiles[i].transform.SetParent(transform);
        }
        for (int i = 0; i < room_collection.Count; i++)
        {
            room_collection[i].SetActive(false);
        }
        room_index = 0;
        tile_stock = 0;
    }

    void Initialize()
    {
        CHUNK_SIZE_X = chunk_size_x;
        CHUNK_SIZE_Y = chunk_size_y;
        //Start chunk.
        row = new int[CHUNK_SIZE_Y];
        tiles = new GameObject[CHUNK_SIZE_X * CHUNK_SIZE_Y];
        //Create tiles.
        int i = 0;
        for (int x = 0; x < CHUNK_SIZE_X; x++) 
        {
            for (int y = 0; y < CHUNK_SIZE_Y; y++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "t";
                quad.transform.position = transform.position + new Vector3( x, 0, y);
                quad.transform.eulerAngles = new Vector3( 90, 0, 0);
                quad.transform.SetParent(transform);
                tiles[i] = quad.gameObject;
                i++;
            }
        }
    }

    void Generate_Room_Partition()
    {
        //Collect tiles.
        List<GameObject> tile_list = new List<GameObject>();

        //Chunk size.
        int tile_count = CHUNK_SIZE_X * CHUNK_SIZE_Y;

        //Limit values.
        int max_amount_x = CHUNK_SIZE_X;
        int max_amount_y = CHUNK_SIZE_Y;
        int max_range_x = CHUNK_SIZE_X + 1;
        int max_range_y = CHUNK_SIZE_Y + 1;

        //Max % size ( x, y) of partitions relative to chunk size.
        int max_segment_y = Mathf.FloorToInt(division_amount * max_amount_y);
        int max_segment_x = Mathf.FloorToInt(division_amount * max_amount_x);

        //Partition start.
        int tile_counting = 0;
        if (tile_stock < tile_count)
        {
            //Define Y range
            int Y_LIMIT = max_segment_y;
            int y_range = Random.Range(1, max_range_y - Y_LIMIT);
            //Clamp the Y range relative to how much space is left.
            int sum_y = y_range + y_stock;
            if (sum_y > max_amount_y) { y_range -= (sum_y - max_amount_y); }

            //Define X range
            int X_LIMIT = max_segment_x;
            int x_range = Random.Range(3, max_range_x - X_LIMIT);

            //Check spaces.
            for (int y = 0; y < y_range; y++)
            {
                int row_space = row[y + y_stock];

                //Clamp the X range relative to how much space is left.
                int sum_x = x_range + row_space;
                if (sum_x > max_amount_x) { x_range -= (sum_x - max_amount_x); }

                //Has the row space?
                if (row_space < max_amount_x)
                {
                    for (int x = 0; x < x_range; x++)
                    {
                        //Get tile with index value.
                        GameObject t = tiles[(y_stock + y + max_amount_y * x) + (max_amount_y * row_space)];

                        //Don't overwrite.
                        if (t.name == "t")
                        {
                            tile_list.Add(t);
                            tile_stock++;
                            tile_counting++;
                            t.name = "room_" + room_index;
                        }
                    }
                    //Update row space.
                    row[y + y_stock] += x_range;
                }
            }

            //New partition created. (We listed tiles.)
            if (tile_counting > 0)
            {
                New_Partition(tile_list);
            }

            //Update y_offset
            y_stock += y_range;

            //Column Limit is reached. Reset Y offset(y_stock).
            if (y_stock >= max_amount_y)
            {
                //Debug.Log("Column limit reached!");
                y_stock = 0;

                //Check for next possible column space for the next iteration.
                for (int i = 0; i < row.Length; i++)
                {
                    if (row[i] < max_amount_x)
                    {
                        //Debug.Log("Column start at: " + i);
                        y_stock = i;
                        break;
                    }
                }
            }
        }
        else
        {
            //No more tiles are left.
            Debug.Log("Chunk is done!");
        }
    }
}
