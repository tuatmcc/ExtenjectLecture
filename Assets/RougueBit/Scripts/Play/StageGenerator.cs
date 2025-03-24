using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;
using System;
using Zenject;
using RougueBit.Play.Interface;

namespace RougueBit.Play
{
    public class StageGenerator: IStageGeneratable
    {
        private readonly int width = 50;
        private readonly int depth = 50; // Z方向のサイズ
        private readonly int minRoomSize = 5;
        private readonly int maxRoomSize = 15;
        private readonly int maxRooms = 10;
        private readonly GameObject wallPrefab;
        private readonly GameObject floorPrefab;

        private List<Rect> rooms;
        private int[,] map;
        private GameObject stageParent; // すべてのオブジェクトの親

        public StageGenerator(PlaySceneSO playSceneSettings)
        {
            width = playSceneSettings.StageWidth;
            depth = playSceneSettings.StageDepth;
            minRoomSize = playSceneSettings.StageMinRoomSize;
            maxRoomSize = playSceneSettings.StageMaxRoomSize;
            maxRooms = playSceneSettings.StageMaxRooms;
            wallPrefab = playSceneSettings.WallPrefab;
            floorPrefab = playSceneSettings.FloorPrefab;
        }

        public void Generate()
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
            ReduceRooms(); // 一定数まで部屋を削除
            FillRooms(); // 部屋を埋める

            // 部屋同士を接続
            ConnectRooms();

            // 外周を確実に壁にする
            EnsureOuterWalls();

            // マップを描画
            DrawMap();
        }

        public Vector2 GetRandomFloor()
        {
            List<Vector2> floorList = new();
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (map[x, z] == 0)
                    {
                        floorList.Add(new Vector2(x, z));
                    }
                }
            }
            return floorList[UnityEngine.Random.Range(0, floorList.Count)];
        }

        private void GenerateDungeon(Rect area)
        {
            if (area.width > maxRoomSize || area.height > maxRoomSize)
            {
                bool splitHorizontally = UnityEngine.Random.value > 0.5f;

                if (splitHorizontally && area.height > maxRoomSize)
                {
                    int split = UnityEngine.Random.Range((int)area.yMin + minRoomSize, (int)area.yMax - minRoomSize);
                    GenerateDungeon(new Rect(area.x, area.y, area.width, split - area.y));
                    GenerateDungeon(new Rect(area.x, split, area.width, area.yMax - split));
                }
                else if (area.width > maxRoomSize)
                {
                    int split = UnityEngine.Random.Range((int)area.xMin + minRoomSize, (int)area.xMax - minRoomSize);
                    GenerateDungeon(new Rect(area.x, area.y, split - area.x, area.height));
                    GenerateDungeon(new Rect(split, area.y, area.xMax - split, area.height));
                }
            }
            else
            {
                int roomWidth = UnityEngine.Random.Range(minRoomSize, (int)area.width);
                int roomHeight = UnityEngine.Random.Range(minRoomSize, (int)area.height);
                int roomX = UnityEngine.Random.Range((int)area.xMin, (int)area.xMax - roomWidth);
                int roomZ = UnityEngine.Random.Range((int)area.yMin, (int)area.yMax - roomHeight);

                Rect room = new Rect(roomX, roomZ, roomWidth, roomHeight);
                rooms.Add(room);
            }
        }

        private void ReduceRooms()
        {
            // 部屋をシャッフル
            rooms = rooms.OrderBy(_ => Guid.NewGuid()).ToList();
            while (rooms.Count > maxRooms)
            {
                rooms.RemoveAt(rooms.Count - 1);
            }
        }

        private void FillRooms()
        {
            foreach (Rect room in rooms)
            {
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

                if (UnityEngine.Random.value > 0.5f)
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
                        tile = GameObject.Instantiate(wallPrefab);
                        tile.transform.position = new Vector3(x, 0.5f, z);
                        tile.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        // 床
                        tile = GameObject.Instantiate(floorPrefab);
                        tile.transform.position = new Vector3(x, 0.01f, z);
                        tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                    }

                    // すべてのオブジェクトを "Stage" の子にする
                    tile.transform.SetParent(stageParent.transform);
                }
            }
        }
    }
}
