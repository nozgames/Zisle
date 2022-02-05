using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    [CustomEditor(typeof(Island))]
    public class IslandEditor : Editor
    {
        private const float PathHeight = -0.05f;
        private const float EdgeHeight = -0.5f;

        private const int ColorGrass = 0;
        private const int ColorEdge = 2;
        private const int ColorPath = 4;

        private static readonly string[] TileToClass = new string[]
        {
            "water",
            "grass",
            "path"
        };

        [MenuItem("Zisle/Regenerate Islands Meshes")]
        private static void RegenerateAllMeshes()
        {
            foreach(var island in Resources.FindObjectsOfTypeAll<GameObject>().Select(g => g.GetComponent<Island>()).Where(c => c != null))
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
            if (currentStage?.prefabContentsRoot.gameObject != (target as Island).gameObject)
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

            var island = (target as Island);
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
            var island = target as Island;

            // Remove any previous classes
            for(int i=0; i< TileToClass.Length; i++)
                tileElement.RemoveFromClassList(TileToClass[i]);

            tileElement.AddToClassList(TileToClass[(int)_selectedTile]);

            island.SetTile(position, _selectedTile);
            EditorUtility.SetDirty(island.gameObject);

            UpdateMesh(target as Island);
        }


        private void InitializeIsland()
        {
            var dirty = false;
            var island = (target as Island);

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
            if(meshRenderer.sharedMaterial != material)
            {
                meshRenderer.sharedMaterial = material;
                dirty = true;
            }

            if (dirty)
            {
                UpdateMesh(island);
                serializedObject.Update();
            }
        }

        private static void UpdateMesh (Island island)
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

            AssetDatabase.AddObjectToAsset(filter.sharedMesh, path);
            EditorUtility.SetDirty(island.gameObject);
        }

        /// <summary>
        /// Generate an island mesh 
        /// </summary>
        private static Mesh GenerateMesh (Island island)
        {
            if (island.Tiles.Length != 13 * 13)
                return null;

            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            for (int y = 1, i = 0; y < 12; y++)
                for (int x = 1; x < 12; x++)
                {
                    var tile = island.GetTile(new Vector2Int(x, y));
                    if (tile == 0)
                        continue;

                    var xx = x - 6;
                    var yy = y - 6;
                    var height = 0.0f;
                    var color = ColorGrass;

                    if (tile == IslandTile.Path)
                    {
                        height = PathHeight;
                        color = ColorPath;
                    }

                    verts.Add(new Vector3(xx - 0.5f, height, -yy - 0.5f));
                    verts.Add(new Vector3(xx + 0.5f, height, -yy - 0.5f));
                    verts.Add(new Vector3(xx + 0.5f, height, -yy + 0.5f));
                    verts.Add(new Vector3(xx - 0.5f, height, -yy + 0.5f));

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    var colorOffset = (x % 2) == (y % 2) ? 1 : 0;
                    var uv = new Vector2(0, color + colorOffset);
                    uvs.Add(uv);
                    uvs.Add(uv);
                    uvs.Add(uv);
                    uvs.Add(uv);

                    tris.Add(i + 2);
                    tris.Add(i + 1);
                    tris.Add(i);
                    tris.Add(i + 3);
                    tris.Add(i + 2);
                    tris.Add(i);
                    i += 4;

                    i += AddEdge(island, verts, uvs, tris, normals, x, y, -1, 0, ColorEdge + colorOffset);
                    i += AddEdge(island, verts, uvs, tris, normals, x, y, 1, 0, ColorEdge + colorOffset);
                    i += AddEdge(island, verts, uvs, tris, normals, x, y, 0, 1, ColorEdge + colorOffset);
                    i += AddEdge(island, verts, uvs, tris, normals, x, y, 0, -1, ColorEdge + colorOffset);
                }

            var mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.name = "IslandMesh";
            mesh.UploadMeshData(false);
            return mesh;
        }

        private static int AddEdge(Island island, List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> normals, int x, int y, int xdir, int ydir, int color)
        {
            var tile = island.GetTile(new Vector2Int(x, y));
            var neighbor = island.GetTile(new Vector2Int(x + xdir, y + ydir));
            if (neighbor == IslandTile.Grass || tile == neighbor)
                return 0;

            var n = new Vector3(xdir, 0.0f, -ydir);
            var p = Vector3.Cross(n, Vector3.up);
            var c = new Vector3(x - 6, 0, -(y - 6));
            var s = Vector3.up * (tile == IslandTile.Path ? PathHeight : 0.0f);
            var d = Vector3.up * EdgeHeight;
            var i = verts.Count;

            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);

            n *= 0.5f;
            p *= 0.5f;

            verts.Add(c + n + p + s);
            verts.Add(c + n - p + s);
            verts.Add(c + n - p + d);
            verts.Add(c + n + p + d);

            var uv = new Vector2(0, color);
            uvs.Add(uv);
            uvs.Add(uv);
            uvs.Add(uv);
            uvs.Add(uv);

            tris.Add(i + 2);
            tris.Add(i + 1);
            tris.Add(i);
            tris.Add(i + 3);
            tris.Add(i + 2);
            tris.Add(i);

            return 4;
        }
    }
}
