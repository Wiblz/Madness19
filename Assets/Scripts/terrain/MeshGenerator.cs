using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshGenerator : MonoBehaviour {
    public BulletHandler bulletHandler;

    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
    List<List<int>> outlines = new List<List<int>> ();
    HashSet<int> checkedVertices = new HashSet<int>();

    Vector2 impactPosition;
    float impactRadius;

    void OnDrawGizmosSelected() {
        if (squareGrid != null) {
            if (impactPosition != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(impactPosition, 0.05f);
            }
            if (impactRadius != 0f) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(impactPosition, impactRadius);
            }

            for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {

                    // Gizmos.color = squareGrid[x,y].topLeft.GetColor();
                    // Gizmos.DrawWireCube(squareGrid[x,y].topLeft.position, Vector3.one);

                    // Gizmos.color = squareGrid[x,y].topRight.GetColor();
                    // Gizmos.DrawWireCube(squareGrid[x,y].topRight.position, Vector3.one);

                    // Gizmos.color = squareGrid[x,y].bottomRight.GetColor();
                    // Gizmos.DrawWireCube(squareGrid[x,y].bottomRight.position, Vector3.one);

                    // Gizmos.color = squareGrid[x,y].bottomLeft.GetColor();
                    // Gizmos.DrawWireCube(squareGrid[x,y].bottomLeft.position, Vector3.one);

                    // Gizmos.color = squareGrid[x,y].configuration == 0?ControlNode.white:ControlNode.gray;
                    Gizmos.color = squareGrid[x,y].highlighted ? Color.white : Color.clear;

                    Gizmos.DrawLine(squareGrid[x,y].topLeft.position, squareGrid[x,y].topRight.position);
                    // Gizmos.DrawLine(squareGrid[x,y].topRight.position, squareGrid[x,y].bottomRight.position);
                    // Gizmos.DrawLine(squareGrid[x,y].bottomRight.position, squareGrid[x,y].bottomLeft.position);
                    Gizmos.DrawLine(squareGrid[x,y].bottomLeft.position, squareGrid[x,y].topLeft.position);

                    // Gizmos.color = squareGrid[x,y].configuration == 0?ControlNode.white:ControlNode.red;
                    // Gizmos.DrawWireCube(squareGrid[x,y].Center(), Vector3.one);

                    // Gizmos.color = gray;
                    // Gizmos.DrawCube(squareGrid.squares[x,y].centreTop.position, Vector3.one * .15f);
                    // Gizmos.DrawCube(squareGrid.squares[x,y].centreRight.position, Vector3.one * .15f);
                    // Gizmos.DrawCube(squareGrid.squares[x,y].centreBottom.position, Vector3.one * .15f);
                    // Gizmos.DrawCube(squareGrid.squares[x,y].centreLeft.position, Vector3.one * .15f);

                }
            }
        }
    }

    /* DEBUG purposes only */
    void Update() {
        if (Input.GetButtonDown("Fire2")) {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Square s = squareGrid.LocateSquare(position);
            Debug.Log($"{s.topLeft.active} {s.topRight.active}\n{s.bottomLeft.active} {s.bottomRight.active}\n {s.configuration}");
            s.highlighted = true;
        }
    }

    private void A(object sender, BulletHandler.OnBulletExplosionArgs args) {
        impactPosition = args.position;
        impactRadius = args.radius;

        foreach (Square square in squareGrid.GetSquaresInRadius(args.position, args.radius)) {
            bool wasChanged = square.ImpactSquare(args);
            // if (wasChanged) {
            //     square.CalculateConfiguration();
            // }
            square.highlighted = true;
        }

        squareGrid.ResetIndices();
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        vertices = new List<Vector3>();
        triangles = new List<int>();

        Debug.Log(squareGrid.ToString());
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                TriangulateSquare(squareGrid[x, y]);
            }
        }

        Mesh mesh = cave.mesh;
        mesh.Clear();
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        
        Generate2DColliders();
    }

    public void GenerateMesh(int[,] map, float squareSize) {
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();
        bulletHandler.OnBulletExplosion += A;

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        Debug.Log(squareGrid.ToString());
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                TriangulateSquare(squareGrid[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // int tileAmount = 10;
        // Vector2[] uvs = new Vector2[vertices.Count];
        // for (int i = 0; i < vertices.Count; i++) {
        //     float percentX = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize,map.GetLength(0)/2*squareSize,vertices[i].x) * tileAmount;
        //     float percentY = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize,map.GetLength(0)/2*squareSize,vertices[i].z) * tileAmount;
        //     uvs[i] = new Vector2(percentX,percentY);
        // }
        // mesh.uv = uvs;


        Generate2DColliders();
    }

    void Generate2DColliders() {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D> ();
        foreach (EdgeCollider2D collider in currentColliders) {
            Destroy(collider);
        }

        CalculateMeshOutlines();

        Debug.Log(outlines.Count);
        foreach (List<int> outline in outlines) {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++) {
                edgePoints[i] = vertices[outline[i]];
            }
            edgeCollider.points = edgePoints;
        }

    }

    void TriangulateSquare(Square square) {
        switch (square.configuration) {
        case 0:
            break;

        // 1 points:
        case 1:
            MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
            break;
        case 2:
            MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
            break;
        case 4:
            MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
            break;
        case 8:
            MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
            break;

        // 2 points:
        case 3:
            MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
            break;
        case 6:
            MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
            break;
        case 9:
            MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
            break;
        case 12:
            MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
            break;
        case 5:
            MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
            break;
        case 10:
            MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
            break;

        // 3 point:
        case 7:
            MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
            break;
        case 11:
            MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
            break;
        case 13:
            MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
            break;
        case 14:
            MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
            break;

        // 4 point:
        case 15:
            MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
            checkedVertices.Add(square.topLeft.vertexIndex);
            checkedVertices.Add(square.topRight.vertexIndex);
            checkedVertices.Add(square.bottomRight.vertexIndex);
            checkedVertices.Add(square.bottomLeft.vertexIndex);
            break;
        }

    }

    void MeshFromPoints(params Node[] points) {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) 
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void AssignVertices(Node[] points) {
        foreach (Node point in points) {
            if (point.vertexIndex == -1) {
                point.vertexIndex = vertices.Count;
                vertices.Add(point.position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary (triangle.vertexIndexA, triangle);
        AddTriangleToDictionary (triangle.vertexIndexB, triangle);
        AddTriangleToDictionary (triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
        if (triangleDictionary.ContainsKey (vertexIndexKey)) {
            triangleDictionary [vertexIndexKey].Add (triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines() {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1) {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count-1);
                    outlines[outlines.Count-1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex) {
        outlines [outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1) {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex) {
        List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];

        foreach (Triangle triangle in trianglesContainingVertex) {
            for (int j = 0; j < 3; j ++) {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
                    if (IsOutlineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB) {
        List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
        int sharedTriangleCount = 0;

        foreach (Triangle triangle in trianglesContainingVertexA) {
            if (triangle.Contains(vertexB)) {
                sharedTriangleCount ++;
                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle (int a, int b, int c) {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i] {
            get {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex) {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid {
        ControlNode[,] controlNodes;
        public Square[,] squares;
        public float squareSize;
        float mapWidth;
        float mapHeight;

        public SquareGrid(int[,] map, float _squareSize) {
            squareSize = _squareSize;
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            mapWidth = nodeCountX * squareSize;
            mapHeight = nodeCountY * squareSize;

            controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    // Vector2 pos = new Vector2(-mapWidth/2 + x * squareSize + squareSize/2, -mapHeight/2 + y * squareSize + squareSize/2);
                    Vector2 pos = new Vector2(-mapWidth/2 + x * squareSize, -mapHeight/2 + y * squareSize);
                    controlNodes[x,y] = new ControlNode(pos, map[x,y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1,nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
                }
            }
        }

        public Square this[int x, int y] {
            get {
                return squares[x, y];
            }
        }

        public string ToString() {
            string str = "";
            for (int x = 0; x < squares.GetLength(0); x++) {
                for (int y = 0; y < squares.GetLength(1); y++) {
                    str += squares[x, y].configuration + " ";
                }
                str += "\n";
            }

            return str;
        }

        public IEnumerable<Square> GetSquaresInRadius(Vector2 center, float radius) {
            int r = (int)(radius / squareSize) + 1;
            int x = (int)((center.x + mapWidth / 2) / squareSize);
            int y = (int)((center.y + mapHeight / 2) / squareSize);

            yield return squares[x, y];

            for (int dx = -r; dx <= r; dx++) {
                int ddy = (int) Math.Sqrt(r * r - dx * dx);
                for (int dy = -ddy; dy <= ddy; dy++) {
                    yield return squares[x + dx, y + dy];
                    yield return squares[x - dx, y - dy];
                }
            }
        }

        public void ResetIndices() {
            for (int x = 0; x < squares.GetLength(0); x++) {
                for (int y = 0; y < squares.GetLength(1); y++) {
                    foreach (Node node in squares[x,y].GetAllNodes()) {
                        node.vertexIndex = -1;
                    }

                    squares[x,y].CalculateConfiguration();
                }
            }
        }

        public Square LocateSquare(Vector2 position) {
            int x = (int)((position.x + mapWidth / 2) / squareSize);
            int y = (int)((position.y + mapHeight / 2) / squareSize);

            Debug.Log($"{x}, {y}");

            return squares[x, y];
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;
        public bool highlighted = false;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            CalculateConfiguration();
        }

        public void CalculateConfiguration() {
            configuration = 0;
            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }

        public IEnumerable<ControlNode> GetNodes() {
            yield return topLeft;
            yield return topRight;
            yield return bottomRight;
            yield return bottomLeft;
        }

        public IEnumerable<Node> GetAllNodes() {
            yield return topLeft;
            yield return topRight;
            yield return bottomRight;
            yield return bottomLeft;

            yield return centreTop;
            yield return centreRight;
            yield return centreBottom;
            yield return centreLeft;
        }

        public Vector2 Center() {
            return Vector2.Lerp(topLeft.position, bottomRight.position, 0.5f);
        }

        public bool ImpactSquare(BulletHandler.OnBulletExplosionArgs args) {
            bool wasChanged = false;
            foreach (ControlNode node in GetNodes().Where(n => n.active)) {
                float distance = Vector2.Distance(node.position, args.position);
                if (distance <= args.radius) {
                    // float dmg = args.power * 1.5f - (distance / args.radius * args.power);
                    float dmg = args.power - (distance / args.radius * args.power);
                    // Debug.Log($"{distance} - {dmg}");

                    if (node.durability < dmg) {
                        // Debug.Log("DEACTIVATED!");
                        node.active = false;
                        wasChanged = true;
                    }
                }
            }

            return wasChanged;
        }
    }

    public class Node {
        public Vector2 position;
        public int vertexIndex = -1;

        public Node(Vector2 _pos) {
            position = _pos;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        public float durability = 250f;

        public static Color black = new Color(0, 0, 0, 0.5f);
        public static Color white = new Color(1, 1, 1, 0.5f);
        public static Color gray = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public static Color red = new Color(1, 0, 0, 1);

        public ControlNode(Vector2 _pos, bool _active, float squareSize) : base(_pos) {
            active = _active;
            above = new Node(position + Vector2.up * squareSize/2f);
            right = new Node(position + Vector2.right * squareSize/2f);
        }

        public Color GetColor() {
            if (active) {
                return gray;
            } else {
                return white;
            }
        }
    }
}