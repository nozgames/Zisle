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
        [SerializeField] private Tag _setScaleRightWeapon = null;
        [SerializeField] private Tag _setScaleLeftWeapon = null;

        public Tag SetScaleRoot => _setScaleRoot;
        public Tag SetScaleBody => _setScaleBody;
        public Tag SetScaleRightWeapon => _setScaleRightWeapon;
        public Tag SetScaleLeftWeapon => _setScaleLeftWeapon;

        public Tag SetPitch => _setPitch;
    }
}
