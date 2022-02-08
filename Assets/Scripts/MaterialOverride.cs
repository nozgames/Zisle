using UnityEngine;

namespace NoZ.Zisle
{
    public class MaterialOverride : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _restore;
        private Material _override;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _restore = _renderer.sharedMaterial;
        }

        public Material Override
        {
            get => _override;
            set
            {
                _override = value;
                if(_override != null)
                    _renderer.sharedMaterial = _override;
                else
                    _renderer.sharedMaterial = _restore;
            }
        }
    }
}
