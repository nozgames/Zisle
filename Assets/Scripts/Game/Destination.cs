using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public struct Destination : INetworkSerializable
    {
        private const float MinStopDistance = 0.01f;

        public static readonly Destination None = new Destination();

        private bool _valid;
        private Vector3 _position;
        private Actor _target;
        private bool _hasTarget;

        public Actor Target => _target;

        public bool HasTarget => _hasTarget;

        public bool IsValid => _valid && (!_hasTarget || _target != null);

        public Vector3 Position => _target != null ? _target.Position : _position;

        public float StopDistance => MinStopDistance;

        public static bool operator == (Destination lhs, Destination rhs) =>
            lhs._valid == rhs._valid && lhs._target == rhs._target && (lhs._target != null || lhs._position == rhs._position);

        public static bool operator != (Destination lhs, Destination rhs) => !(lhs == rhs);

        public override bool Equals(object obj) => obj is Destination rhs && this == rhs;

        public override int GetHashCode() => 0;

        public Destination (Actor actor)
        {
            _valid = true;
            _target = actor;
            _position = Vector3.zero;
            _hasTarget = true;
        }

        public Destination (Vector3 position)
        {
            _valid = true;
            _target = null;
            _position = position;
            _hasTarget = false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _valid);
            if (!_valid)
            {
                _target = null;
                _position = Vector3.zero;
                return;
            }

            ulong actorNetworkId = 0;
            if (serializer.IsWriter)
                actorNetworkId = _target != null ? _target.NetworkObjectId : 0;

            serializer.SerializeValue(ref actorNetworkId);
            serializer.SerializeValue(ref _position);

            if (serializer.IsReader && actorNetworkId != 0)
                _target = Actor.FromNetworkId(actorNetworkId);
        }
    }
}
