using UnityEngine;

namespace NoZ.Zisle
{
    public class ActorRenderer : MonoBehaviour
    {
        private Actor _actor;
        private Renderer _renderer;

        public Actor Actor
        {
            get => _actor;
            set
            {
                if (value == _actor)
                    return;

                if (_actor != null)
                    _actor.OnMaterialPropertiesChanged -= OnMaterialPropertiesChanged;

                _actor = value;

                if (_actor != null)
                    _actor.OnMaterialPropertiesChanged += OnMaterialPropertiesChanged;
            }
        }

        private void Awake() => _renderer = GetComponent<Renderer>();
        private void OnEnable() => Actor = GetComponentInParent<Actor>();
        private void OnTransformParentChanged() => Actor = GetComponentInParent<Actor>();
        private void OnDisable() => Actor = null;

        private void OnMaterialPropertiesChanged(MaterialPropertyBlock properties)
        {
            if(_renderer != null)
                _renderer.SetPropertyBlock(properties);
        }            
    }
}
