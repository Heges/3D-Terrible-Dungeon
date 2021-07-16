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
        List<List<Coord>> wallRegions;

        public int widthDungeon;
        public int heightDungeon;
        public int numberOfOperations;
        public int roomTreesholdWhatNeedDestroy = 40;
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
            survivingRooms = new List<Room>();

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
            GenerateCorridorInTree(dungeonTree);

            foreach (Room room in survivingRooms)
            {
                if (room.roomSize < roomTreesholdWhatNeedDestroy)
                {
                    foreach (Coord tile in room.tiles)
                    {
                        worldMap[tile.coordTileX, tile.coordTileY] = 1;
                    }
                }
            }
            //wallRegions = GetRegions(1);

            //List <List<Coord>> roomsRegion = GetRegions(0);
            //foreach (var room in roomsRegion)
            //{
            //    if (room.Count < roomTreesholdWhatNeedDestroy)
            //    {
            //        foreach (var tile in room)
            //        {
            //            worldMap[tile.coordTileX, tile.coordTileY] = 1;
            //        }
            //    }
            //    else
            //    {
            //        survivingRooms.Add(new Room(room, worldMap));
            //    }
            //}
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

        private void GenerateCorridorInTree(BspTree tree)
        {
            if (tree.IsInternal())
            {
                Room roomA = tree.left.tilesRoom;
                Room roomB = tree.left.tilesRoom;

                int bestDistance = 0;
                bool isConnected = false;

                if (roomA != null && roomB != null)
                {
                    for (int tilesRoomA = 0; tilesRoomA < roomA.edgeTiles.Count; tilesRoomA++)
                    {
                        for (int tilesRoomB = 0; tilesRoomB < roomB.edgeTiles.Count; tilesRoomB++)
                        {
                            Coord tileA = roomA.edgeTiles[tilesRoomA];
                            Coord tileB = roomB.edgeTiles[tilesRoomB];

                            Vector2 pointA = new Vector2(tileA.coordTileX, tileA.coordTileY);
                            Vector2 pointB = new Vector2(tileB.coordTileX, tileB.coordTileY);

                            var distance = Math.Pow(tileA.coordTileX - tileB.coordTileX, 2) + Math.Pow(tileA.coordTileY - tileB.coordTileY, 2);

                            if (distance < bestDistance)
                            {

                            }
                        }
                    }
                }

                if (tree.left != null)
                {
                    GenerateCorridorInTree(tree.left);
                }
                if (tree.right != null)
                {
                    GenerateCorridorInTree(tree.right);
                }
            }

            if (tree.IsLeaf())
            {
                //room here
            }
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
                List<Coord> tiles = new List<Coord>();
                for (int x = 0; x < tree.room.width; x++)
                {
                    for (int y = 0; y < tree.room.height; y++)
                    {
                        int posX = x + tree.room.x;
                        int posY = y + tree.room.y;

                        if (MapIsInRange(posX,posY))
                        {
                            if (posX > 0 && posX < widthDungeon - 1 && posY > 0 && posY < heightDungeon - 1)
                            {
                                worldMap[posX, posY] = 0;
                                tiles.Add(new Coord(posX, posY));
                            }
                            else
                            {
                                worldMap[posX, posY] = 1;
                            }
                        }
                    }
                }
                Room newRoom = new Room(tiles, worldMap);
                tree.tilesRoom = newRoom;
                survivingRooms.Add(newRoom);
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

        bool MapIsInRange(int x, int y)
        {
            return x >= 0 && x < widthDungeon && y >= 0 && y < heightDungeon;
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                GenerateMap();
            }
        }

        void DrawCircle(Coord c, int r)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int drawX = c.coordTileX + x;
                        int drawY = c.coordTileY + y;
                        if (MapIsInRange(drawX, drawY))
                        {
                            worldMap[drawX, drawY] = 0;
                        }
                    }
                }
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
                //wallRegions = GetRegions(1);

                //if (wallRegions != null)
                //{
                //    foreach (var wallRegion in wallRegions)
                //    {
                //        foreach (var tile in wallRegion)
                //        {
                //            Gizmos.color = Color.black;
                //            Vector3 pos = new Vector3(tile.coordTileX, tile.coordTileY, 0);
                //            Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                //        }
                //    }
                //}

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

