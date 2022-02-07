using UnityEngine.UIElements;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class UILoadingController : UIController
    {
        public new class UxmlFactory : UxmlFactory<UILoadingController, UxmlTraits> { }

        private VisualElement[] _squares;

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBack).Focus();
        }

        private void OnBack()
        {
            UIManager.Instance.ShowCooperative();
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            _squares = new VisualElement[5]
            {
                this.Q("square1"),
                this.Q("square2"),
                this.Q("square3"),
                this.Q("square4"),
                this.Q("square5"),
            };

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
        }

        public override void OnAfterTransitionOut()
        {
            base.OnBeforeTransitionIn();

            if(_squares != null)
            {
                for (int i = 0; i < _squares.Length; i++)
                {
                    var square = _squares[i];
                    square.TweenStop();
                }

                _squares = null;
            }
        }
    }
}
