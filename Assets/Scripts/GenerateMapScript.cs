using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TerribleDungeon
{
    public class GenerateMapScript : MonoBehaviour
    {
        List<BspTree> allLevels;

        BspTree dungeonTree;

        int[,] worldMap;
        int[,] borderedMap;
        List<Room> survivingRooms;

        public int widthDungeon;
        public int heightDungeon;
        public int numberOfOperations;
        public bool shouldDrawOnlyCubes;
        public bool shouldDrawOnlyWorldMap;
        public bool shouldDrawOnlyRooms;

        void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            
            worldMap = new int[widthDungeon, heightDungeon];
            for (int x = 0; x < worldMap.GetLength(0); x++)
            {
                for (int y = 0; y < worldMap.GetLength(1); y++)
                {
                    worldMap[x, y] = 1;
                }
            }

            RectInt dungeonRect = new RectInt(0, 0, widthDungeon, heightDungeon);
            dungeonTree = BspTree.Split(numberOfOperations, dungeonRect);
            BspTree.GenerateRoomInsideContainersNode(dungeonTree);
            GenerateArrayOfMap(dungeonTree);

            List <List<Coord>> roomsRegion = GetRegions(0);
            int roomTreesholdWhatNeedDestroy = 20;
            survivingRooms = new List<Room>();

            foreach (var room in roomsRegion)
            {
                if (room.Count < roomTreesholdWhatNeedDestroy)
                {
                    foreach (var tile in room)
                    {
                        worldMap[tile.coordTileX, tile.coordTileY] = 1;
                    }
                }
                else
                {
                    survivingRooms.Add(new Room(room, worldMap));
                }
            }
            survivingRooms.Sort();

            int borderSize = 10;
            borderedMap = new int[widthDungeon + borderSize * 2, heightDungeon + borderSize * 2];
            for (int x = 0; x < borderedMap.GetLength(0); x++)
            {
                for (int y = 0; y < borderedMap.GetLength(1); y++)
                {
                    if (x >= borderSize && x < widthDungeon + borderSize && y >= borderSize && y < heightDungeon + borderSize)
                    {
                        borderedMap[x, y] = worldMap[x - borderSize, y - borderSize];
                    }
                    else
                    {
                        borderedMap[x, y] = 1;
                    }
                }
            }

            GenerateMesh meshGenerator = GetComponent<GenerateMesh>();
            meshGenerator.GenerateMeshFromMap(borderedMap, 1f);
        }

        private void GenerateArrayOfMap(BspTree tree)
        {
            if (tree.IsInternal())
            {
                if (tree.left != null)
                {
                    GenerateArrayOfMap(tree.left);
                }
                if (tree.right != null)
                {
                    GenerateArrayOfMap(tree.right);
                }
            }else if(tree.IsLeaf())
            {
                for (int x = 0; x < tree.room.width; x++)
                {
                    for (int y = 0; y < tree.room.height; y++)
                    {
                        int posX = x + tree.room.x;
                        int posY = y + tree.room.y;

                        if (MapIsInRange(posX,posY))// posX >= 0 && posX < widthDungeon && posY >= 0 && posY < heightDungeon)
                        {
                            if (posX > 0 && posX < widthDungeon - 1 && posY > 0 && posY < heightDungeon - 1)
                            {
                                worldMap[posX, posY] = 0;
                            }
                            else
                            {
                                worldMap[posX, posY] = 1;
                            }
                            //if (x >= 0 && y >= 0 && x < tree.room.width && y < tree.room.height  )
                            //{
                            //    worldMap[posX, posY] = 0;
                            //}
                        }
                    }
                }
            }
        }

        List<List<Coord>> GetRegions(int tileType)
        {
            int[,] mapFlags = new int[widthDungeon, heightDungeon];
            List<List<Coord>> regions = new List<List<Coord>>();
            for (int x = 0; x < widthDungeon; x++)
            {
                for (int y = 0; y < heightDungeon; y++)
                {
                    if (mapFlags[x,y] == 0 && worldMap[x,y] == tileType)
                    {
                        List<Coord> newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        foreach (var tile in newRegion)
                        {
                            mapFlags[tile.coordTileX, tile.coordTileY] = 1;
                        }
                    }
                }
            }
            return regions;
        }

        List<Coord> GetRegionTiles(int startX, int startY)
        {
            List<Coord> tiles = new List<Coord>();
            int tileType = worldMap[startX, startY];
            int[,] mapFlags = new int[widthDungeon, heightDungeon];

            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;

            while (queue.Count > 0)
            {
                Coord tile = queue.Dequeue();
                tiles.Add(tile);

                for (int x = tile.coordTileX - 1; x <= tile.coordTileX + 1; x++)
                {
                    for (int y = tile.coordTileY - 1; y <= tile.coordTileY + 1; y++)
                    {
                        if (MapIsInRange(x,y) && (x == tile.coordTileX || y == tile.coordTileY))
                        {
                            if (mapFlags[x,y] == 0 && worldMap[x,y] == tileType)
                            {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
                    }
                }
            }
            return tiles;
        }

        public struct Coord
        {
            public int coordTileX;
            public int coordTileY;

            public Coord(int a, int b)
            {
                coordTileX = a;
                coordTileY = b;
            }
        }

        bool MapIsInRange(int x, int y)
        {
            return x >= 0 && x < widthDungeon && y >= 0 && y < heightDungeon;
        }

        public class Room : IComparable<Room>
        {
            public List<Coord> tiles;
            public List<Coord> edgeTiles;

            public int roomSize;
            public bool disabled;

            public Room()
            {

            }

            public Room(List<Coord> tileList, int[,] map)
            {
                edgeTiles = new List<Coord>();
                tiles = tileList;
                roomSize = tiles.Count;

                foreach (var tile in tiles)
                {
                    for (int x = tile.coordTileX - 1; x <= tile.coordTileX + 1; x++)
                    {
                        for (int y = tile.coordTileY - 1; y <= tile.coordTileY + 1; y++)
                        {
                            if (x == tile.coordTileX || y == tile.coordTileY)
                            {
                                if (map[x, y] == 1)
                                {
                                    edgeTiles.Add(tile);
                                }
                            }
                        }
                    }
                }
            }

            public int CompareTo(Room otherRoom)
            {
                return otherRoom.roomSize.CompareTo(roomSize);
            }
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                GenerateMap();
            }
        }

        private void OnDrawGizmos()
        {
            if (shouldDrawOnlyCubes)
            {
                if (borderedMap != null)
                {
                    for (int x = 0; x < borderedMap.GetLength(0); x++)
                    {
                        for (int y = 0; y < borderedMap.GetLength(1); y++)
                        {
                            Gizmos.color = borderedMap[x, y] == 0 ? Color.white : Color.black;
                            Vector3 pos = new Vector3(x, y, 0);
                            Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                        }
                    }
                }
            }
            if (shouldDrawOnlyWorldMap)
            {
                if (worldMap != null)
                {
                    for (int x = 0; x < worldMap.GetLength(0); x++)
                    {
                        for (int y = 0; y < worldMap.GetLength(1); y++)
                        {
                            Gizmos.color = worldMap[x, y] == 0 ? Color.white : Color.black;
                            Vector3 pos = new Vector3(x, y, 0);
                            Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                        }
                    }
                }
            }

            if (shouldDrawOnlyRooms)
            {
                if (survivingRooms != null)
                {
                    foreach (var room in survivingRooms)
                    {
                        foreach (var tile in room.tiles)
                        {
                            Gizmos.color = Color.white;
                            Vector3 pos = new Vector3(tile.coordTileX, tile.coordTileY, 0);
                            Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                        }
                        foreach (var tile in room.edgeTiles)
                        {
                            Gizmos.color = Color.black;
                            Vector3 pos = new Vector3(tile.coordTileX, tile.coordTileY, 0);
                            Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                        }
                    }
                }
            }

        }
    }

}

