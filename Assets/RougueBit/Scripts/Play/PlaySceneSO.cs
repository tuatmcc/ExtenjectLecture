using UnityEngine;

namespace RougueBit.Play
{
    [CreateAssetMenu(fileName = "PlaySceneSettings", menuName = "Scriptable Objects/PlaySceneSettings")]
    public class PlaySceneSO : ScriptableObject
    {
        public int StageWidth => stageWidth;
        public int StageDepth => stageDepth;
        public int StageMinRoomSize => stageMinRoomSize;
        public int StageMaxRoomSize => stageMaxRoomSize;
        public int StageMaxRooms => stageMaxRooms;
        public GameObject WallPrefab => wallPrefab;
        public GameObject FloorPrefab => floorPrefab;

        public float PlayerMoveSpeed => playerMoveSpeed;
        public float PlayerAcceleration => playerMoveAcceleration;
        public float PlayerRotationSpeed => playerRotationSpeed;
        public Vector2 PlayerStartPosition { get => playerStartPosition; set => playerStartPosition = value; }

        [SerializeField] private int stageWidth = 50;
        [SerializeField] private int stageDepth = 50; // Z方向のサイズ
        [SerializeField] private int stageMinRoomSize = 5;
        [SerializeField] private int stageMaxRoomSize = 15;
        [SerializeField] private int stageMaxRooms = 10;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private float playerMoveSpeed = 5f;
        [SerializeField] private float playerMoveAcceleration = 10f;
        [SerializeField] private float playerRotationSpeed = 10f;
        
        private Vector2 playerStartPosition = new(0, 0);
    }
}
