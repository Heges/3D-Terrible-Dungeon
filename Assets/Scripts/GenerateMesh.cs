using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerribleDungeon
{
    public class GenerateMesh : MonoBehaviour
    {
        public SquareGrid squareGrid;
        public bool shouldDrawMarchingCubes;

        public void GenerateMeshFromMap(int[,] map, float squareSize)
        {
            squareGrid = new SquareGrid(map, squareSize);
        }

        public class SquareGrid
        {
            public Square[,] squares;

            public SquareGrid(int[,] map, float squareSize)
            {
                int nodeCountX = map.GetLength(0);
                int nodeCountY = map.GetLength(1);
                float mapWidth = nodeCountX * squareSize;
                float mapHeight = nodeCountY * squareSize;

                ControllNode[,] controllNodes = new ControllNode[nodeCountX, nodeCountY];
                for (int x = 0; x < nodeCountX; x++)
                {
                    for (int y = 0; y < nodeCountY; y++)
                    {
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                        controllNodes[x, y] = new ControllNode(pos, map[x, y] == 1, squareSize);
                    }
                }
                squares = new Square[nodeCountX - 1, nodeCountY - 1];
                for (int x = 0; x < nodeCountX - 1; x++)
                {
                    for (int y = 0; y < nodeCountY - 1; y++)
                    {
                        squares[x, y] = new Square(controllNodes[x, y + 1], controllNodes[x + 1, y + 1], controllNodes[x, y], controllNodes[x + 1, y]);
                    }
                }
            }
        }

        public class Square
        {
            public ControllNode topLeft, topRight, bottomLeft, bottomRight;
            public Node centreTop, centreRight, centreLeft, centreBottom;
            public int configuration;

            public Square(ControllNode topLeft, ControllNode topRight, ControllNode bottomLeft, ControllNode bottomRight)
            {
                this.topLeft = topLeft;
                this.topRight = topRight;
                this.bottomLeft = bottomLeft;
                this.bottomRight = bottomRight;

                centreTop = topLeft.right;
                centreRight = bottomRight.above;
                centreLeft = bottomLeft.above;
                centreBottom = bottomLeft.right;

                if (topLeft.active)
                {
                    configuration += 8;
                }
                if (topRight.active)
                {
                    configuration += 4;
                }
                if (bottomRight.active)
                {
                    configuration += 2;
                }
                if (bottomLeft.active)
                {
                    configuration += 1;
                }
            }
        }

        public class Node
        {
            public Vector3 position;
            public int vertexIdex = -1;

            public Node(Vector3 _pos)
            {
                position = _pos;
            }
        }

        public class ControllNode : Node
        {
            public bool active;
            public Node above, right;

            public ControllNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
            {
                active = _active;
                above = new Node(position + Vector3.forward * squareSize/2f);
                right = new Node(position + Vector3.right * squareSize/2f);
            }
        }

        private void OnDrawGizmos()
        {
            if (shouldDrawMarchingCubes)
            {
                if (squareGrid != null)
                {
                    for (int i = 0; i < squareGrid.squares.GetLength(0); i++)
                    {
                        for (int j = 0; j < squareGrid.squares.GetLength(1); j++)
                        {
                            Gizmos.color = squareGrid.squares[i, j].topLeft.active ? Color.black : Color.white;
                            Gizmos.DrawCube(squareGrid.squares[i, j].topLeft.position, Vector3.one * 0.4f);

                            Gizmos.color = squareGrid.squares[i, j].topRight.active ? Color.black : Color.white;
                            Gizmos.DrawCube(squareGrid.squares[i, j].topRight.position, Vector3.one * 0.4f);

                            Gizmos.color = squareGrid.squares[i, j].bottomRight.active ? Color.black : Color.white;
                            Gizmos.DrawCube(squareGrid.squares[i, j].bottomRight.position, Vector3.one * 0.4f);

                            Gizmos.color = squareGrid.squares[i, j].bottomLeft.active ? Color.black : Color.white;
                            Gizmos.DrawCube(squareGrid.squares[i, j].bottomLeft.position, Vector3.one * 0.4f);

                            Gizmos.color = Color.gray;
                            Gizmos.DrawCube(squareGrid.squares[i, j].centreTop.position, Vector3.one * 0.15f);
                            Gizmos.DrawCube(squareGrid.squares[i, j].centreRight.position, Vector3.one * 0.15f);
                            Gizmos.DrawCube(squareGrid.squares[i, j].centreBottom.position, Vector3.one * 0.15f);
                            Gizmos.DrawCube(squareGrid.squares[i, j].centreLeft.position, Vector3.one * 0.15f);
                        }
                    }
                }
            }
        }
    }
}

