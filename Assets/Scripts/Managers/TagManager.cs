using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class TagManager : Singleton<TagManager>
    {
        [Header("SetPitch")]
        [SerializeField] private Tag _setPitch = null;

        [Header("SetScale")]
        [SerializeField] private Tag _setScaleRoot = null;
        [SerializeField] private Tag _setScaleBody = null;
        [SerializeField] private Tag _setScaleRightHand = null;
        [SerializeField] private Tag _setScaleLeftHand = null;

        public Tag SetScaleRoot => _setScaleRoot;
        public Tag SetScaleBody => _setScaleBody;
        public Tag SetScaleRightHand => _setScaleRightHand;
        public Tag SetScaleLeftHand => _setScaleLeftHand;

        public Tag SetPitch => _setPitch;
    }
}
