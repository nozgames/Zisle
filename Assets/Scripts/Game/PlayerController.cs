using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private float _rotateDuration = 0.05f;
        [SerializeField] private float _moveBounceHeight = 0.5f;
        [SerializeField] private Transform _test = null;

        Tween _moveTween;

        private void OnEnable()
        {
            _moveTween = this.TweenGroup()
                .Element(Tween.FromTo(
                    QuaternionMemberProvider<Transform>.Get("localRotation"),
                    _test,
                    Quaternion.Euler(-5, 90, 0),
                    Quaternion.Euler(5, 90, 0))
                    .Duration(0.2f)
                    .PingPong()
                ).Element(Tween.FromTo(
                    Vector3MemberProvider<Transform>.Get("localPosition"),
                    _test,
                    new Vector3(0,0, 0),
                    new Vector3(0,_moveBounceHeight,0)).Duration(0.2f).PingPong())
                .UpdateMode(UpdateMode.Manual)
                .Loop()
                .Play();
        }

        private void Update()
        {
            var move = InputManager.Instance.playerMove * Time.deltaTime * _speed;

            move = Camera.main.transform.right.ToVector2XZ() * move.x +
                Camera.main.transform.forward.ToVector2XZ() * move.y;

            transform.position += move.ToVector3XZ();

            var t = move.ToVector3XZ();
            if (t.sqrMagnitude > 0)
            {
                transform.TweenRotation(Quaternion.LookRotation(t)).Duration(_rotateDuration).Play();
                _moveTween.Update(Time.deltaTime);
            }
        }
    }
}
