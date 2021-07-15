using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerribleDungeon
{
    public class GenerateMesh : MonoBehaviour
    {
        public SquareGrid squareGrid;
        public MeshFilter dungeonMesh;
        public MeshFilter walls;
        public bool shouldDrawMarchingCubes;

        private List<int> triangles;
        private List<Vector3> vertices;
        private HashSet<int> checkedVertices = new HashSet<int>();
        private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
        private List<List<int>> outlines = new List<List<int>>();

        public void GenerateMeshFromMap(int[,] map, float squareSize)
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            outlines.Clear();
            triangleDictionary.Clear();
            checkedVertices.Clear();

            squareGrid = new SquareGrid(map, squareSize);
            for (int i = 0; i < squareGrid.squares.GetLength(0); i++)
            {
                for (int j = 0;j < squareGrid.squares.GetLength(1); j++)
                {
                    TriangulateSquare(squareGrid.squares[i, j]);
                }
            }
            Mesh mesh = new Mesh();
            dungeonMesh.mesh = mesh;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            int tileAmount = 10;
            Vector2[] uvs = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                float percentegeX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
                float percentegeY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
                uvs[i] = new Vector2(percentegeX, percentegeY);
            }
            mesh.uv = uvs;

            mesh.RecalculateNormals();

            CreateWallMesh(map, squareSize);
        }

        void CreateWallMesh(int[,] map, float squareSize)
        {
            

            CallculateWallMeshOutlines();

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            Mesh wallMesh = new Mesh();

            foreach (List<int> outline in outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(vertices[outline[i]]); // left
                    wallVertices.Add(vertices[outline[i + 1]]); //right
                    wallVertices.Add(vertices[outline[i]] - Vector3.up * 10f); // bottom left
                    wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * 10f); // bottom right

                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }

            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            walls.mesh = wallMesh;

            int tileAmount = 1;
            Vector2[] uvs = new Vector2[wallVertices.Count];
            for (int i = 0; i < wallVertices.Count; i++)
            {
                float percentegeX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, wallVertices[i].x) * tileAmount;
                float percentegeY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, wallVertices[i].y) * tileAmount;
                uvs[i] = new Vector2(percentegeX, percentegeY);
            }
            walls.mesh.uv = uvs;

            walls.mesh.RecalculateNormals();
        }

        void CallculateWallMeshOutlines()
        {
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                if (!checkedVertices.Contains(vertexIndex))
                {
                    int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOutlineVertex != -1)
                    {
                        checkedVertices.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        outlines.Add(newOutline);
                        FollowOutlines(newOutlineVertex, outlines.Count - 1);
                        outlines[outlines.Count - 1].Add(vertexIndex);
                    }
                }
            }
        }

        void FollowOutlines(int vertexIndex, int index)
        {
            outlines[index].Add(vertexIndex);
            checkedVertices.Add(vertexIndex);

            int nextOutlineConnectedVertex = GetConnectedOutlineVertex(vertexIndex);
            if (nextOutlineConnectedVertex != -1)
            {
                FollowOutlines(nextOutlineConnectedVertex, index);
            }
        }

        int GetConnectedOutlineVertex(int vertexIndex)
        {
            List<Triangle> trianglesContainsVertex = triangleDictionary[vertexIndex];
            for (int index = 0; index < trianglesContainsVertex.Count; index++)
            {
                Triangle triangle = trianglesContainsVertex[index];

                for (int i = 0; i < 3; i++)
                {
                    int vertexB = triangle[i];
                    if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                    {
                        if (IsOutlineContainsVertex(vertexIndex, vertexB))
                        {
                            return vertexB;
                        }
                    }
                }
            }
            return -1;
        }

        bool IsOutlineContainsVertex(int vertexA, int vertexB)
        {
            int sharedTrianglesCount = 0;
            List<Triangle> trianglesContainsVertexA = triangleDictionary[vertexA];
            for (int vertexIndex = 0; vertexIndex < trianglesContainsVertexA.Count; vertexIndex++)
            {
                if (trianglesContainsVertexA[vertexIndex].Contains(vertexB))
                {
                    sharedTrianglesCount++;
                    if (sharedTrianglesCount > 1)
                    {
                        break;
                    }
                }
                
            }
            return sharedTrianglesCount == 1;
        }

        private void TriangulateSquare(Square square)
        {
            switch (square.configuration)
            {
                case 0:
                    break;
                case 1:
                    MeshFromPoint(square.centreLeft, square.centreBottom, square.bottomLeft);
                    break;
                case 2:
                    MeshFromPoint(square.bottomRight, square.centreBottom, square.bottomRight);
                    break;
                case 4:
                    MeshFromPoint(square.topRight, square.centreRight, square.centreTop);
                    break;
                case 8:
                    MeshFromPoint(square.topLeft, square.centreTop, square.centreLeft);
                    break;
                // 2 points
                case 3:
                    MeshFromPoint(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                case 6:
                    MeshFromPoint(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                    break;
                case 9:
                    MeshFromPoint(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                    break;
                case 12:
                    MeshFromPoint(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                    break;
                //diagonal
                case 5:
                    MeshFromPoint(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                    break;
                case 10:
                    MeshFromPoint(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;
                //end diagonal
                //3 points
                case 7:
                    MeshFromPoint(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                case 11:
                    MeshFromPoint(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                    break;
                case 13:
                    MeshFromPoint(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                    break;
                case 14:
                    MeshFromPoint(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;
               
                //4 points
                case 15:
                    MeshFromPoint(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);

                    //checkedVertices.Add(square.topLeft.vertexIdex);
                    //checkedVertices.Add(square.topRight.vertexIdex);
                    //checkedVertices.Add(square.bottomRight.vertexIdex);
                    //checkedVertices.Add(square.bottomLeft.vertexIdex);
                    break;
                default:
                    break;
            }
        }

        private void MeshFromPoint(params Node[] points)
        {
            AssingPoints(points);

            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }
            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }
            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }
            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }

        private void CreateTriangle(Node a, Node b, Node c)
        {
            triangles.Add(a.vertexIdex);
            triangles.Add(b.vertexIdex);
            triangles.Add(c.vertexIdex);

            Triangle triangle = new Triangle(a.vertexIdex, b.vertexIdex, c.vertexIdex);
            AddToTriangleDictionary(a.vertexIdex, triangle);
            AddToTriangleDictionary(b.vertexIdex, triangle);
            AddToTriangleDictionary(c.vertexIdex, triangle);
        }

        private void AssingPoints(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].vertexIdex == -1)
                {
                    points[i].vertexIdex = vertices.Count;
                    vertices.Add(points[i].position);
                }
            }
        }

        private void AddToTriangleDictionary(int vertexIndex, Triangle triangle)
        {
            if (triangleDictionary.ContainsKey(vertexIndex))
            {
                triangleDictionary[vertexIndex].Add(triangle);
            }
            else
            {
                List<Triangle> newList = new List<Triangle>();
                newList.Add(triangle);
                triangleDictionary[vertexIndex] = newList;
            }
        }

        public struct Triangle
        {
            public int vertexIndexA;
            public int vertexIndexB;
            public int vertexIndexC;
            public int[] vertices;

            public Triangle(int vertA, int vertB, int vertC)
            {
                vertexIndexA = vertA;
                vertexIndexB = vertB;
                vertexIndexC = vertC;

                vertices = new int[3];
                vertices[0] = vertexIndexA;
                vertices[1] = vertexIndexB;
                vertices[2] = vertexIndexC;
            }

            public int this[int index]
            {
                get { return vertices[index]; }
            }

            public bool Contains(int otherVertex)
            {
                return vertexIndexA == otherVertex || vertexIndexB == otherVertex || vertexIndexC == otherVertex;
            }
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

