using RougueBit.Play.Interface;
using UnityEngine;

namespace RougueBit.Play.Tests
{
    public class TestStageGenerator : IStageGeneratable
    {
        private readonly int width = 50;
        private readonly int depth = 50; // Z方向のサイズ
        private readonly GameObject floorPrefab;

        public TestStageGenerator(PlaySceneSO playSceneSO)
        {
            width = playSceneSO.StageWidth;
            depth = playSceneSO.StageDepth;
            floorPrefab = playSceneSO.FloorPrefab;
        }

        public void Generate()
        {

            var stageParent = new GameObject("Stage");
            DrawMap(stageParent);
        }

        public Vector2 GetRandomFloor()
        {
            return new Vector2(width / 2, depth / 2);
        }

        private void DrawMap(GameObject stageParent)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // 床
                    var tile = GameObject.Instantiate(floorPrefab);
                    tile.transform.position = new Vector3(x, 0.01f, z);
                    tile.transform.rotation = Quaternion.Euler(90, 0, 0);

                    // すべてのオブジェクトを "Stage" の子にする
                    tile.transform.SetParent(stageParent.transform);
                }
            }
        }
    }
}