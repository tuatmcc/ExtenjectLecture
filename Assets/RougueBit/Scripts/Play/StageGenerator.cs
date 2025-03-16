using UnityEngine;
using System.Collections.Generic;

public class BSPDungeon : MonoBehaviour
{
    public int width = 50;
    public int depth = 50; // Z方向のサイズ
    public int minRoomSize = 5;
    public int maxRoomSize = 15;

    private List<Rect> rooms;
    private int[,] map;
    private GameObject stageParent; // すべてのオブジェクトの親

    void Start()
    {
        rooms = new List<Rect>();
        map = new int[width, depth];

        // 親オブジェクトを作成
        stageParent = new GameObject("Stage");

        // マップを壁で埋める（1が壁, 0が床）
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                map[x, z] = 1; // 初期状態は壁
            }
        }

        // BSPでダンジョンを生成
        GenerateDungeon(new Rect(1, 1, width - 2, depth - 2)); // 外周は壁にする

        // 部屋同士を接続
        ConnectRooms();

        // 外周を確実に壁にする
        EnsureOuterWalls();

        // マップを描画
        DrawMap();
    }

    private void GenerateDungeon(Rect area)
    {
        if (area.width > maxRoomSize || area.height > maxRoomSize)
        {
            bool splitHorizontally = Random.value > 0.5f;

            if (splitHorizontally && area.height > maxRoomSize)
            {
                int split = Random.Range((int)area.yMin + minRoomSize, (int)area.yMax - minRoomSize);
                GenerateDungeon(new Rect(area.x, area.y, area.width, split - area.y));
                GenerateDungeon(new Rect(area.x, split, area.width, area.yMax - split));
            }
            else if (area.width > maxRoomSize)
            {
                int split = Random.Range((int)area.xMin + minRoomSize, (int)area.xMax - minRoomSize);
                GenerateDungeon(new Rect(area.x, area.y, split - area.x, area.height));
                GenerateDungeon(new Rect(split, area.y, area.xMax - split, area.height));
            }
        }
        else
        {
            int roomWidth = Random.Range(minRoomSize, (int)area.width);
            int roomHeight = Random.Range(minRoomSize, (int)area.height);
            int roomX = Random.Range((int)area.xMin, (int)area.xMax - roomWidth);
            int roomZ = Random.Range((int)area.yMin, (int)area.yMax - roomHeight);

            Rect room = new Rect(roomX, roomZ, roomWidth, roomHeight);
            rooms.Add(room);
            FillRoom(room);
        }
    }

    private void FillRoom(Rect room)
    {
        for (int x = (int)room.xMin; x < (int)room.xMax; x++)
        {
            for (int z = (int)room.yMin; z < (int)room.yMax; z++)
            {
                map[x, z] = 0; // 0は床
            }
        }
    }

    private void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2 roomCenterA = new(rooms[i].center.x, rooms[i].center.y);
            Vector2 roomCenterB = new(rooms[i + 1].center.x, rooms[i + 1].center.y);

            if (Random.value > 0.5f)
            {
                CreateHorizontalCorridor((int)roomCenterA.x, (int)roomCenterB.x, (int)roomCenterA.y);
                CreateVerticalCorridor((int)roomCenterA.y, (int)roomCenterB.y, (int)roomCenterB.x);
            }
            else
            {
                CreateVerticalCorridor((int)roomCenterA.y, (int)roomCenterB.y, (int)roomCenterA.x);
                CreateHorizontalCorridor((int)roomCenterA.x, (int)roomCenterB.x, (int)roomCenterB.y);
            }
        }
    }

    private void CreateHorizontalCorridor(int xStart, int xEnd, int z)
    {
        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            map[x, z] = 0;
        }
    }

    private void CreateVerticalCorridor(int zStart, int zEnd, int x)
    {
        for (int z = Mathf.Min(zStart, zEnd); z <= Mathf.Max(zStart, zEnd); z++)
        {
            map[x, z] = 0;
        }
    }

    // 外周を壁にする
    private void EnsureOuterWalls()
    {
        for (int x = 0; x < width; x++)
        {
            map[x, 0] = 1;
            map[x, depth - 1] = 1;
        }
        for (int z = 0; z < depth; z++)
        {
            map[0, z] = 1;
            map[width - 1, z] = 1;
        }
    }

    private void DrawMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                GameObject tile;
                if (map[x, z] == 1)
                {
                    // 壁
                    tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.transform.position = new Vector3(x, 0.5f, z);
                    tile.transform.localScale = new Vector3(1, 1, 1);
                    tile.GetComponent<Renderer>().material.color = Color.black;
                }
                else
                {
                    // 床
                    tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.transform.position = new Vector3(x, 0.01f, z);
                    tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                    tile.GetComponent<Renderer>().material.color = Color.white;
                }

                // すべてのオブジェクトを "Stage" の子にする
                tile.transform.SetParent(stageParent.transform);
            }
        }
    }
}
