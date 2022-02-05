using NoZ.Animations;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public enum ActorState
    {
        None,
        Idle,
        Run,
        Ability
    }

    public class Actor : NetworkBehaviour, ISerializationCallbackReceiver
    {
        private static readonly int ActorAbilityCount = System.Enum.GetNames(typeof(ActorAttribute)).Length;

        [Header("Attributes")]
        [SerializeField] private float _baseHealthMax = 100.0f;
        [SerializeField] private float _baseHealthRegen = 5.0f;
        [SerializeField] private float _baseSpeed = 10.0f;
        [SerializeField] private float _baseAttack = 60.0f;
        [SerializeField] private float _baseAttackSpeed = 1.0f;
        [SerializeField] private float _baseDefense = 50.0f;
        [SerializeField] private float _baseHarvest = 1.0f;
        [SerializeField] private float _baseBuild = 1.0f;

        [Header("Visuals")]
        [SerializeField] protected Transform _runPitchTransform = null;
        [SerializeField] protected float _runPitch = 20.0f;

        [Header("Animations")]
        [SerializeField] private AnimationShader _idleAnimation = null;
        [SerializeField] private AnimationShader _runAnimation = null;

        [Space]
        [SerializeField] private ActorAbility[] _abilities = null;

        private NetworkVariable<bool> _running = new NetworkVariable<bool>();

        private List<ActorEffect.Context> _effects = new List<ActorEffect.Context>();
        private ActorAttributeValue[] _attributeTable;
        private float _health = 100.0f;
        private BlendedAnimationController _animator;
        private ActorState _state;

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

            foreach (var ability in _abilities)
                ability.RegisterNetworkId();
        }

        public void PlayAnimation(AnimationShader shader, BlendedAnimationController.AnimationCompleteDelegate onComplete=null)
        {
            if (shader == null)
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

        public void Damage (float damage)
        {
            Debug.Log($"Actor took damage of {damage}!");
            _health = Mathf.Clamp(_health - damage, 0.0f, GetAttributeValue(ActorAttribute.HealthMax));
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
        public ActorAbility[] Abilities => _abilities;
        public ActorAttributeValue GetAttribute(ActorAttribute attribute) => _attributeTable[(int)attribute];
        public float GetAttributeValue(ActorAttribute attribute) => _attributeTable[(int)attribute]?.CurrentValue ?? 0.0f;

        public void ResetAttributes()
        {
            for (int i = 0; i < _attributeTable.Length; i++)
            {
                var attribute = _attributeTable[i];
                attribute.Add = 0.0f;
                attribute.Multiply = 1.0f;
                attribute.CurrentValue = attribute.BaseValue;
            }
        }

        public virtual void UpdateAttributes ()
        {
            var oldMaxHealth = GetAttributeValue(ActorAttribute.HealthMax);

            for (int i = 0; i < _attributeTable.Length; i++)
            {
                var attribute = _attributeTable[i];
                attribute.CurrentValue = (attribute.BaseValue + attribute.Add) * attribute.Multiply;
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

        public void OnBeforeSerialize()
        {            
        }

        public void OnAfterDeserialize()
        {
            _attributeTable = new ActorAttributeValue[ActorAbilityCount];
            _attributeTable[(int)ActorAttribute.HealthMax] = new ActorAttributeValue { BaseValue = _baseHealthMax };
            _attributeTable[(int)ActorAttribute.HealthRegen] = new ActorAttributeValue { BaseValue = _baseHealthRegen };
            _attributeTable[(int)ActorAttribute.Speed] = new ActorAttributeValue { BaseValue = _baseSpeed };
            _attributeTable[(int)ActorAttribute.AttackSpeed] = new ActorAttributeValue { BaseValue = _baseAttackSpeed };
            _attributeTable[(int)ActorAttribute.Attack] = new ActorAttributeValue { BaseValue = _baseAttack };
            _attributeTable[(int)ActorAttribute.Defense] = new ActorAttributeValue { BaseValue = _baseDefense };
            _attributeTable[(int)ActorAttribute.Harvest] = new ActorAttributeValue { BaseValue = _baseHarvest };
            _attributeTable[(int)ActorAttribute.Build] = new ActorAttributeValue { BaseValue = _baseBuild };

            foreach (var attribute in _attributeTable)
                if (attribute == null)
                    throw new System.InvalidOperationException("All attributes must be filled in");

            ResetAttributes();
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
        public void ExecuteCommandServerRpc (ulong commandId, ulong sourceId)
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
        public void ExecuteCommandClientRpc (ulong commandId, ulong sourceId)
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
