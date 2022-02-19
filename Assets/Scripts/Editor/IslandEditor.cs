using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace NoZ.Zisle
{
    /// <summary>
    /// Custom editor for designing the look and feel of the island.
    /// </summary>
    [CustomEditor(typeof(IslandMesh))]
    public class IslandEditor : Editor
    {
        private const float FoamSize = 0.3f;
        private const int ColorEdge = 0;
        
        private static readonly float[] TileToColor = new float[] { 0, 2, 4, 6, 8 };
        private static readonly float[] TileToHeight = new float[] { -0.2f, -0.05f, 0, 0.05f, 0.1f };
        private static readonly string[] TileToClass = new string[] { "water", "path", "grass", "grass2", "grass3" };

        [MenuItem("Zisle/Regenerate Islands Meshes")]
        private static void RegenerateAllMeshes()
        {
            foreach(var island in Resources.FindObjectsOfTypeAll<GameObject>().Select(g => g.GetComponent<IslandMesh>()).Where(c => c != null))
                UpdateMesh(island);

            AssetDatabase.SaveAssets();
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Root
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/IslandEditor.uss"));

            // Only allow editing of the island when the prefab is opened as the current stage
            var currentStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (currentStage?.prefabContentsRoot.gameObject != (target as IslandMesh).gameObject)
            {
                root.AddToClassList("disabled");
                root.Add(new Label("Open the prefab to edit"));
                return root;
            }

            InitializeIsland();

            // Properties
            var foldout = new Foldout();
            foldout.text = "Properties";
            InspectorElement.FillDefaultInspector(foldout, serializedObject, this);
            root.Add(foldout);

            // Grid
            var centered = new VisualElement();
            var grid = CreateGrid();
            centered.AddClass("centered");
            centered.Add(grid);
            root.Add(centered);
            root.AddToClassList("root");

            // Palette
            centered.Add(CreatePalette());

            return root;
        }

        private VisualElement CreateGrid ()
        {
            var grid = new VisualElement();
            grid.AddToClassList("grid");

            var island = (target as IslandMesh);
            var tiles = island.Tiles;
            var gridSize = IslandMesh.GridSize + 2;
            var gridMin = 0;
            var gridMax = gridSize - 1;
            var gridCenter = gridSize / 2;
                      
            for (int y=0; y< gridSize; y++)
            {
                var row = new VisualElement();
                row.AddToClassList("grid-row");
                for (int x=0; x< gridSize; x++)
                {
                    var col = new VisualElement();
                    col.AddToClassList("grid-col");                    
                    row.Add(col);

                    if(x == gridMin || x == gridMax || y == gridMin || y == gridMax)
                    {
                        col.AddToClassList(TileToClass[(int)IslandTile.Water]);
                        continue;
                    }

                    var cell = new Vector2Int(x - 1, y - 1);
                    var tile = island.GetTile(cell);
                    col.RegisterCallback<MouseDownEvent>((evt) => OnTileClick(col, cell));
                    col.RegisterCallback<MouseEnterEvent>((evt) =>
                    {
                        if ((evt.pressedButtons & 1) == 1)
                            OnTileClick(col, cell);
                    });

                    col.AddToClassList(TileToClass[(int)tile]);

                    if ((x == gridCenter && (y == gridMin + 1 || y == gridMax - 1)) || (y == gridCenter && (x == gridMin + 1 || x == gridMax - 1)))
                    {
                        var border = new VisualElement();
                        border.AddToClassList("connection");
                        col.Add(border);
                    }
                }

                grid.Add(row);
            }

            return grid;
        }

        private VisualElement _selectedColor;
        private IslandTile _selectedTile;

        private VisualElement CreatePalette()
        {
            var palette = new VisualElement();

            palette.AddToClassList("palette");

            for(int i=0; i<TileToClass.Length; i++)
                palette.Add(CreatePaletteColor(i));

            _selectedColor = palette.Children().Last();
            _selectedTile = IslandTile.Path;
            _selectedColor.AddToClassList("selected");

            return palette;
        }

        private VisualElement CreatePaletteColor(int index)
        {
            var button = new VisualElement();
            button.AddToClassList("palette-color");
            button.AddToClassList(TileToClass[index]);
            button.AddManipulator(new Clickable(() =>
            {
                if (_selectedColor != null)
                    _selectedColor.RemoveFromClassList("selected");

                _selectedColor = button;
                _selectedTile = (IslandTile)index;

                button.AddToClassList("selected");
            }));

            return button;
        }

        private void OnTileClick (VisualElement tileElement, Vector2Int position)
        {
            var island = target as IslandMesh;

            // Remove any previous classes
            for(int i=0; i< TileToClass.Length; i++)
                tileElement.RemoveFromClassList(TileToClass[i]);

            tileElement.AddToClassList(TileToClass[(int)_selectedTile]);

            island.SetTile(position, _selectedTile);
            EditorUtility.SetDirty(island.gameObject);

            UpdateMesh(target as IslandMesh);
        }


        private void InitializeIsland()
        {
            var dirty = false;
            var island = (target as IslandMesh);

            if (island.Tiles == null || island.Tiles.Length != IslandMesh.GridIndexMax)
            {
                var tiles = new IslandTile[IslandMesh.GridIndexMax];
                for (int y = 0, i = 0; y < IslandMesh.GridSize; y++)
                    for (int x = 0; x < IslandMesh.GridSize; x++, i++)
                        tiles[i] = IslandTile.Grass;

                island.Tiles = tiles;
                dirty = true;
            }

            var meshFilter = island.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = island.gameObject.AddComponent<MeshFilter>();
                dirty = true;
            }

            var meshRenderer = island.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = island.gameObject.AddComponent<MeshRenderer>();
                dirty = true;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Island.mat");
            var foam = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/IslandFoam.mat");
            if (meshRenderer.sharedMaterials.Length != 2 || meshRenderer.sharedMaterials[0] != material || meshRenderer.sharedMaterials[1] != foam)
            {
                meshRenderer.sharedMaterials = new Material[] { material, foam };
                dirty = true;
            }

            if (dirty)
            {
                UpdateMesh(island);
                serializedObject.Update();
            }
        }

        private static void UpdateMesh (IslandMesh island)
        {
            // Determine the prefab path
            var path = AssetDatabase.GetAssetPath(island.gameObject);
            if (string.IsNullOrEmpty(path))
            {
                var currentStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (currentStage?.prefabContentsRoot.gameObject == island.gameObject)
                    path = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(island.gameObject)?.assetPath;
            }

            // Cant update if we dont have a prefab path
            if (string.IsNullOrEmpty(path))
                return;

            var filter = island.GetComponent<MeshFilter>();
            if (null == filter)
                return;

            // Remove old meshes
            foreach(var mesh in AssetDatabase.LoadAllAssetsAtPath(path).Select(a => a as Mesh).Where(m => m != null))
                AssetDatabase.RemoveObjectFromAsset(mesh);

            filter.sharedMesh = GenerateMesh(island);
            
            var collider = island.GetComponent<MeshCollider>();
            if(null == collider)
                collider = island.gameObject.AddComponent<MeshCollider>();

            collider.sharedMesh = filter.sharedMesh;
            island.gameObject.layer = LayerMask.NameToLayer("Ground");


            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Island.mat");
            var foam = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/IslandFoam.mat");
            var meshRenderer = island.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[] { material, foam };

            AssetDatabase.AddObjectToAsset(filter.sharedMesh, path);
            EditorUtility.SetDirty(island.gameObject);
        }

        /// <summary>
        /// Generate an island mesh 
        /// </summary>
        private static Mesh GenerateMesh (IslandMesh island)
        {
            if (island.Tiles.Length != IslandMesh.GridIndexMax)
                return null;

            var builder = new MeshBuilder();
            builder.BeginSubmesh();

            for(int index=0; index<IslandMesh.GridIndexMax; index++)
            {
                var cell = IslandMesh.IndexToCell(index);
                var tile = island.GetTile(index);
                if (tile == IslandTile.Water)
                    continue;

                var world = IslandMesh.CellToLocal(cell);
                var color = TileToColor[(int)tile];
                var colorOffset = (cell.x % 2) == (cell.y % 2) ? 1 : 0;
                var uv = new Vector2(0, color + colorOffset);
                var height = TileToHeight[(int)tile];                    

                builder.BeginConvex();
                builder.AddVertex(new Vector3(world.x - 0.5f, height, world.z + 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x + 0.5f, height, world.z + 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x + 0.5f, height, world.z - 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x - 0.5f, height, world.z - 0.5f), uv, Vector3.up);
                builder.EndConvex();

                height = TileToHeight[(int)IslandTile.Water];
                uv = new Vector2(0, ColorEdge + colorOffset);
                builder.BeginConvex();
                builder.AddVertex(new Vector3(world.x - 0.5f, height, world.z - 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x + 0.5f, height, world.z - 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x + 0.5f, height, world.z + 0.5f), uv, Vector3.up);
                builder.AddVertex(new Vector3(world.x - 0.5f, height, world.z + 0.5f), uv, Vector3.up);
                builder.EndConvex();

                // Add edges on all 4 sides
                for (var dir=0; dir<4; dir++)
                    AddEdge(island, builder, cell, (CardinalDirection)dir, ColorEdge + colorOffset);
            }

            // Add foam for the full grid, including the outside edges
            builder.BeginSubmesh();
            for (int y = -1; y <= IslandMesh.GridSize; y++)
                for (int x = -1; x <= IslandMesh.GridSize; x++)
                {
                    var tile = island.GetTile(new Vector2Int(x, y));
                    if (tile != IslandTile.Water && tile != IslandTile.None)
                        continue;

                    for (int dir = 0; dir < 4; dir++)
                        AddFoam(island, builder, new Vector2Int(x, y), (CardinalDirection)dir);
                }

            var mesh = builder.ToMesh();
            mesh.name = "IslandMesh";
            return mesh;
        }

        private static float GetFoamMitre (IslandMesh island, Vector2Int cell, CardinalDirection dir, CardinalDirection perpendicularDir)
        {
            var offset = dir.ToOffset();
            var perpendicular = perpendicularDir.ToOffset();
            var nearTile = island.GetTile(cell + perpendicular);
            if (nearTile != IslandTile.Water && nearTile != IslandTile.None)
                return -FoamSize;

            var diagonalTile = island.GetTile(cell + offset + perpendicular);
            if (diagonalTile == IslandTile.Water || diagonalTile == IslandTile.None)
                return FoamSize;

            return 0.0f;
        }

        private static void AddFoam (IslandMesh island, MeshBuilder builder, Vector2Int cell, CardinalDirection dir)
        {
            // Get the neighbor tile in the cardinal direction
            var neighbor = island.GetTile(cell + dir.ToOffset());
            if (neighbor == IslandTile.Water || neighbor == IslandTile.None)
                return;

            var rightMitre = GetFoamMitre(island, cell, dir, dir.Rotate(3));
            var leftMitre = GetFoamMitre(island, cell, dir, dir.Rotate(1));

            var edgeNormal = -dir.ToWorld();
            var edgeCenter = IslandMesh.CellToLocal(cell) - edgeNormal * 0.5f;
            var edgePerpendicular = Vector3.Cross(edgeNormal, Vector3.up);
            var edgeHeight = Vector3.up * TileToHeight[(int)IslandTile.Water];

            builder.BeginConvex();
            builder.AddVertex(edgeCenter + edgePerpendicular * 0.5f + edgeHeight, new Vector2(1.0f, 1.0f), Vector3.up);
            builder.AddVertex(edgeCenter + edgePerpendicular * (0.5f + leftMitre) + edgeNormal * FoamSize + edgeHeight, new Vector2(1.0f, 0.0f), Vector3.up);
            builder.AddVertex(edgeCenter - edgePerpendicular * (0.5f + rightMitre) + edgeNormal * FoamSize + edgeHeight, new Vector2(0.0f, 0.0f), Vector3.up);
            builder.AddVertex(edgeCenter - edgePerpendicular * 0.5f + edgeHeight, new Vector2(0.0f, 1.0f), Vector3.up);
            builder.EndConvex();
        }

        private static void AddEdge(IslandMesh island, MeshBuilder builder, Vector2Int cell, CardinalDirection dir, int color)
        {
            var tile = island.GetTile(cell);
            var neighbor = island.GetTile(cell + dir.ToOffset());

            if(neighbor != IslandTile.None && TileToHeight[(int)neighbor] >= TileToHeight[(int)tile])
                return;

            var normal = dir.ToWorld();
            var halfNormal = normal * 0.5f;
            var perpendicular = Vector3.Cross(normal, Vector3.up) * 0.5f;
            var center = IslandMesh.CellToLocal(cell);
            var top = Vector3.up * TileToHeight[(int)tile];
            var bottom = Vector3.up * TileToHeight[(int)IslandTile.Water];

            var uv = new Vector2(0, color);
            builder.BeginConvex();
            builder.AddVertex(center + halfNormal + perpendicular + bottom, uv, normal);
            builder.AddVertex(center + halfNormal - perpendicular + bottom, uv, normal);
            builder.AddVertex(center + halfNormal - perpendicular + top, uv, normal);
            builder.AddVertex(center + halfNormal + perpendicular + top, uv, normal);
            builder.EndConvex();
        }

        private struct PathMapNode
        {
            public Vector2Int from;
            public Vector2Int position;
            public int cost;
        }

        private void GeneratePathMap (IslandMesh islandMesh)
        {
            var dir = CardinalDirection.West;
            var tile = islandMesh.GetTile(new Vector2Int(IslandMesh.GridCenter - dir.ToOffset().x * IslandMesh.GridCenter, IslandMesh.GridCenter - dir.ToOffset().y * IslandMesh.GridCenter));

#if false
            var nodes = new PathMapNode[IslandMesh.GridIndexMax];
            var center = new Vector2Int(IslandMesh.GridCenter, IslandMesh.GridCenter);
            var queue = new Queue<Vector2Int>();

            for (int i=0; i<4; i++)
            {
                // Skip if there is no connection for this direction
                if (!islandMesh.HasConnection((CardinalDirection)i))
                    continue;

                for(int j=0; j < IslandMesh.GridIndexMax; j++)
                    nodes[j] = new PathMapNode();

                queue.Enqueue(center + ((CardinalDirection)i).ToOffset() * (IslandMesh.GridCenter - 1));

                while (queue.Count > 0)
                {
                    var position = queue.Dequeue();
                    ref var node = ref nodes[IslandMesh.PositionToIndex(position)];
                    //if(node.cost == 0)
                    //closed[IslandMesh.PositionToIndex(position)] = true;
                }
            }

            
            

            // TODO: for each tile we need 8 bits, 2 bits for each island exit saying which tile to go to next if following the path

            // TODO: for each exit of the tile run a simple astar algorithm to determine the best path 
#endif
        }
    }
}
