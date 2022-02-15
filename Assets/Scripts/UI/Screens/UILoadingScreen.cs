using UnityEngine.UIElements;
using NoZ.Tweening;
using UnityEngine;

namespace NoZ.Zisle.UI
{
    public class UILoadingScreen : UIScreen
    {
        private VisualElement[] _squares;
        private VisualElement _back;

        protected override void Awake ()
        {
            base.Awake();

            _back = BindClick("back", OnBack);

            _squares = new VisualElement[5]
            {
                Q("square1"),
                Q("square2"),
                Q("square3"),
                Q("square4"),
                Q("square5"),
            };
        }

        private void OnBack() => UIManager.Instance.ShowMultiplayer();

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            var small = new StyleLength(new Length(26.0f, LengthUnit.Percent));
            var large = new StyleLength(new Length(90.0f, LengthUnit.Percent));
            var width = StyleLengthMemberProvider<IStyle>.Get("width");
            var height = StyleLengthMemberProvider<IStyle>.Get("height");
            for (int i = 0; i < _squares.Length; i++)
            {
                var square = _squares[i];
                square.style.width = small;
                square.style.height = small;
                square.TweenGroup()
                    .Element(Tween.Sequence(square).Delay(i*0.5f).Element(Tween.FromTo(width, square.style, small, large).Duration(2.0f).PingPong()).Element(Tween.Wait(square, (4-i)*0.5f+3.0f)))
                    .Element(Tween.Sequence(square).Delay(i * 0.5f).Element(Tween.FromTo(height, square.style, small, large).Duration(2.0f).PingPong()).Element(Tween.Wait(square, (4-i) * 0.5f + 3.0f)))
                    .Delay(i)
                    .Loop()
                    .Play();
            }

            _back.Focus();
        }

        public override void OnAfterTransitionOut()
        {
            base.OnBeforeTransitionIn();

            for (int i = 0; i < _squares.Length; i++)
                _squares[i].TweenStop();
        }
    }
}
