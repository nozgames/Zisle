using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Damage")]
    public class Damage : ActorCommand, IExecuteOnServer
    {
        public void ExecuteOnServer(Actor source, Actor target)
        {
            // Calculate the total damage and apply to the target
            var attack = source.GetAttributeValue(ActorAttribute.Attack);
            var defense = target.GetAttributeValue(ActorAttribute.Defense);
            var damage = Mathf.Max(attack - defense, 0.0f);            
            target.Damage(damage);
        }
    }
}
