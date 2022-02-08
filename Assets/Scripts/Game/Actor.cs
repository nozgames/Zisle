using NoZ.Animations;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NoZ.Tweening;
using UnityEngine.AI;

namespace NoZ.Zisle
{
    public enum ActorState
    {
        None,
        Idle,
        Run,
        Ability
    }

    public enum ActorType
    {
        Player,
        Enemy,
        StaticEnemy,
        Building
    }

    public class Actor : NetworkBehaviour
    {
        private static readonly int ActorAttributeCount = System.Enum.GetNames(typeof(ActorAttribute)).Length;

        [Header("General")]
        [SerializeField] private ActorDefinition _actorDefinition = null;

        [Header("Visuals")]
        [SerializeField] protected Material _ghostMaterial = null;
        [SerializeField] protected Transform _runPitchTransform = null;
        [SerializeField] protected float _runPitch = 20.0f;
        [SerializeField] protected float _height = 0.5f;

        [Header("Animations")]
        [SerializeField] private AnimationShader _idleAnimation = null;
        [SerializeField] private AnimationShader _runAnimation = null;
        [SerializeField] private AnimationShader _deathAnimation = null;

        [SerializeField] private Collider _hitCollider = null;

        private NetworkVariable<bool> _running = new NetworkVariable<bool>();

        private List<ActorEffect.Context> _effects = new List<ActorEffect.Context>();
        private ActorAttributeValue[] _attributeTable;
        private float _health = 100.0f;
        private BlendedAnimationController _animator;
        private ActorState _state;

        public bool IsDead => _health <= 0.0f;

        public AnimationShader IdleAnimation => _idleAnimation;

        public NavMeshAgent NavAgent { get; private set; }
        public NavMeshObstacle NavObstacle { get; private set; }

        public float Health => _health;
        public ActorState State
        {
            get => _state;
            set
            {
                if (value == _state)
                    return;

                var oldState = _state;
                _state = value;

                if(NetworkManager.LocalClientId == OwnerClientId)
                    if (_state == ActorState.Run)
                        SetRunServerRpc(true);
                    else
                        SetRunServerRpc(false);

                OnNetworkStateChanged(oldState, _state);

                //SetStateServerRpc(State);
            }
        }

        [ServerRpc]
        private void SetRunServerRpc(bool run) => _running.Value = run;

        protected virtual void Awake()
        {
            _animator = GetComponent<BlendedAnimationController>();

            NavAgent = GetComponent<NavMeshAgent>();
            NavObstacle = GetComponent<NavMeshObstacle>();

            // By default these should only be enabled on the host
            if (NavAgent != null) NavAgent.enabled = false;
            if (NavObstacle != null) NavObstacle.enabled = false;
        }

        public void PlayAnimation(AnimationShader shader, BlendedAnimationController.AnimationCompleteDelegate onComplete=null)
        {
            if (shader == null || _animator == null)
                return;

            _animator.Play(shader, onComplete: onComplete);
        }

        public void PlayOneShotAnimation(AnimationShader shader, BlendedAnimationController.AnimationCompleteDelegate onComplete = null)
        {
            if (shader == null)
                return;

            _animator.Play(shader, onComplete: () => 
            { 
                State = ActorState.Idle;

                if(OwnerClientId != NetworkManager.LocalClientId && _running.Value)
                    PlayAnimation(_runAnimation);
                else
                    PlayAnimation(_idleAnimation);

                onComplete?.Invoke(); 
            });
        }

        public virtual void Damage (float damage)
        {
            _health = Mathf.Clamp(_health - damage, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));

            UIManager.Instance.AddFloatingText(((int)Mathf.Ceil(damage)).ToString(), null, transform.position + Vector3.up * (_height * 2.0f));

            if(_health <= 0.0f)
                Die();
        }

        public virtual void Die ()
        {
            if(null != _hitCollider)
                _hitCollider.enabled = false;

            if (_deathAnimation != null)
            {
                if (_ghostMaterial != null)
                {
                    foreach (var renderer in GetComponentsInChildren<Renderer>())
                    {
                        if(renderer.materials.Length == 1)
                        {
                            renderer.material = _ghostMaterial;
                            renderer.material.TweenFloat(ShaderPropertyID.Opacity, 0.0f).EaseOutSine().Duration(_deathAnimation.length / _deathAnimation.speed).Play();
                        }
                        else
                        {
                            var materials = new Material[renderer.materials.Length];
                            for (int i = 0; i < renderer.materials.Length; i++)
                                materials[i] = _ghostMaterial;

                            renderer.materials = materials;
                        }
                    }
                }
                PlayAnimation(_deathAnimation, () => NetworkObject.Despawn(true));
            }
            else
                NetworkObject.Despawn(true);
        }

        /// <summary>
        /// Add an effect to the actor
        /// </summary>
        /// <param name="effect"></param>
        public void AddEffect (Actor source, ActorEffect effect)
        {
            if (effect == null || source == null)
                return;

            var context = new ActorEffect.Context { Effect = effect, Target = this, Source = source, Time = Time.timeAsDouble };
            _effects.Add(context);
            effect.Apply(context);
        }

        public List<ActorEffect.Context> Effects => _effects;
        public ActorAbility[] Abilities => _actorDefinition.Abilities;


        //public ActorAttributeValue GetAttribute(ActorAttribute attribute) => _attributeTable[(int)attribute];

        /// <summary>
        /// Return the current modified value for the given attribute
        /// </summary>
        public float GetAttributeValue(ActorAttribute attribute) => _attributeTable[(int)attribute].Value;

        public void ResetAttributes()
        {
            // Allocate a new attribute data and set it to base values
            _attributeTable = new ActorAttributeValue[ActorAttributeCount];
            for (int i = 0; i < _attributeTable.Length; i++)
                _attributeTable[i] = new ActorAttributeValue { Value = _actorDefinition.GetBaseAttribute((ActorAttribute)i), Add = 0.0f, Multiply = 1.0f }; 

            // Reset health back to max health
            _health = GetAttributeValue(ActorAttribute.HealthMax);
        }

        public virtual void UpdateAttributes ()
        {
            var oldMaxHealth = GetAttributeValue(ActorAttribute.HealthMax);

            for (int i = 0; i < ActorAttributeCount; i++)
            {
                ref var attribute = ref _attributeTable[i];
                attribute.Value = (_actorDefinition.GetBaseAttribute((ActorAttribute)i) + attribute.Add) * attribute.Multiply;
                attribute.Add = 0.0f;
                attribute.Multiply = 1.0f;
            }

            var newMaxHealth = GetAttributeValue(ActorAttribute.HealthMax);
            if (newMaxHealth > oldMaxHealth)
                _health += (newMaxHealth - oldMaxHealth);
            else
                _health = Mathf.Min(_health, newMaxHealth);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResetAttributes();

            UpdateAttributes();

            // Local function that waits to play the idle animation until the player stops moving
            System.Collections.IEnumerator WaitForStop(Vector3 start)
            {
                while (!_running.Value)
                {
                    if ((transform.position - start).magnitude < 0.001f)
                    {
                        PlayAnimation(_idleAnimation);
                        break;
                    }

                    yield return null;

                    start = transform.position;
                }
            }


            if (OwnerClientId != NetworkManager.LocalClientId)
                _running.OnValueChanged += (p, n) =>
                {
                    if (_running.Value)
                        PlayAnimation(_runAnimation);
                    else
                        StartCoroutine(WaitForStop(transform.position));
                };

            // Default our state to idle and 
            OnNetworkStateChanged(ActorState.None, ActorState.Idle);
        }

        private void OnNetworkStateChanged(ActorState p, ActorState n)
        {
            _state = n;

            // Local function that waits to play the idle animation until the player stops moving
            System.Collections.IEnumerator WaitForStop(Vector3 start)
            {
                while (_state == ActorState.Idle)
                {
                    if ((transform.position - start).magnitude < 0.001f)
                    {
                        PlayAnimation(_idleAnimation);
                        break;
                    }

                    yield return null;

                    start = transform.position;
                }
            }

            switch (_state)
            {
                case ActorState.Idle:
                    StartCoroutine(WaitForStop(transform.position));
                    break;

                case ActorState.Run:
                    PlayAnimation(_runAnimation);
                    break;
            }
        }

        protected void ExecuteAbility(ActorAbility ability)
        {
            if (ability == null)
                return;

            _state = ActorState.Ability;

            ability.Execute(this);
        }        

        public void ExecuteCommand(ActorCommand command, Actor source)
        {
            // Client command?
            if(command is IExecuteOnClient clientCommand)
            {
                // If coming from us then we can execute the client commands immediately
                if(source.OwnerClientId == NetworkManager.LocalClientId)
                    clientCommand.ExecuteOnClient(source, this);
            }

            // Always execute on the server to make sure client commands are sent to other clients
            ExecuteCommandServerRpc(command.NetworkId, source.NetworkObjectId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ExecuteCommandServerRpc (ushort commandId, ulong sourceId)
        {
            var command = NetworkScriptableObject.Get<ActorCommand>(commandId);
            if (command == null)
                return;

            if (command is IExecuteOnServer serverCommand)
            {
                var source = FromNetworkId(sourceId);
                if (null == source)
                    return;

                serverCommand.ExecuteOnServer(source, this);
            }

            if (command is IExecuteOnClient)
                ExecuteCommandClientRpc(commandId, sourceId);
        }
        
        [ClientRpc]
        public void ExecuteCommandClientRpc (ushort commandId, ulong sourceId)
        {
            // We already executed this command, dont execute again
            var source = FromNetworkId(sourceId);
            if (null == source || source.OwnerClientId == NetworkManager.LocalClientId)
                return;

            var command = NetworkScriptableObject.Get<ActorCommand>(commandId);
            if (command == null)
                return;

            if (command is IExecuteOnClient clientCommand)
                clientCommand.ExecuteOnClient(source, this);
        }

        /// <summary>
        /// Return the actor for the given network id
        /// </summary>
        /// <param name="networkId">Network id</param>
        /// <returns>Actor or null</returns>
        public static Actor FromNetworkId (ulong networkId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var sourceObj))
                return null;

            return sourceObj.GetComponent<Actor>();
        }
    }
}
