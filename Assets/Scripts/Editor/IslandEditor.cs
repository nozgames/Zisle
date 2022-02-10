using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(IslandMesh))]
    public class IslandEditor : Editor
    {
        private const float FoamSize = 0.3f;
        private const int ColorEdge = 2;

        private static readonly float[] TileToColor = new float[] { 0, 0, 4 };
        private static readonly float[] TileToHeight = new float[] { -0.2f, 0, -0.05f };
        private static readonly string[] TileToClass = new string[] { "water", "grass", "path" };

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

            // Grid
            var grid = CreateGrid();
            root.Add(grid);
            root.AddToClassList("root");

            // Palette
            root.Add(CreatePalette());

            return root;
        }

        private VisualElement CreateGrid ()
        {
            var grid = new VisualElement();
            grid.AddToClassList("grid");

            var island = (target as IslandMesh);
            var tiles = island.Tiles;
                      
            for (int y=0; y<13; y++)
            {
                var row = new VisualElement();
                row.AddToClassList("grid-row");
                for (int x=0; x<13; x++)
                {
                    var col = new VisualElement();
                    col.AddToClassList("grid-col");                    
                    row.Add(col);

                    var tile = island.GetTile(new Vector2Int(x, y));

                    if (x != 0 && y != 0 && y != 12 && x != 12)
                    {
                        var position = new Vector2Int(x, y);
                        col.RegisterCallback<MouseDownEvent>((evt) => OnTileClick(col, position));
                        col.RegisterCallback<MouseEnterEvent>((evt) =>
                        {
                            if ((evt.pressedButtons & 1) == 1)
                                OnTileClick(col, position);
                        });
                    }

                    col.AddToClassList(TileToClass[(int)tile]);

                    if ((x == 6 && (y == 1 || y == 11)) || (y == 6 && (x == 1 || x == 11)))
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

            if (island.Tiles == null || island.Tiles.Length != 13 * 13)
            {
                var tiles = new IslandTile[13 * 13];
                for (int y = 0, i = 0; y < 13; y++)
                    for (int x = 0; x < 13; x++, i++)
                        tiles[i] = (x > 0 && x < 12 && y > 0 && y < 12) ? IslandTile.Grass : IslandTile.Water;

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

            if(filter.sharedMesh != null)
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
            if (island.Tiles.Length != IslandMesh.GridSize * IslandMesh.GridSize)
                return null;

            var builder = new MeshBuilder();
            builder.BeginSubmesh();

            for (int y = 1; y < IslandMesh.GridSize - 1; y++)
            {
                for (int x = 1; x < IslandMesh.GridSize - 1; x++)
                {
                    var tile = island.GetTile(new Vector2Int(x, y));
                    if (tile == IslandTile.Water)
                        continue;

                    var xx = x - 6;
                    var yy = y - 6;
                    var color = TileToColor[(int)tile];
                    var colorOffset = (x % 2) == (y % 2) ? 1 : 0;
                    var uv = new Vector2(0, color + colorOffset);
                    var height = TileToHeight[(int)tile];                    

                    builder.BeginConvex();
                    builder.AddVertex(new Vector3(xx - 0.5f, height, -yy + 0.5f), uv, Vector3.up);
                    builder.AddVertex(new Vector3(xx + 0.5f, height, -yy + 0.5f), uv, Vector3.up);
                    builder.AddVertex(new Vector3(xx + 0.5f, height, -yy - 0.5f), uv, Vector3.up);
                    builder.AddVertex(new Vector3(xx - 0.5f, height, -yy - 0.5f), uv, Vector3.up);
                    builder.EndConvex();

                    AddEdge(island, builder, x, y, -1, 0, ColorEdge + colorOffset);
                    AddEdge(island, builder, x, y, 1, 0, ColorEdge + colorOffset);
                    AddEdge(island, builder, x, y, 0, 1, ColorEdge + colorOffset);
                    AddEdge(island, builder, x, y, 0, -1, ColorEdge + colorOffset);
                }
            }

            AddFoam(island, builder);

            var mesh = builder.ToMesh();
            mesh.name = "IslandMesh";
            return mesh;
        }

        private static void AddFoam(IslandMesh island, MeshBuilder builder)
        {
            builder.BeginSubmesh();

            for(var i=0; i<IslandMesh.GridIndexMax; i++)
            {
                if (!island.IsTile(i, IslandTile.Water))
                    continue;

                AddFoam(island, builder, i, new Vector2Int( 1, 0));
                AddFoam(island, builder, i, new Vector2Int(-1, 0));
                AddFoam(island, builder, i, new Vector2Int( 0, 1));
                AddFoam(island, builder, i, new Vector2Int( 0,-1));
            }
        }

        private static float GetFoamMitre (IslandMesh island, int index, Vector2Int offset, int side)
        {
            var perpendicular = new Vector2Int(offset.y, -offset.x) * side;
            var nearTile = island.GetTile(index, perpendicular);
            if (nearTile != IslandTile.Water)
                return -FoamSize;

            var diagonalTile = island.GetTile(index, offset + perpendicular);
            if (diagonalTile == IslandTile.Water)
                return FoamSize;

            return 0.0f;
        }

        private static void AddFoam (IslandMesh island, MeshBuilder builder, int index, Vector2Int offset)
        {
            var tile = island.GetTile(index, offset);
            if (tile == IslandTile.Water || tile == IslandTile.None)
                return;

            var rightMitre = GetFoamMitre(island, index, offset, 1);
            var leftMitre = GetFoamMitre(island, index, offset, -1);

            offset.y *= -1;

            var normal = (-offset).ToVector3XZ().normalized;
            var position = island.IndexToWorld(index) + new Vector3(offset.x * 0.5f, 0.0f, offset.y * 0.5f);
            var right = Vector3.Cross(normal, Vector3.up);
            var height = Vector3.up * TileToHeight[(int)IslandTile.Water];

            builder.BeginConvex();
            builder.AddVertex(position + right * 0.5f + height, new Vector2(1.0f, 1.0f), Vector3.up);
            builder.AddVertex(position + right * (0.5f + leftMitre) + normal * FoamSize + height, new Vector2(1.0f, 0.0f), Vector3.up);
            builder.AddVertex(position - right * (0.5f + rightMitre) + normal * FoamSize + height, new Vector2(0.0f, 0.0f), Vector3.up);
            builder.AddVertex(position - right * 0.5f + height, new Vector2(0.0f, 1.0f), Vector3.up);
            builder.EndConvex();
        }

        private static void AddEdge(IslandMesh island, MeshBuilder builder, int x, int y, int xdir, int ydir, int color)
        {
            var tile = island.GetTile(new Vector2Int(x, y));
            var neighbor = island.GetTile(new Vector2Int(x + xdir, y + ydir));
            if (neighbor == IslandTile.Grass || tile == neighbor)
                return;

            var n = new Vector3(xdir, 0.0f, -ydir);
            var hn = n * 0.5f;
            var p = Vector3.Cross(n, Vector3.up) * 0.5f;
            var c = new Vector3(x - 6, 0, -(y - 6));
            var s = Vector3.up * TileToHeight[(int)tile];
            var d = Vector3.up * TileToHeight[(int)IslandTile.Water];

            var uv = new Vector2(0, color);
            builder.BeginConvex();
            builder.AddVertex(c + hn + p + d, uv, n);
            builder.AddVertex(c + hn - p + d, uv, n);
            builder.AddVertex(c + hn - p + s, uv, n);
            builder.AddVertex(c + hn + p + s, uv, n);
            builder.EndConvex();
        }
    }
}
