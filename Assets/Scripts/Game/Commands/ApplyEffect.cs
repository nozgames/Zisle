using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Apply Effect")]
    public class ApplyEffect : ActorCommand, IExecuteOnServer, IExecuteOnClient
    {
        [SerializeField] private ActorEffect _effect = null;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            target.AddEffect(source, _effect);
        }

        public void ExecuteOnServer(Actor source, Actor target)
        {
        }
    }
}
