using UnityEngine;
using System.Collections;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Hit Effect")]
    public class PlayHitEffect : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField] [Range(0, 1)] private float _strength = 1.0f;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            var hiteffect = target.GetComponent<HitEffect>();
            if (hiteffect != null)
                hiteffect.Play(_color, _strength);
        }
    }
}
