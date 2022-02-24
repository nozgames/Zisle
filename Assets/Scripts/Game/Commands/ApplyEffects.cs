using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Apply Effect")]
    public class ApplyEffects : ActorCommand, IExecuteOnServer, IExecuteOnClient
    {
        [SerializeField] private ActorEffect[] _effects = null;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            foreach(var effect in _effects)
                target.AddEffect(source, effect);
        }

        public void ExecuteOnServer(Actor source, Actor target)
        {
        }
    }
}
