using UnityEngine;
using NoZ.Animations;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Animation")]
    public class PlayAnimation : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private AnimationShader _animation;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            target.PlayOneShotAnimation(_animation);
        }
    }
}
