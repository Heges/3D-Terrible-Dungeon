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
        public int wallsTreesholdWhatNeedDestroy = 40;
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
            //GenerateCorridorBetweenLeafs(dungeonTree);
            GenerateCorridorsNode(dungeonTree);

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
            survivingRooms.Sort();

            wallRegions = GetRegions(1);
            foreach (var wallRegion in wallRegions)
            {
                if (wallRegion.Count < wallsTreesholdWhatNeedDestroy)
                {
                    foreach (var tile in wallRegion)
                    {
                        worldMap[tile.coordTileX, tile.coordTileY] = 0;
                    }
                }
            }

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

        private void GenerateCorridorsNode(BspTree node)
        {
            if (node.IsInternal())
            {
                RectInt leftContainer = node.left.container;
                RectInt rightContainer = node.right.container;

                Vector2 leftCenter = leftContainer.center;
                Vector2 rightCenter = rightContainer.center;

                Vector2 direction = (rightCenter - leftCenter).normalized;
                //List<Coord> newList = new List<Coord>();

                while (Vector2.Distance(leftCenter, rightCenter) > 1) // 1f
                {
                    if (direction.Equals(Vector2.right))
                    {
                        int a = UnityEngine.Random.Range(0, 101);

                        for (int i = 0; i < 4; i++)
                        {
                            if (a < 81)
                            {
                                DrawRectangle(new Coord((int)leftCenter.x + i, (int)leftCenter.y), 6, 6);
                            }
                            else
                            {
                                DrawCircle(new Coord((int)leftCenter.x + i, (int)leftCenter.y), 4);
                            }
                        }
                        
                    }
                    if (direction.Equals(Vector2.up))
                    {
                        int a = UnityEngine.Random.Range(0, 101);

                        for (int i = 0; i < 4; i++)
                        {
                            if (a < 81)
                            {
                                DrawRectangle(new Coord((int)leftCenter.x + i, (int)leftCenter.y), 6, 6);
                            }
                            else
                            {
                                DrawCircle(new Coord((int)leftCenter.x, (int)leftCenter.y + i), 3);
                            }
                        }
                    }
                    leftCenter.x += direction.x; // direction normalized 
                    leftCenter.y += direction.y; // direction normalized 
                }
                if (node.left != null) GenerateCorridorsNode(node.left);
                if (node.right != null) GenerateCorridorsNode(node.right);
            }
        }

        private void GenerateCorridorBetweenLeafs(BspTree tree)
        {
            if (tree.IsInternal())
            {
                Room roomAtiles = tree.left.tilesRoom;
                Room roomBtiles = tree.right.tilesRoom;
                RectInt roomA = tree.left.room;
                RectInt roomB = tree.right.room;
                int bestDistance = int.MaxValue;
                Coord bestTileA = new Coord(0, 0);
                Coord bestTileB = new Coord(0, 0);

                if (roomA.width != 0 && roomB.width != 0)
                {
                    for (int tileInEdgeTilesRoomA = 0; tileInEdgeTilesRoomA < roomAtiles.edgeTiles.Count; tileInEdgeTilesRoomA++)
                    {
                        for (int tileInEdgeTilesRoomB = 0; tileInEdgeTilesRoomB < roomBtiles.edgeTiles.Count; tileInEdgeTilesRoomB++)
                        {
                            int w = roomAtiles.edgeTiles[tileInEdgeTilesRoomA].coordTileX - roomBtiles.edgeTiles[tileInEdgeTilesRoomB].coordTileX;
                            int h = roomAtiles.edgeTiles[tileInEdgeTilesRoomA].coordTileY - roomBtiles.edgeTiles[tileInEdgeTilesRoomB].coordTileY;
                            int distanceBetweenPoints = (int)(Mathf.Pow(w, 2) + Mathf.Pow(h, 2));
                            if (distanceBetweenPoints < bestDistance)
                            {
                                bestDistance = distanceBetweenPoints;
                                bestTileA = roomAtiles.edgeTiles[tileInEdgeTilesRoomA];
                                bestTileB = roomBtiles.edgeTiles[tileInEdgeTilesRoomB];
                            }
                        }
                    }
                }
                GoCreatePassage(bestTileA, bestTileB);

                if (tree.left != null)
                {
                    GenerateCorridorBetweenLeafs(tree.left);
                }
                if (tree.right != null)
                {
                    GenerateCorridorBetweenLeafs(tree.right);
                }
            }

            if (tree.IsLeaf())
            {
                //room here
            }
        }

        private void GoCreatePassage(Coord tileA, Coord tileB)
        {
            //Debug.Log("GoCreatePassage" + tileA.coordTileX + ":" + tileA.coordTileY);
            //Debug.Log("GoCreatePassage" + tileB.coordTileX + ":" + tileB.coordTileY);
            List<Coord> line = GetLine(tileA, tileB);
            foreach (var tile in line)
            {
                int a = UnityEngine.Random.Range(0, 101);
                if (a < 78)
                {
                    DrawRectangle(tile, 6, 6);

                }
                else
                {
                    DrawCircle(tile, 2);
                }
                

            }
        }

        private List<Coord> GetLine(Coord from, Coord to)
        {
            
            List<Coord> line = new List<Coord>();

            int x = from.coordTileX;
            int y = from.coordTileY;

            //if (from.coordTileX == to.coordTileX && from.coordTileY == to.coordTileY)
            //{
            //    line.Add(new Coord(x, y));
            //    return line;
            //}

            int dx = to.coordTileX - from.coordTileX;
            int dy = to.coordTileY - from.coordTileY;

            int step = (int)Mathf.Sign(dx);
            int gradientStep = (int)Mathf.Sign(dy);

            bool isInverted = false;

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest)
            {
                isInverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = (int)Mathf.Sign(dy);
                gradientStep = (int)Mathf.Sign(dx);
            }
            int gradientAccumulation = longest / 2;

            for (int i = 0; i < longest; i++)
            {
                line.Add(new Coord(x, y));

                if (isInverted)
                {
                    y += step;
                }
                else
                {
                    x += step;
                }
                gradientAccumulation += shortest;
                if (gradientAccumulation >= shortest)
                {
                    if (isInverted)
                    {
                        y += gradientStep;
                    }
                    else
                    {
                        x += gradientStep;
                    }
                    gradientAccumulation -= longest;
                }
            }
            return line;
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

        void DrawRectangle(Coord c, int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    int drawX = c.coordTileX + i;
                    int drawY = c.coordTileY + j;
                    if (MapIsInRange(drawX, drawY))
                    {
                        worldMap[drawX, drawY] = 0;
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

