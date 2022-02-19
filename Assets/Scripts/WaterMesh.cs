using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class WaterMesh : MonoBehaviour
    {
        public void Generate(Transform islandParent)
        {
            var islands = islandParent.GetComponentsInChildren<IslandMesh>(false);
            var occupied = new bool[IslandGrid.IndexMax];
            foreach (var island in islands)
                occupied[IslandGrid.CellToIndex(island.Position)] = true;

            // TODO: This should use the tile grid for best results....

            var texture = new Texture2D(IslandGrid.Size * 4, IslandGrid.Size * 4, TextureFormat.Alpha8, true);
            for (int index = 0; index < IslandGrid.IndexMax; index++)
            {
                var cell = IslandGrid.IndexToCell(index);
                for(int x=0; x<4; x++)
                    for(int y=0;y<4; y++)
                        texture.SetPixel(cell.x * 4 + x, cell.y * 4 + y, occupied[index] ? Color.white : new Color(0,0,0,0));
            }

            var waterHalfSize = TileGrid.Size * 0.5f;
            var islandHalfSize = (IslandMesh.GridSize + 1) * 0.5f;
            var offset = Vector3.right * -islandHalfSize + Vector3.forward * islandHalfSize;

            var meshBuilder = new MeshBuilder();
            meshBuilder.BeginConvex();
            meshBuilder.AddVertex(offset - Vector3.right * waterHalfSize + Vector3.forward * waterHalfSize, Vector2.zero);
            meshBuilder.AddVertex(offset + Vector3.right * waterHalfSize + Vector3.forward * waterHalfSize, new Vector2(1,0));
            meshBuilder.AddVertex(offset + Vector3.right * waterHalfSize - Vector3.forward * waterHalfSize, Vector2.one);
            meshBuilder.AddVertex(offset - Vector3.right * waterHalfSize - Vector3.forward * waterHalfSize, new Vector2(0,1));
            meshBuilder.EndConvex();


#if false
            var occupied = new bool[IslandGrid.IndexMax];
            var shallow = new bool[IslandGrid.IndexMax];

            foreach (var island in islands)
                occupied[IslandGrid.CellToIndex(island.Position)] = true;

            for(int index=0; index<IslandGrid.IndexMax; index++)
            {
                shallow[index] = occupied[index];

#if false
                var cell = IslandGrid.IndexToCell(index);
                var neighborCell = IslandGrid.OffsetCell(cell, CardinalDirection.East);
                if(IslandGrid.IsValidCell(neighborCell) && occupied[IslandGrid.CellToIndex(neighborCell)])
                    shallow[index] = true;

                neighborCell = IslandGrid.OffsetCell(cell, CardinalDirection.South);
                if (IslandGrid.IsValidCell(neighborCell) && occupied[IslandGrid.CellToIndex(neighborCell)])
                    shallow[index] = true;

                neighborCell = IslandGrid.OffsetCell(neighborCell, CardinalDirection.East);
                if (IslandGrid.IsValidCell(neighborCell) && occupied[IslandGrid.CellToIndex(neighborCell)])
                    shallow[index] = true;
#endif
            }

            var islandHalfSize = (IslandMesh.GridSize * 0.5f) + 0.5f;
            var meshBuilder = new MeshBuilder();

            var offset = -5;
            var size = IslandGrid.Size - 2 * offset;

            for (int y=0; y<size + 1; y++)
                for (int x = 0; x < size + 1; x++)
                {
                    var cell = new Vector2Int(x + offset, y + offset);
                    var center = IslandGrid.CellToWorld(cell) - new Vector3(islandHalfSize, 0, -islandHalfSize);

                    cell = new Vector2Int(cell.x - 1, cell.y - 1);
                    if (!IslandGrid.IsValidCell(cell) || !shallow[IslandGrid.CellToIndex(cell)])
                        meshBuilder.AddVertex(center, Vector2.one);
                    else
                        meshBuilder.AddVertex(center, Vector3.zero); 
                }

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var b = y * (size + 1) + x;
                    meshBuilder.AddTriangle(b + size + 2, b + size + 1, b);
                    meshBuilder.AddTriangle(b + 1, b + size + 2, b);
                }

            GetComponent<MeshFilter>().sharedMesh = meshBuilder.ToMesh();
#else
            GetComponent<MeshFilter>().sharedMesh = meshBuilder.ToMesh();
            texture.Apply();
            GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Depth", texture);
#endif
        }

        private void OnEnable()
        {
        }
    }
}
