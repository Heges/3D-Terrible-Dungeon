using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerribleDungeon
{
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
}
