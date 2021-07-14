using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerribleDungeon
{
    public class GenerateMapScript : MonoBehaviour
    {
        List<BspTree> allLevels;

        BspTree dungeonTree;

        int[,] worldMap;
        int[,] borderedMap;

        public int widthDungeon;
        public int heightDungeon;
        public int numberOfOperations;
        public bool shouldDrawOnlyCubes;

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
                        //Vector2 vec2 = new Vector2(posX, posY);

                        if (posX >= 0 && posX < widthDungeon && posY >= 0 && posY < heightDungeon)
                        {
                            worldMap[posX, posY] = 0;
                            if (x >= 0 && y >= 0 && x < tree.room.width - 1 && y < tree.room.height -1  )
                            {
                                worldMap[posX, posY] = 0;
                            }
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
            
        }
        
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                GenerateMap();
            }
        }
    }
}

