using NoZ.Animations;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NoZ.Tweening;
using UnityEngine.AI;
using NoZ.Events;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    /// <summary>
    /// WARNING: DO NOT REORDER THESE!!!
    /// 
    /// List of actor types.  
    /// 
    /// Note that there must be matching entries in the LayerMask as well
    /// </summary>
    public enum ActorType
    {
        /// <summary>
        /// Player
        /// </summary>
        Player,

        /// <summary>
        /// Player's base and ultimate goal of enemy attacks
        /// </summary>
        Base,

        /// <summary>
        /// Building built by players
        /// </summary>
        Building,

        /// <summary>
        /// Enemy that tries to attack player and the base
        /// </summary>
        Enemy
    }

    [System.Flags]
    public enum ActorTypeMask
    {
        None = 0,
        Player = 1 << ActorType.Player,
        Base = 1 << ActorType.Base,
        Building = 1 << ActorType.Building,
        Enemy = 1 << ActorType.Enemy
    }

    public static class ActorTypeMaskExtensions
    {
        private static readonly int _shift = LayerMask.NameToLayer("Player");

        public static LayerMask ToLayerMask (this ActorTypeMask mask) => (LayerMask)((int)mask << _shift);

        public static ActorTypeMask ToMask(this ActorType type) => (ActorTypeMask)(1 << (int)type);

        public static LayerMask ToLayerMask(this ActorType type) => ToLayerMask(ToMask(type));

        public static int ToLayer(this ActorType type) => _shift + (int)type;
    }

    public class Actor : NetworkBehaviour
    {
        public static readonly int ActorTypeCount = System.Enum.GetNames(typeof(ActorType)).Length;
        public static readonly int ActorAttributeCount = System.Enum.GetNames(typeof(ActorAttribute)).Length;

        private const float MaxBusyTime = 5.0f;

        [Header("General")]
        [SerializeField] private ActorDefinition _actorDefinition = null;
        [SerializeField] private Collider _hitCollider = null;

        [Header("Visuals")]
        [SerializeField] protected Material _ghostMaterial = null;
        [SerializeField] protected GameObject _spawnVFXPrefab = null;
        [SerializeField] protected Transform _runPitchTransform = null;
        [SerializeField] protected float _runPitch = 20.0f;
        [SerializeField] protected float _height = 0.5f;

        [Header("Animations")]
        [SerializeField] private AnimationShader _idleAnimation = null;
        [SerializeField] private AnimationShader _runAnimation = null;
        [SerializeField] private AnimationShader _deathAnimation = null;

        private float[] _abilityUsedTime;
        private float _lastAbilityUsedTime;
        private float _lastAbilityUsedEndTime;
        private ActorAbility _lastAbilityUsed;
        private AnimationShader _currentAnimation;

        private List<ActorEffect.Context> _effects = new List<ActorEffect.Context>();
        private ActorAttributeValue[] _attributeTable;
        private IThinkState _thinkState;
        private float _health = 100.0f;
        private BlendedAnimationController _animator;
        private bool _busy;
        private float _busyTime;
        private Vector3 _lastPosition;
        private float _speed = 0.0f;

        public ActorDefinition Definition => _actorDefinition;
        public float Speed => _speed;
        public bool IsMoving => Speed > 0.1f;
        public bool IsDead => _health <= 0.0f;

        public AnimationShader IdleAnimation => _idleAnimation;

        public NavMeshAgent NavAgent { get; private set; }
        public NavMeshObstacle NavObstacle { get; private set; }
        public Material GhostMaterial => _ghostMaterial;

        public ActorAbility LastAbilityUsed => _lastAbilityUsed;
        public float LastAbilityUsedEndTime => _lastAbilityUsedEndTime;
        public float LastAbilityUsedTime => _lastAbilityUsedTime;

        /// <summary>
        /// True if the actor is in a busy state, preventing movement or controls
        /// </summary>
        public bool IsBusy
        {
            get => _busy;
            set
            {
                if (_busy == value)
                    return;

                _busy = value;

                if (value)
                    _busyTime = Time.time;

                OnBusyChanged();
            }
        }

        /// <summary>
        /// True if the actor can be hit by other actors
        /// </summary>
        public bool CanHit
        {
            get => _hitCollider == null ? false : _hitCollider.enabled;
            set
            {
                if (_hitCollider != null)
                    _hitCollider.enabled = value;
            }
        }

        /// <summary>
        /// Current health value of the actor
        /// </summary>
        public float Health
        {
            get => _health;
            protected set => _health = value;
        }

        /// <summary>
        /// Returns true if the actor has taken any damage
        /// </summary>
        public bool IsDamaged => Health < GetAttributeValue(ActorAttribute.HealthMax);

        protected virtual void Awake()
        {
            _animator = GetComponent<BlendedAnimationController>();

            NavAgent = GetComponent<NavMeshAgent>();
            NavObstacle = GetComponent<NavMeshObstacle>();

            if(NavAgent != null)
                NavAgent.updateRotation = false;

            if (_actorDefinition != null)
                _abilityUsedTime = new float[_actorDefinition.Abilities.Length];

            // Force layer of hit collider to match actor type
            if (_hitCollider != null)
                _hitCollider.gameObject.layer = _actorDefinition.ActorType.ToLayer();
        }

        public void PlayAnimation(AnimationShader shader, BlendedAnimationController.AnimationCompleteDelegate onComplete = null)
        {
            if (shader == null || _animator == null || shader == _currentAnimation)
                return;

            _currentAnimation = shader;
            _animator.Play(shader, onComplete: onComplete);
        }

        public void PlayOneShotAnimation(AnimationShader shader)
        {
            if (shader == null || shader == _currentAnimation)
                return;

            IsBusy = true;

            _currentAnimation = shader;
            _animator.Play(shader, onComplete: OnOneShotAnimationComplete);
        }

        private void OnOneShotAnimationComplete ()
        {
            _lastAbilityUsedEndTime = Time.time;
            _currentAnimation = null;
            IsBusy = false;
            UpdateAnimation();
        }

        public virtual void Damage (Actor source, float damage)
        {
            if (IsDead || damage < 0)
                return;

            _health = Mathf.Clamp(_health - damage, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));

            DamageClientRpc(source.OwnerClientId, damage);
            
            if(_health <= 0.0f)
                Die(source);
        }

        public virtual void Heal (Actor source, float heal)
        {
            if (IsDead || heal < 0)
                return;

            _health = Mathf.Clamp(_health + heal, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));

            DamageClientRpc(source.OwnerClientId, heal);
        }

        [ClientRpc]
        private void DamageClientRpc (ulong sourceId, float damage)
        {
            // Only show damage numbers on the local client
            if(sourceId == NetworkManager.LocalClientId)
                UIManager.Instance.AddFloatingText(((int)Mathf.Ceil(damage)).ToString(), null, transform.position + Vector3.up * (_height * 2.0f));
        }

        public virtual void Die (Actor source)
        {
            CanHit = false;
            NavAgent.enabled = false;
            GameEvent.Raise(this, new ActorDiedEvent { });
            DieClientRpc();
        }

        private void Despawn()
        {
            if (IsHost)
                NetworkObject.Despawn(true);
            else
                gameObject.SetActive(false);
        }

        [ClientRpc]
        private void DieClientRpc()
        {
            _health = 0.0f;
            
            if (_deathAnimation != null)
            {
                if (_ghostMaterial != null)
                {
                    foreach (var renderer in GetComponentsInChildren<Renderer>())
                    {
                        if (renderer.materials.Length == 1)
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
                PlayAnimation(_deathAnimation, Despawn);
            }
            else
                Despawn();
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

            if(NavAgent != null)
                NavAgent.speed = GetAttributeValue(ActorAttribute.Speed);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _lastPosition = transform.position;

            if (_spawnVFXPrefab != null)
                Instantiate(_spawnVFXPrefab, transform.position, transform.rotation);

            ResetAttributes();

            UpdateAttributes();

            if (_actorDefinition.Brain != null)
                _thinkState = _actorDefinition.Brain.AllocThinkState(this);

            GameEvent.Raise(this, new ActorSpawnEvent { });
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (_thinkState != null)
                _actorDefinition.Brain.ReleaseThinkState(_thinkState);

            GameEvent.Raise(this, new ActorDespawnEvent{ });
        }

        public void LookAt(Actor actor) => LookAt(actor.transform.position);

        public void LookAt (Vector3 target)
        {
            var delta = (target - transform.position).ZeroY();
            if (delta.sqrMagnitude < (0.001f * 0.001f))
                return;

            transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        }

        public void SetDestination (Vector3 destination, bool force=false, float stoppingDistance=0.001f)
        {
            if (NavAgent == null || (!force && destination == NavAgent.destination))
                return;

            NavAgent.stoppingDistance = stoppingDistance;
            NavAgent.enabled = true;
            NavAgent.SetDestination(destination);
        }

        public virtual bool ExecuteAbility(ActorAbility ability, List<Actor> targets)
        {
            if (ability == null)
                return false;

            if (!ability.Execute(this, targets))
                return false;

            _lastAbilityUsedTime = Time.time;
            _lastAbilityUsed = ability;

            for (int i = 0, c = _actorDefinition.Abilities.Length; i < c; i++)
            {
                if (_actorDefinition.Abilities[i] == ability)
                {
                    _abilityUsedTime[i] = Time.time;
                    break;
                }
            }

            return true;
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

        public float GetAbilityLastUsedTime (ActorAbility actorAbility)
        {
            for (int i = 0, c = _actorDefinition.Abilities.Length; i < c; i++)
            {
                var ability = _actorDefinition.Abilities[i];
                if (ability == actorAbility)
                    return _abilityUsedTime[i];
            }

            return 0.0f;
        }

        protected virtual void OnBusyChanged() { }

        protected virtual void Update ()
        {
            if (!IsSpawned || Game.Instance == null)
                return;

            // Just to make sure actors never get stuck in the busy state
            if (IsBusy && (Time.time - _busyTime) > MaxBusyTime)
                IsBusy = false;

            if (IsDead)
                return;

            _speed = _speed * 0.9f + ((_lastPosition - transform.position).ZeroY().magnitude / Time.deltaTime) * 0.1f;
            _lastPosition = transform.position;

            SnapToGround();
            UpdateAnimation();
            UpdateRunPitch();

            // TODO: think rate?
            if (_actorDefinition.Brain != null)
                _actorDefinition.Brain.Think(this, _thinkState);
        }

        public void SnapToGround()
        {
            // Only actors that can move need to snap
            if (NavAgent == null)
                return;

            if (!Physics.Raycast(transform.position + Vector3.up * _height * 0.5f, Vector3.down, out var hit, 5.0f, GameManager.Instance.GroundLayer))
                return;

            var ychangemax = Time.deltaTime * 0.01f;
            var ychange = Mathf.Clamp(hit.point.y - transform.position.y, -ychangemax, ychangemax);
            //transform.position += Vector3.up * ychange;
            transform.position = hit.point;
        }

        private void UpdateAnimation ()
        {
            if (IsBusy || IsDead)
                return;

            if(!IsMoving)
            {
                PlayAnimation(_idleAnimation);
                return;
            }

            PlayAnimation(_runAnimation);
        }

        private void UpdateRunPitch ()
        {
            if (_runPitchTransform != null)
            {
                var normalizedSpeed = Mathf.Clamp01(_speed / GetAttributeValue(ActorAttribute.Speed));
                _runPitchTransform.localRotation = Quaternion.Euler(_runPitch * normalizedSpeed, 0, 0);
            }
        }
    }
}
