using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public abstract class WorldVisualElement
    {
        protected static LinkedList<WorldVisualElement> _elements = new LinkedList<WorldVisualElement>();

        protected LinkedListNode<WorldVisualElement> _node;

        public static VisualElement Root { get; set; }

        public WorldVisualElement()
        {
            _node = new LinkedListNode<WorldVisualElement>(this);
        }

        /// <summary>
        /// Offset from transform in world cooordinates
        /// </summary>
        public Vector3 Offset { get; set; }

        /// <summary>
        /// Transform to display the world element at.  If null will use the UIManager transform which is 0,0,0
        /// </summary>
        public Transform Transform { get; protected set; }        

        /// <summary>
        /// True if the element should scale with the camera zoom
        /// </summary>
        public bool ScaleWithCamera { get; set; } = true;

        /// <summary>
        /// Cast of the Element to a VisualElement
        /// </summary>
        public abstract VisualElement VisualElement { get; }

        /// <summary>
        /// Show / Hide the world element
        /// </summary>
        public void Show (bool show=true) =>
            VisualElement.style.display = new StyleEnum<DisplayStyle>(show ? DisplayStyle.Flex : DisplayStyle.None);

        /// <summary>
        /// Release the world element which will remove it from the world and return it to its pool
        /// </summary>
        public virtual void Release()
        {
            Show(false);
            Root.Remove(VisualElement);
            _elements.Remove(_node);
            Transform = null;
            Offset = Vector3.zero;
            ScaleWithCamera = true;
        }

        public static void UpdateElements ()
        {
            // Cache the FOV correction for scale
            var camera = CameraManager.Instance.Camera;
            var fov = 1.0f / (2.0f * Mathf.Tan(camera.fieldOfView / 2.0f * Mathf.Deg2Rad));
            var pixelsToScale = 1.0f / 10000.0f;

            // Calculate the fov corrected scale using the camera distance
            var scale = 1.0f / ((CameraManager.Instance.IsometricDistance * fov * camera.pixelHeight) * pixelsToScale);
            var styleScale = new StyleScale(new Scale(new Vector3(scale, scale, 1)));

            // Update all world elements
            for (var node = _elements.First; node != null; node = node.Next)
            {
                var we = node.Value;
                if (!we.VisualElement.visible)
                    continue;

                var screen = camera.WorldToViewportPoint(we.Transform.position + we.Offset);

                // Optional scale
                if (we.ScaleWithCamera)
                    we.VisualElement.style.scale = styleScale;

                // Position on screen
                we.VisualElement.style.top = new StyleLength(new Length((1 - screen.y) * 100, LengthUnit.Percent));
                we.VisualElement.style.left = new StyleLength(new Length(screen.x * 100, LengthUnit.Percent));
            }

            // TODO: sort by y
        }
    }

    public class WorldVisualElement<TElement> : WorldVisualElement where TElement : VisualElement, new()
    {
        private static LinkedList<WorldVisualElement> _pool = new LinkedList<WorldVisualElement>();

        public TElement Element { get; private set; }

        public override VisualElement VisualElement => Element;

        public static WorldVisualElement<TElement> Alloc(Transform transform = null) => Alloc(Vector3.zero, transform);

        public static WorldVisualElement<TElement> Alloc (Vector3 offset, Transform transform = null)
        {
            if (transform == null)
                transform = UIManager.Instance.transform;

            WorldVisualElement<TElement> element;

            if(_pool.Count > 0)
            {
                var last = _pool.Last;
                _pool.RemoveLast();
                element = last.Value as WorldVisualElement<TElement>;
            }
            else
            {
                element = new WorldVisualElement<TElement> 
                { 
                    Element = new TElement()
                };
            }

            _elements.AddLast(element._node);
            element.Transform = transform;
            element.Offset = offset;

            Root.Add(element.Element);

            element.Show();

            return element;
        }

        public override void Release ()
        {
            base.Release();

            _pool.AddLast(_node);
        }
    }
}
