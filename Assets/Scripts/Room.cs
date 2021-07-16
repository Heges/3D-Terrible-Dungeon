using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TerribleDungeon
{
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
}
