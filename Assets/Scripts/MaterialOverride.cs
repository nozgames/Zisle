using System.Linq;
using UnityEngine;

namespace NoZ.Zisle
{
    public class MaterialOverride : MonoBehaviour
    {
        private struct CachedRenderer
        {
            public Renderer Renderer;
            public Material[] Materials;
            public Material[] Overrides;
        }

        private CachedRenderer[] _renderers;

        public void Clear ()
        {
            if (_renderers != null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                renderer.Renderer.sharedMaterials = renderer.Materials;
            }
        }

        public void Override (Material original, Material replace, MaterialPropertyBlock properties = null)
        {
            if(null == _renderers)
            {
                _renderers = GetComponentsInChildren<Renderer>().Select(r => new CachedRenderer
                {
                    Renderer = r,
                    Materials = r.sharedMaterials,
                    Overrides = r.sharedMaterials.ToArray()
                }).ToArray();
            }

            for(int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                var renderer = _renderers[rendererIndex];
                for (int materialIndex = 0; materialIndex < renderer.Materials.Length; materialIndex++)
                    if (original == null || renderer.Materials[materialIndex] == original)
                    {
                        renderer.Overrides[materialIndex] = replace;
                        renderer.Renderer.SetPropertyBlock(properties, materialIndex);
                    }

                renderer.Renderer.sharedMaterials = renderer.Overrides;
            }
        }
    }
}
