using FishNet.Component.ColliderRollback;
using UnityEngine;

namespace FishNet.Managing
{
    public partial class NetworkManager : MonoBehaviour
    {

        #region Public.
        /// <summary>
        /// RollbackManager for this NetworkManager.
        /// </summary>
        public RollbackManager RollbackManager { get; private set; }
        #endregion


    }


}