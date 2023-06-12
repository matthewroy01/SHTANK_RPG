using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKDungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Floor Space")]
        public int floorSize;

        [Header("Rooms")]
        public int roomNum;
        public int roomSizeMin;
        public int roomSizeMax;
        public int roomDistanceMin;

        [Header("Prefabs")]
        public GameObject prefabFloor;
        public GameObject prefabWall;

        [Header("Enemies")]
        public Transform overworldEnemyParent;
        public OverworldEnemyController overworldEnemyPrefab;
        public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();

        public GameObject[,] grid;
        private List<GameObject> walls = new List<GameObject>();
        private List<Vector2Int> roomPositions = new List<Vector2Int>();

        private OverworldPlayerController player;

        private void Awake()
        {
            player = FindObjectOfType<OverworldPlayerController>();

            Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                walls.Clear();
                roomPositions.Clear();

                Generate();
            }
        }

        public void Generate()
        {
            grid = new GameObject[floorSize, floorSize];

            // create rooms
            CreateRooms();

            // create extras in rooms
            CreateExtras();

            // create connecting paths
            CreatePaths();

            // create walls
            CreateWalls();

            // place characters
            PlaceCharacters();
        }

        private void CreateRooms()
        {
            int x, y;

            for (int i = 0; i < roomNum; ++i)
            {
                int counter = 0;

                // check if the new room is too close to other rooms
                do
                {
                    x = Random.Range(0, floorSize);
                    y = Random.Range(0, floorSize);

                    counter++;

                    // if RNG is being bad or if the minimum room distance is too large, break here as a failsafe
                    if (counter > floorSize)
                    {
                        break;
                    }
                }
                while (CheckRoomDistance(x, y));

                CreateRoom(x, y);
            }
        }

        private void CreateRoom(int centerX, int centerY)
        {
            int width, height;

            width = Random.Range(roomSizeMin, roomSizeMax);
            height = Random.Range(roomSizeMin, roomSizeMax);

            int startingX = centerX - (int)(width * 0.5f);
            int startingY = centerY - (int)(height * 0.5f);

            int roomPosX = centerX;
            int roomPosY = centerY;

            if (roomPosX < 2)
            {
                roomPosX = 2;
            }
            else if (roomPosX > floorSize - 2)
            {
                roomPosX = floorSize - 2;
            }

            if (roomPosY < 2)
            {
                roomPosY = 2;
            }
            else if (roomPosY > floorSize - 2)
            {
                roomPosY = floorSize - 2;
            }

            roomPositions.Add(new Vector2Int(roomPosX, roomPosY));

            for (int i = startingX; i < startingX + width; ++i)
            {
                for (int j = startingY; j < startingY + height; ++j)
                {
                    // instantiate a floor tile
                    TryCreateObject(i, j, 1, -0.5f, prefabFloor, null, 0);
                }
            }
        }

        private void CreatePaths()
        {
            List<Vector2Int> pairs = new List<Vector2Int>();

            for (int i = 0; i < roomPositions.Count; ++i)
            {
                for (int j = 0; j < roomPositions.Count; ++j)
                {
                    if (i != j && !pairs.Contains(new Vector2Int(i, j)))
                    {
                        pairs.Add(new Vector2Int(j, i));

                        int y = roomPositions[i].y;

                        while (y != roomPositions[j].y)
                        {
                            // instantiate a floor tile
                            TryCreateObject(roomPositions[i].x, y, 1, -0.5f, prefabFloor, null, 1);

                            if (roomPositions[i].y < roomPositions[j].y)
                            {
                                y++;
                            }
                            else
                            {
                                y--;
                            }
                        }

                        int x = roomPositions[i].x;

                        while (x != roomPositions[j].x)
                        {
                            // instantiate a floor tile
                            TryCreateObject(x, roomPositions[j].y, 1, -0.5f, prefabFloor, null, 1);

                            if (roomPositions[i].x < roomPositions[j].x)
                            {
                                x++;
                            }
                            else
                            {
                                x--;
                            }
                        }
                    }
                }
            }
        }

        private void CreateWalls()
        {
            for (int i = 0; i < floorSize; ++i)
            {
                for (int j = 0; j < floorSize; ++j)
                {
                    if (grid[i, j] == null)
                    {
                        if (CheckShouldBeWall(i, j) == true)
                        {
                            TryCreateObject(i, j, 0, 0.5f, prefabWall, walls, 0, true);
                        }
                    }
                }
            }
        }

        private bool CheckRoomDistance(int x, int y)
        {
            foreach(Vector2Int pos in roomPositions)
            {
                if (Vector2Int.Distance(pos, new Vector2Int(x, y)) > roomDistanceMin)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckShouldBeWall(int x, int y)
        {
            if (x != floorSize - 1 && (grid[x + 1, y] != null && !walls.Contains(grid[x + 1, y])))
            {
                return true;
            }

            if (x != 0 && (grid[x - 1, y] != null && !walls.Contains(grid[x - 1, y])))
            {
                return true;
            }

            if (y != floorSize - 1 && (grid[x, y + 1] != null && !walls.Contains(grid[x, y + 1])))
            {
                return true;
            }

            if (y != 0 && (grid[x, y - 1] != null && !walls.Contains(grid[x, y - 1])))
            {
                return true;
            }

            return false;
        }

        private void TryCreateObject(int x, int y, int bound, float yOffset, GameObject prefab, List<GameObject> toAddTo = null, int extras = 0, bool mustReplace = false)
        {
            GameObject tmp = null;

            if (x > bound && x < floorSize - bound && y > bound && y < floorSize - bound)
            {
                if (grid[x, y] == null)
                {
                    tmp = Instantiate(prefab, new Vector3(x, yOffset, y), Quaternion.identity, transform);
                    if (toAddTo != null)
                    {
                        toAddTo.Add(tmp);
                    }

                    grid[x, y] = tmp;
                }
                else if (mustReplace)
                {
                    Destroy(grid[x, y]);

                    tmp = Instantiate(prefab, new Vector3(x, yOffset, y), Quaternion.identity, transform);
                    if (toAddTo != null)
                    {
                        toAddTo.Add(tmp);
                    }

                    grid[x, y] = tmp;
                }
            }

            if (extras > 0)
            {
                extras--;
                TryCreateObject(x + 1, y, bound, yOffset, prefab, toAddTo, extras, mustReplace);
                TryCreateObject(x - 1, y, bound, yOffset, prefab, toAddTo, extras, mustReplace);
                TryCreateObject(x, y - 1, bound, yOffset, prefab, toAddTo, extras, mustReplace);
                TryCreateObject(x, y + 1, bound, yOffset, prefab, toAddTo, extras, mustReplace);
            }
        }

        private void PlaceCharacters()
        {
            if (roomPositions.Count > 0)
            {
                player.transform.position = new Vector3(roomPositions[0].x, 1.0f, roomPositions[0].y);
            }
            
            if (roomPositions.Count > 1)
            {
                for (int i = 1; i < roomPositions.Count; ++i)
                {
                    OverworldEnemyController tmp = Instantiate(overworldEnemyPrefab, new Vector3(roomPositions[i].x, 1.0f, roomPositions[i].y), overworldEnemyPrefab.transform.rotation, transform);
                    tmp.toSpawn = enemyGroups[Random.Range(0, enemyGroups.Count)];
                }
            }
        }

        private void CreateExtras()
        {
            for (int i = 0; i < roomPositions.Count; ++i)
            {
                int numExtras = Random.Range(3, 7);

                int roomSizeAverage = roomSizeMin/*(int)((roomSizeMin + roomSizeMax) * 0.5f)*/;

                for (int j = 0; j < numExtras; ++j)
                {
                    int x = Random.Range(0, roomSizeAverage) - (int)(roomSizeAverage * 0.5f);
                    int y = Random.Range(0, roomSizeAverage) - (int)(roomSizeAverage * 0.5f);

                    TryCreateObject(roomPositions[i].x + x, roomPositions[i].y + y, 1, 0.5f, prefabWall, walls, 0, true);
                }
            }
        }
    }
}