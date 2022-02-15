namespace NoZ.Zisle.UI
{
    public class UIKeyboardControls : UIScreen
    {
        protected override void Awake ()
        {
            base.Awake();

            Q("back").BindClick(OnBackInternal).Focus();
        }

        private void OnBackInternal() => UIManager.Instance.ShowOptions();

        public override void OnNavigationBack()
        {
            AudioManager.Instance.PlayButtonClick();
            OnBackInternal();
        }
    }
}
