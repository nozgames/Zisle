using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class ConnectingController : UIController
    {
        public new class UxmlFactory : UxmlFactory<ConnectingController, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", OnBack).Focus();
            //BindClick("join", OnJoin);
            //BindClick("host", OnHost);
            //BindClick("continue", OnContinue);
        }

        private void OnBack()
        {
            // TODO: cancel the connection

            // TODO: if we were on the join menu then show the join menu instead?
            UIManager.Instance.ShowCooperative();
        }
    }
}
