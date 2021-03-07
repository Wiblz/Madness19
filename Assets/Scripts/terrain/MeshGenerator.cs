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

    static Dictionary<int, Triangle> _triangles = new Dictionary<int, Triangle>();
    static Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>> ();
    HashSet<int> checkedVertices = new HashSet<int>();

    static HashSet<int> freeVertexIndices = new HashSet<int>();
    static HashSet<int> freeTriangleIndices = new HashSet<int>();

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
                    Gizmos.color = squareGrid[x,y].highlighted ? Color.blue : Color.clear;

                    Gizmos.DrawLine(squareGrid[x,y].topLeft.position, squareGrid[x,y].topRight.position);
                    Gizmos.DrawLine(squareGrid[x,y].bottomLeft.position, squareGrid[x,y].topLeft.position);
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

    private int[] convertTriangles() {
        int[] tt = new int[3 * _triangles.Count];
        for (int l = 0; l < _triangles.Count; l++) {
            try{
                tt[l * 3] = _triangles[l][0].vertexIndex;
                tt[l * 3 + 1] = _triangles[l][1].vertexIndex;
                tt[l * 3 + 2] = _triangles[l][2].vertexIndex;
            } catch(IndexOutOfRangeException e) {
                Debug.Log(".");
            }
        }

        return tt;
    }

    private void A(object sender, BulletHandler.OnBulletExplosionArgs args) {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        impactPosition = args.position;
        impactRadius = args.radius;

        freeVertexIndices.Clear();
        freeTriangleIndices.Clear();

        List<Square> impactedSquares = new List<Square>();

        foreach (Square square in squareGrid.GetSquaresInRadius(args.position, args.radius)) {
            bool wasChanged = square.ImpactSquare(args);
            if (wasChanged) {
                impactedSquares.Add(square);
                square.highlighted = true;
            }
        }

        Debug.Log(impactedSquares.Count);

        foreach (Square square in impactedSquares) {
            foreach (var node in square.toRemove.Where(v => v.active)) {
                node.active = false;
                square.verticesRemoved.Add(node);
                
            }
            square.Check();
        }

        foreach (Square square in impactedSquares) {
            foreach (Node v in square.verticesAdded) {
                if (v.vertexIndex == -1) {
                    if (freeVertexIndices.Count == 0) break;
                    v.vertexIndex = freeVertexIndices.First();
                    vertices[v.vertexIndex] = v.position;
                    freeVertexIndices.Remove(v.vertexIndex);
                }
            }
            square.verticesAdded.Clear();

            // var newTriangles = TriangulateSquare(square);
            var newTriangles = TriangulateSquare(square, square.oldConfiguration);
            int i = 0;
            for (; i < newTriangles.Count; i++) {
                if (freeTriangleIndices.Count == 0) break;
                int triangleID = freeTriangleIndices.First();
                freeTriangleIndices.Remove(triangleID);

                AssignTriangle(newTriangles[i], triangleID);
            }

            for (; i < newTriangles.Count; i++) {
                AssignTriangle(newTriangles[i]);
            }
        }

        List<int> _freeVertexIndices = new List<int>(freeVertexIndices);
        List<int> _freeTriangleIndices = new List<int>(freeTriangleIndices);
        _freeVertexIndices.Sort();
        _freeTriangleIndices.Sort();

        _freeVertexIndices.Add(vertices.Count);
        for (int j = 0; j < _freeVertexIndices.Count - 1; j++) {
            for (int k = _freeVertexIndices[j] + 1; k < _freeVertexIndices[j + 1]; k++) {
                foreach (Triangle triangle in triangleDictionary[k]) {
                    triangle.Replace(k, k - j - 1);
                }
                triangleDictionary[k - j - 1] = triangleDictionary[k];
                triangleDictionary[k] = new List<Triangle>();
            }
        }

        for (int i = _freeVertexIndices.Count - 2; i >= 0; i--) {
            vertices.RemoveAt(_freeVertexIndices[i]);
        }

        _freeTriangleIndices.Add(_triangles.Count);
        for (int j = 0; j < _freeTriangleIndices.Count - 1; j++) {
            for (int k = _freeTriangleIndices[j] + 1; k < _freeTriangleIndices[j + 1]; k++) {
                var t = _triangles[k];
                t.id = (k - j - 1);
                _triangles[k - j - 1] = t;
            }
        }

        int c = _triangles.Count;
        for (int i = c - 1; i > c - _freeTriangleIndices.Count; i--) {
            _triangles.Remove(i);
        }

        Mesh mesh = cave.mesh;
        mesh.Clear();
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = convertTriangles();
        mesh.RecalculateNormals();
        
        outlines.Clear();
        checkedVertices.Clear();
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                if (squareGrid[x, y].configuration == 15) {
                    checkedVertices.Add(squareGrid[x, y].topLeft.vertexIndex);
                    checkedVertices.Add(squareGrid[x, y].topRight.vertexIndex);
                    checkedVertices.Add(squareGrid[x, y].bottomRight.vertexIndex);
                    checkedVertices.Add(squareGrid[x, y].bottomLeft.vertexIndex);
                }
            }
        }

        foreach(KeyValuePair<int, List<Triangle>> entry in triangleDictionary) {
            entry.Value.RemoveAll(t => t.Contains(-1));
        }

        Generate2DColliders();

        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds);
    }

    public void GenerateMesh(int[,] map, float squareSize) {
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();
        bulletHandler.OnBulletExplosion += A;

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();

        // Debug.Log(squareGrid.ToString());
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                List<Triangle> newTriangles = TriangulateSquare(squareGrid[x, y]);
                foreach (Triangle t in newTriangles) {
                    AssignTriangle(t);
                }
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = convertTriangles();
        mesh.RecalculateNormals();

        Generate2DColliders();
    }

    void Generate2DColliders() {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders) {
            Destroy(collider);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in outlines) {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++) {
                edgePoints[i] = vertices[outline[i]];
            }
            edgeCollider.points = edgePoints;
        }

    }

    List<Triangle> TriangulateSquare(Square square, int oldConfiguration) {
        List<Triangle> triangles = TriangulateSquare(square);

        if (oldConfiguration == 5  && square.configuration == 4) {
            triangles.RemoveAt(0);
        } else if (oldConfiguration == 7  && square.configuration == 5) {
            triangles.RemoveAt(3);
        } else if (oldConfiguration == 11  && square.configuration == 10) {
            triangles.RemoveAt(0);
            triangles.RemoveAt(1);
        } else if (oldConfiguration == 13  && square.configuration == 9) {
            triangles.RemoveAt(1);
        } else if (oldConfiguration == 13  && square.configuration == 12) {
            triangles.RemoveAt(0);
        } else if (oldConfiguration == 14  && square.configuration == 10) {
            triangles.RemoveAt(3);
            triangles.RemoveAt(2);
        } else if (oldConfiguration == 15  && square.configuration == 11) {
            triangles.RemoveAt(2);
        } else if (square.configuration == 14) {
            triangles.RemoveAt(0);
        }

        return triangles;
    }

    List<Triangle> TriangulateSquare(Square square) {
        switch (square.configuration) {
        case 0:
            return new List<Triangle>();

        // 1 points:
        case 1:
            return MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
        case 2:
            return MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
        case 4:
            return MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
        case 8:
            return MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);

        // 2 points:
        case 3:
            return MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
        case 6:
            return MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
        case 9:
            return MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
        case 12:
            return MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
        case 5:
            return MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
        case 10:
            return MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);

        // 3 point:
        case 7:
            return MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
        case 11:
            return MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
        case 13:
            return MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
        case 14:
            return MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);

        // 4 point:
        case 15:
            checkedVertices.Add(square.topLeft.vertexIndex);
            checkedVertices.Add(square.topRight.vertexIndex);
            checkedVertices.Add(square.bottomRight.vertexIndex);
            checkedVertices.Add(square.bottomLeft.vertexIndex);
            return MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
        }

        return null;
    }

    List<Triangle> MeshFromPoints(params Node[] points) {
        AssignVertices(points);
        var triangles = new List<Triangle>();

        if (points.Length >= 3)
            triangles.Add(new Triangle(points[0], points[1], points[2]));
        if (points.Length >= 4)
            triangles.Add(new Triangle(points[0], points[2], points[3]));
        if (points.Length >= 5)
            triangles.Add(new Triangle(points[0], points[3], points[4]));
        if (points.Length >= 6)
            triangles.Add(new Triangle(points[0], points[4], points[5]));

        return triangles;
    }

    void AssignVertices(Node[] points) {
        foreach (Node point in points) {
            if (point.vertexIndex == -1) {
                point.vertexIndex = vertices.Count;
                vertices.Add(point.position);
            }
        }
    }

    void AssignTriangle(Triangle triangle) {
        triangle.id = _triangles.Count;
        _triangles[triangle.id] = triangle;

        for (int i = 0; i < 3; i++) {
            AddTriangleToDictionary(triangle[i].vertexIndex, triangle);
        }
    }

    void AssignTriangle(Triangle triangle, int id) {
        triangle.id = id;
        _triangles[triangle.id] = triangle;

        for (int i = 0; i < 3; i++) {
            AddTriangleToDictionary(triangle[i].vertexIndex, triangle);
        }
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
        outlines[outlineIndex].Add(vertexIndex);
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
                int vertexB = triangle[j].vertexIndex;
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
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        foreach (Triangle triangle in trianglesContainingVertexA) {
            if (triangle.Contains(vertexB)) {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    class Triangle {
        public int id;
        Node[] vertices;

        public Triangle (Node a, Node b, Node c) {
            id = -1;

            vertices = new Node[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public Node this[int i] {
            get {
                return vertices[i];
            }
        }

        public override string ToString() {
            return $"#{id} ({vertices[0].vertexIndex} {vertices[1].vertexIndex} {vertices[2].vertexIndex})";
        }

        public bool Contains(int vertexIndex) {
            return vertices.Any(v => v.vertexIndex == vertexIndex);
        }

        public void Replace(int from, int to) {
            int nodeIndex = Array.FindIndex(vertices, v => v.vertexIndex == from);
            if (nodeIndex != -1) {
                vertices[nodeIndex].vertexIndex = to;
            }
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

        public IEnumerable<Square> GetSquaresInRadius(Vector2 centre, float radius) {
            int r = (int)(radius / squareSize) + 2;
            int x = (int)((centre.x + mapWidth / 2) / squareSize);
            int y = (int)((centre.y + mapHeight / 2) / squareSize);

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
        public int oldConfiguration;
        public bool highlighted = false;

        static HashSet<int> centreLeftConfigs = new HashSet<int>( new int[] { 1, 3, 5, 7, 8, 10, 12, 14 } );
        static HashSet<int> centreRightConfigs = new HashSet<int>( new int[] { 2, 3, 4, 5, 10, 11, 12, 13 } );
        static HashSet<int> centreTopConfigs = new HashSet<int>( new int[] { 4, 5, 6, 7, 8, 9, 10, 11 } );
        static HashSet<int> centreBottomConfigs = new HashSet<int>( new int[] { 1, 2, 5, 6, 9, 10, 13, 14 } );

        public List<Node> verticesAdded = new List<Node>();
        public List<Node> verticesRemoved = new List<Node>();
        public HashSet<ControlNode> toRemove = new HashSet<ControlNode>();

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

        public override string ToString() {
            string str = "";
            foreach(var node in GetNodes()) {
                str += $"{node.vertexIndex} ";
            }

            return str;
        }

        public void CalculateConfiguration() {
            oldConfiguration = configuration;
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

        public void Check() {
            CalculateConfiguration();

            if (centreLeftConfigs.Contains(configuration)) {
                if (centreLeft.vertexIndex == -1) {
                    verticesAdded.Add(centreLeft);
                }
            } else {
                if (centreLeft.vertexIndex != -1) {
                    verticesRemoved.Add(centreLeft);
                }
            }

            if (centreRightConfigs.Contains(configuration)) {
                if (centreRight.vertexIndex == -1) {
                    verticesAdded.Add(centreRight);
                }
            } else {
                if (centreRight.vertexIndex != -1) {
                    verticesRemoved.Add(centreRight);
                }
            }

            if (centreTopConfigs.Contains(configuration)) {
                if (centreTop.vertexIndex == -1) {
                    verticesAdded.Add(centreTop);
                }
            } else {
                if (centreTop.vertexIndex != -1) {
                    verticesRemoved.Add(centreTop);
                }
            }

            if (centreBottomConfigs.Contains(configuration)) {
                if (centreBottom.vertexIndex == -1) {
                    verticesAdded.Add(centreBottom);
                }
            } else {
                if (centreBottom.vertexIndex != -1) {
                    verticesRemoved.Add(centreBottom);
                }
            }

            foreach (Node point in verticesRemoved.Where(v => v.vertexIndex != -1)) {
                freeVertexIndices.Add(point.vertexIndex);
                foreach (Triangle t in triangleDictionary[point.vertexIndex]) {
                    freeTriangleIndices.Add(t.id);
                }

                triangleDictionary[point.vertexIndex].Clear();
                point.vertexIndex = -1;
            }
            verticesRemoved.Clear();
        }

        public Vector2 centre() {
            return Vector2.Lerp(topLeft.position, bottomRight.position, 0.5f);
        }

        public bool ImpactSquare(BulletHandler.OnBulletExplosionArgs args) {
            bool wasChanged = false;
            foreach (ControlNode node in GetNodes().Where(n => n.active && !toRemove.Contains(n))) {
                float distance = Vector2.Distance(node.position, args.position);
                if (distance <= args.radius) {
                    float dmg = args.power - (distance / args.radius * args.power);
                    // Debug.Log($"{distance} - {dmg}");

                    if (node.durability < dmg) {
                        toRemove.Add(node);
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

        public override string ToString() {
            return $"#{vertexIndex} {position}";
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