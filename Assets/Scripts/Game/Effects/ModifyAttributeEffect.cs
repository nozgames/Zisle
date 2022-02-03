using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Actor Effects/Modify Attribute")]
    public class ModifyAttributeEffect : ActorEffect
    {
        [Tooltip("Attribute to effect")]
        [SerializeField] private ActorAttribute _attribute = ActorAttribute.HealthMax;

        [Tooltip("Amount to add to the attribute")]
        [SerializeField] private float _add = 0.0f;

        [Tooltip("Amount to multiply the attribute by")]
        [SerializeField] private float _multiply = 1.0f;

        public override void Apply(Context context)
        {
            var attribute = context.Target.GetAttribute(_attribute);
            if (attribute == null)
                return;

            attribute.Add += _add;
            attribute.Multiply += _multiply;
        }

        public override void Remove(Context context)
        {
            UpdateActorAttributes(context.Target);
        }

        public void UpdateActorAttributes (Actor actor)
        {
            foreach(var effect in actor.Effects)
            {
                if(effect.Effect is ModifyAttributeEffect modify)
                {
                    var attribute = actor.GetAttribute(modify._attribute);
                    if (attribute == null)
                        continue;

                    attribute.Add += _add;
                    attribute.Multiply += _multiply;
                }
            }

            actor.UpdateAttributes();
        }
    }
}
