using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Play Sound")]
    public class PlaySound : ActorCommand, IExecuteOnClient
    {
        [SerializeField] private AudioShader _shader = null;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            if (_shader == null)
                return;

            AudioManager.Instance.PlaySound(_shader, target.gameObject);
        }
    }
}
