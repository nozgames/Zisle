using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Colorize")]
    public class Colorize : ActorEffect
    {
        [ColorUsage(true, true)]
        [SerializeField] private Color _color = new Color(255, 255, 255, 255);

        public override void Apply(ActorEffectContext context)
        {
            //context.Target.Color = _color;
        }

        public override void Remove(ActorEffectContext context)
        {
            //context.Target.Color = Color.white;
        }
    }
}
