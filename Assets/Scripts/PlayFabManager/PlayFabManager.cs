using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Assets.Scripts.PlayFabManager
{
    public class PlayFabManager : MonoBehaviour
    {
        [SerializeField]
        GameObject menuObject = default;

        Menu menu;
        bool createAccount;
        string customId;

        void Awake()
        {
            menu = menuObject.GetComponent<Menu>();
        }

        void Start()
        {
            Login();
        }

        public void Login()
        {
            customId = PlayFabCustomId.LoadCustomId(out createAccount);
            var request = new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = createAccount,
            };
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }

        private void OnLoginSuccess(LoginResult result)
        {
            if (createAccount && !result.NewlyCreated)
            {
                Login();
                return;
            }

            if (result.NewlyCreated)
                PlayFabCustomId.SaveCustomId(customId);

            Debug.Log("login success!");

            if (!result.NewlyCreated)
                menu.GetData();
            else
                menu.CreateData();

            menu.Interactable(false);
        }

        private void OnLoginFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
}