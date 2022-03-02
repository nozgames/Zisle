using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using NoZ.Animations;
using NoZ.Events;
using NoZ.Zisle.UI;

namespace NoZ.Zisle
{
    public class Actor : NetworkBehaviour
    {
        public static readonly int ActorTypeCount = System.Enum.GetNames(typeof(ActorType)).Length;
        public static readonly int ActorAttributeCount = System.Enum.GetNames(typeof(ActorAttribute)).Length;

        private const float MaxBusyTime = 5.0f;

        [Header("General")]
        [SerializeField] private ActorDefinition _actorDefinition = null;
        [SerializeField] private Collider _hitCollider = null;

        [Header("Visuals")]
        [SerializeField] protected Transform _scaleTransform = null;
        [SerializeField] protected Transform _pitchTransform = null;
        [SerializeField] protected Transform _offsetTransform = null;        
        [SerializeField] protected GameObject _spawnVFXPrefab = null;
        [SerializeField] protected float _runPitch = 20.0f;
        [SerializeField] protected float _height = 0.5f;

        [Header("Slots")]
        [SerializeField] protected Transform _slotRightHand = null;
        [SerializeField] protected Transform _slotLeftHand = null;
        [SerializeField] protected Transform _slotBody = null;

        private EffectList _effects;
        private NetworkVariable<ActorState> _state;

        private float[] _abilityUsedTime;
        private float _lastAbilityUsedTime;
        private float _lastAbilityUsedEndTime;
        private Ability _lastAbilityUsed;
        private AnimationShader _currentAnimation;
        private AnimationShader _oneShotAnimation;
        private LinkedListNode<Actor> _node;
        private WorldVisualElement<HealthCircle> _healthCircle;
        private MaterialPropertyBlock _materialProperties;
        private bool _materialPropertiesDirty;
        private float _visualPitch;

        private ActorAttributeValue[] _attributeTable;
        private IThinkState _thinkState;
        private float _health = 100.0f;
        private BlendedAnimationController _animator;
        private bool _busy;
        private float _busyTime;
        private Vector3 _lastPosition;
        private float _speed = 0.0f;
        private Destination _destination;

        public ActorDefinition Definition => _actorDefinition;
        public float Speed => _speed;
        public bool IsMoving => NavAgent != null && NavAgent.desiredVelocity.sqrMagnitude > 0.1f;
        public bool IsDead => _health <= 0.0f;
        public Destination Destination => _destination;

        /// <summary>
        /// Current target of the actor
        /// </summary>
        public Actor Target => _destination.Target;

        public float Radius => 
            NavAgent != null ? NavAgent.radius : (NavObstacle != null ? NavObstacle.radius : 0.5f);

        public NavMeshAgent NavAgent { get; private set; }
        public NavMeshObstacle NavObstacle { get; private set; }

        public Ability LastAbilityUsed => _lastAbilityUsed;
        public float LastAbilityUsedEndTime => _lastAbilityUsedEndTime;
        public float LastAbilityUsedTime => _lastAbilityUsedTime;

        /// <summary>
        /// Get the array of available abilities for this actor
        /// </summary>
        public Ability[] Abilities => _actorDefinition.Abilities;

        /// <summary>
        /// Get the current effect list for this actor
        /// </summary>
        public EffectList Effects => _effects;

        /// <summary>
        /// Get the actor type
        /// </summary>
        public ActorType Type => _actorDefinition.ActorType;

        /// <summary>
        /// Get the actor type as a mask
        /// </summary>
        public ActorTypeMask TypeMask => (ActorTypeMask)(1 << (int)_actorDefinition.ActorType);

        /// <summary>
        /// Get the current position of the actor
        /// </summary>
        public Vector3 Position => transform.position.ZeroY();

        public Vector3 VisualScale
        {
            get => _scaleTransform == null ? Vector3.one : _scaleTransform.localScale;
            set
            {
                if (_scaleTransform == null)
                    return;

                _scaleTransform.localScale = value;
            }
            
        }

        public float VisualPitch
        {
            get => _visualPitch;
            set
            {
                _visualPitch = value;
                UpdateVisualPitch();
            }
        }

        /// <summary>
        /// Get/Set the visual y offset of the actor.
        /// </summary>
        public float VisualOffsetY
        {
            get => _offsetTransform == null ? _offsetTransform.localPosition.y : 0.0f;
            set
            {
                if (_offsetTransform == null)
                    return;
                var position = _offsetTransform.localPosition;
                position.y = value;
                _offsetTransform.localPosition = position;
            }
        }

        public MaterialPropertyBlock MaterialProperties => _materialProperties;

        public event System.Action<MaterialPropertyBlock> OnMaterialPropertiesChanged;

        /// <summary>
        /// Current actor state 
        /// </summary>
        public ActorState State
        {
            get => _state.Value;
            set
            {
                if (_state.Value == value)
                    return;

                if (!IsHost)
                    throw new System.InvalidOperationException("State can only be set by the Host");

                _state.Value = value;
            }
        }

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

                if (_busy == false && State == ActorState.Dead && IsHost)
                    Despawn();
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

        public float HealthRatio => Health / GetAttributeValue(ActorAttribute.HealthMax);

        /// <summary>
        /// Returns true if the actor has taken any damage
        /// </summary>
        public bool IsDamaged => Health < GetAttributeValue(ActorAttribute.HealthMax);

        public LinkedListNode<Actor> Node => _node;

        public Actor()
        {
            _effects = new EffectList(this);
            _state = new NetworkVariable<ActorState>();
            _node = new LinkedListNode<Actor>(this);
        }

        protected virtual void Awake()
        {
            _materialProperties = new MaterialPropertyBlock();
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

            ResetAttributes();
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

            // If a one shot animation is already being played, stop it first
            if (_oneShotAnimation != null)
                _animator.StopAll(0.0f);

            IsBusy = true;

            _oneShotAnimation = shader;
            _currentAnimation = shader;

            if(IsHost)
                _animator.Play(shader, onComplete: OnAbilityEnd, onEvent: OnOneShotAnimationEvent);
            else
                _animator.Play(shader, onComplete: OnAbilityEnd);
        }

        private void OnOneShotAnimationEvent(Animations.AnimationEvent evt) =>
            _lastAbilityUsed.OnEvent(this, evt, _lastAbilityUsed.FindTargets(this));

        protected virtual void OnAbilityEnd ()
        {
            // Remove any effects that should only last for the duration of an ability
            if(IsHost)
                _effects.RemoveEffects(EffectLifetime.Ability);

            _oneShotAnimation = null;
            _currentAnimation = null;
            _lastAbilityUsedEndTime = Time.time;
            IsBusy = false;
            UpdateAnimation();
        }

        public virtual void Damage (Actor source, float damage)
        {
            if (IsDead || damage < 0)
                return;

            _health = Mathf.Clamp(_health - damage, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));

            OnHealthChanged();

            DamageClientRpc(source.OwnerClientId, damage);
            
            if(_health <= 0.0f)
                Die(source);
        }

        public virtual void Heal (Actor source, float heal)
        {
            if (IsDead || heal < 0)
                return;

            _health = Mathf.Clamp(_health + heal, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));

            OnHealthChanged();

            DamageClientRpc(source.OwnerClientId, heal);
        }

        protected virtual void OnHealthChanged() 
        {
            UpdateHealthCircle();
        }

        private void UpdateHealthCircle()
        {
            if (_healthCircle != null)
            {
                _healthCircle.Element.Health = HealthRatio;
                _healthCircle.Show(HealthRatio < 1.0f);
            }
        }

        [ClientRpc]
        private void DamageClientRpc (ulong sourceId, float damage)
        {
            // Only show damage numbers on the local client
            if(sourceId == NetworkManager.LocalClientId)
                UIManager.Instance.AddFloatingText(((int)Mathf.Ceil(damage)).ToString(), null, transform.position + Vector3.up * (_height * 2.0f));

            _actorDefinition.PlayHitSound(this);
        }

        public virtual void Die (Actor source)
        {
            // Move to the dead state and run one more brain think 
            // to allow any death abilities to fire
            State = ActorState.Dead;

            RemoveHealthCircle();

            CanHit = false;
            if(NavAgent != null)
                NavAgent.enabled = false;
            GameEvent.Raise(this, new ActorDiedEvent { });

            if (_actorDefinition.Brain != null)
                _actorDefinition.Brain.Think(this, _thinkState);

            if (!IsBusy && IsHost)
                Despawn();

//            DieClientRpc();
        }

        private void Despawn()
        {
            if (IsHost)
                NetworkObject.Despawn(true);
            else
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Add an effect to the actor
        /// </summary>
        /// <param name="effect"></param>
        public void AddEffect (Actor source, Effect effect, EffectContext inherit=null) => 
            _effects.Add(source, effect);

        /// <summary>
        /// Return the current modified value for the given attribute
        /// </summary>
        public float GetAttributeValue(ActorAttribute attribute) => _attributeTable[(int)attribute].Value;

        /// <summary>
        /// Reset all attributes back to their base values
        /// </summary>
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

            _state.OnValueChanged += OnStateChanged;

            _lastPosition = transform.position;

            if (_spawnVFXPrefab != null)
                Instantiate(_spawnVFXPrefab, transform.position, transform.rotation);
           
            // On host add the default effects to each actor
            if(IsHost)
            {
                AddEffect(this, GameManager.Instance.DefaultActorEffect);

                foreach (var effect in _actorDefinition.Effects)
                    AddEffect(this, effect);
            }

            UpdateAttributes();

            if (_actorDefinition.Brain != null)
                _thinkState = _actorDefinition.Brain.AllocThinkState(this);

            GameEvent.Raise(this, new ActorSpawnEvent { });

            if(!string.IsNullOrEmpty(Definition.HealthCircleClass))
            {
                _healthCircle = WorldVisualElement<HealthCircle>.Alloc(Vector3.up * (_height + 0.5f), transform);
                _healthCircle.Element.Health = 1.0f;
                _healthCircle.Element.AddToClassList(Definition.HealthCircleClass);
                UpdateHealthCircle();
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _state.OnValueChanged -= OnStateChanged;

            RemoveHealthCircle();

            if (_thinkState != null)
                _actorDefinition.Brain.ReleaseThinkState(_thinkState);

            GameEvent.Raise(this, new ActorDespawnEvent{ });
        }

        private void RemoveHealthCircle()
        {
            if (_healthCircle == null)
                return;

            _healthCircle.Element.RemoveFromClassList(Definition.HealthCircleClass);
            _healthCircle.Release();
            _healthCircle = null;
        }

        public void LookAt(Actor actor) => LookAt(actor.transform.position);

        public void LookAt (Vector3 target)
        {
            var delta = (target - transform.position).ZeroY();
            if (delta.sqrMagnitude < (0.001f * 0.001f))
                return;

            transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        }

        public void SetDestination (Destination destination)
        {
            if (NavAgent == null)
                return;

            if (_destination == destination)
                return;

            _destination = destination;

            if (!_destination.IsValid)
                return;

            // TODO: choose best surround position
            NavAgent.stoppingDistance = destination.StopDistance;

#if true
            var pos = _destination.Position;
            if(_destination.Target != null)
            {
                var delta = (Position - _destination.Target.Position).normalized;
                pos = _destination.Target.Position + delta * (Target.Radius + Radius);
                NavAgent.stoppingDistance -= (Target.Radius - Radius);
            }
#endif
            
            NavAgent.enabled = true;
            NavAgent.SetDestination(pos);
        }

        public virtual void ExecuteAbility(Ability ability)
        {
            if (!IsHost)
                throw new System.InvalidOperationException("Only host can execute abilities");

            // Remove any effects that should only last for the duration of an ability
            _effects.RemoveEffects(EffectLifetime.NextAbility);

            if (ability == null)
                return;

            if (ability.Animation == null)
                return;

            if(Target != null)
                LookAt(Target);

            _lastAbilityUsedTime = Time.time;
            _lastAbilityUsed = ability;

            ability.OnEvent(this, GameManager.Instance.AbilityBeginEvent, ability.FindTargets(this));
            PlayOneShotAnimation(ability.Animation);

            ExecuteAbilityClientRpc(ability.NetworkId);

            for (int i = 0, c = _actorDefinition.Abilities.Length; i < c; i++)
            {
                if (_actorDefinition.Abilities[i] == ability)
                {
                    _abilityUsedTime[i] = Time.time;
                    break;
                }
            }
        }

        [ClientRpc]
        private void ExecuteAbilityClientRpc (ushort abilityId)
        {
            var ability = NetworkScriptableObject.Get<Ability>(abilityId);
            if (null == ability)
                return;

            PlayOneShotAnimation(ability.Animation);
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

        public float GetAbilityLastUsedTime (Ability actorAbility)
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

        protected virtual void Update()
        {
            if (!IsSpawned || Game.Instance == null)
                return;

            // Look for effects ending due to time
            _effects.RemoveEffects(EffectLifetime.Time);

            // Just to make sure actors never get stuck in the busy state
            if (IsBusy && (Time.time - _busyTime) > MaxBusyTime)
                IsBusy = false;

            if (State != ActorState.Active)
                return;

            if (IsDead)
                return;

            _speed = _speed * 0.9f + ((_lastPosition - transform.position).ZeroY().magnitude / Time.deltaTime) * 0.1f;
            _lastPosition = transform.position;

            // Look towards the move direction
            // TODO: we could look at our target when we are close enough maybe
            if(NavAgent != null)
            {
                var velocity = NavAgent.desiredVelocity.ZeroY();
                if (!IsBusy && _destination.IsValid && velocity.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            }

            SnapToGround();
            UpdateAnimation();
            UpdateVisualPitch();

            // TODO: think rate?
            if (_actorDefinition.Brain != null)
                _actorDefinition.Brain.Think(this, _thinkState);
        }

        private void LateUpdate()
        {
            if (_materialPropertiesDirty)
            {
                _materialPropertiesDirty = false;
                OnMaterialPropertiesChanged?.Invoke(_materialProperties);
            }
        }

        public void SnapToGround()
        {
#if false

            // Only actors that can move need to snap
            if (NavAgent == null)
                return;

            if (!Physics.Raycast(_offsetTransform.position + Vector3.up * _height * 0.5f, Vector3.down, out var hit, 5.0f, GameManager.Instance.GroundLayer))
                return;

            //var ychangemax = Time.deltaTime * 0.01f;
            //var ychange = Mathf.Clamp(hit.point.y - _offsetTransform.position.y, -ychangemax, ychangemax);
            //transform.position += Vector3.up * ychange;
            transform.position = hit.point;
#endif
        }

        private void UpdateAnimation ()
        {
            if (IsBusy || IsDead)
                return;

            if(!IsMoving)
            {
                PlayAnimation(_actorDefinition.IdleAnimation);
                return;
            }

            PlayAnimation(_actorDefinition.RunAnimation);
        }

        private void UpdateVisualPitch ()
        {
            if (_pitchTransform == null)
                return;

            var normalizedSpeed = Mathf.Clamp01(_speed / GetAttributeValue(ActorAttribute.Speed));
            _pitchTransform.localRotation = Quaternion.Euler(_runPitch * normalizedSpeed + _visualPitch, 0, 0);
        }

        /// <summary>
        /// Helper function to return the distance between the actor and the given position
        /// </summary>
        public float DistanceTo(Vector3 position) => (transform.position - position).magnitude;

        /// <summary>
        /// Helper function to return the distance between the actor and the given actor
        /// </summary>
        public float DistanceTo(Actor actor) => (Position - actor.Position).magnitude;

        /// <summary>
        /// Helper function to return the distance squared between the actor and the given position
        /// </summary>
        public float DistanceToSqr (Vector3 position) => (transform.position - position).sqrMagnitude;

        /// <summary>
        /// Helper function to return the distance squared between the actor and the given actor
        /// </summary>
        public float DistanceToSqr (Actor actor) => (Position - actor.Position).sqrMagnitude;


        protected virtual void OnStateChanged(ActorState oldState, ActorState newState)
        {
            switch(newState)
            {
                case ActorState.Intro:
                    // TODO: check for intro animation
                    if(IsHost)
                        State = ActorState.Active;
                    break;

                case ActorState.Active:
                    if (NavAgent != null) NavAgent.enabled = true;
                    if (NavObstacle != null) NavObstacle.enabled = true;
                    break;
            }
        }

        /// <summary>
        /// Return the slot for the given slot type
        /// </summary>
        public Transform GetSlotTransform (ActorSlot slot) => slot switch
        {
            ActorSlot.None => transform,
            ActorSlot.RightHand => _slotRightHand,
            ActorSlot.LeftHand => _slotLeftHand,
            ActorSlot.Body => _slotBody,
            _ => throw new System.NotImplementedException()
        };

        /// <summary>
        /// Get the current material color for the given property name
        /// </summary>
        public Color GetMaterialColor(int nameId) => _materialProperties.GetColor(nameId);

        /// <summary>
        /// Set the current material color for the given property name
        /// </summary>
        public void SetMaterialColor(int nameId, Color value)
        {
            _materialProperties.SetColor(nameId, value);
            _materialPropertiesDirty = true;
        }
    }
}
