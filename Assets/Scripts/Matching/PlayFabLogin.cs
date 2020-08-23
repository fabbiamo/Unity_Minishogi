using Assets.Scripts.Matching;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

struct UsersData
{
    public string UserName;
    public int Game;
    public int Win;
    public int Lose;
    public double Rating;
    public double OpponentRating;
};

public class PlayFabLogin : MonoBehaviour
{

    [SerializeField] GameObject titleCanvas, accountCanvas;
    [SerializeField] Button matchingButton, accountButton, exitButton;
    [SerializeField] Text matchingText;
    [SerializeField] TextMeshProUGUI ratingText, winText, loseText;
    [SerializeField] TMP_InputField userNameField = default;

    private bool _shouldCreateAccount;
    private string _customID;
    private string _matchID;
    private string _queueName = "1vs1";

    private static UsersData usersData;
    PhotonManager photonManager;

    public void Start()
    {
        photonManager = GameObject.FindWithTag("Photon").GetComponent<PhotonManager>();
        
        Login();

        userNameField.text = usersData.UserName;

        matchingButton.onClick.AddListener(Matchmaking);
        accountButton.onClick.AddListener(DisplayAccount);
        exitButton.onClick.AddListener(DisplayTitle);
    }

    public void Login()
    {
        _customID = LoadCustomID();
        var request = new LoginWithCustomIDRequest { CustomId = _customID, CreateAccount = _shouldCreateAccount };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        //Debug.Log("Congratulations, you made your first successful API call!");
        
        if(_shouldCreateAccount && ! result.NewlyCreated)
        {
            Debug.LogWarning($"CustomId : {_customID} は既に使われています");
            Login();
            return;
        }

        if (result.NewlyCreated)
        {
            SaveCustomID();
        }

        Debug.Log($"PlayFabのログインに成功\nPlayFabId : {result.PlayFabId}, CustomId : {_customID}\nアカウントを作成したか : {result.NewlyCreated}");

        if (!result.NewlyCreated)
            GetUserData();
        else
            NewUserData(); // 内部でGetUserData()を呼び出す

        // すべてのボタンを有効にする
        matchingButton.interactable = true;
        accountButton.interactable = true;
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    public static void GetTitleData()
    {
        var request = new GetTitleDataRequest();
        PlayFabClientAPI.GetTitleData(request, OnSuccess, OnError);

        void OnSuccess(GetTitleDataResult result)
        {
            Debug.Log("GetTitleData: Success!");

            var loginMessage = result.Data["LoginMessage"];
            Debug.Log(loginMessage);
        }

        void OnError(PlayFabError error)
        {
            Debug.Log("GetTitleData: Fail...");
            Debug.Log(error.GenerateErrorReport());
        }
    }

    //=================================================================================
    // カスタムIDの取得
    //=================================================================================

    //IDを保存する時のKEY
    private static readonly string CUSTOM_ID_SAVE_KEY = "CUSTOM_ID_SAVE_KEY";

    //IDを取得
    private string LoadCustomID()
    {
        //IDを取得
        string id = PlayerPrefs.GetString(CUSTOM_ID_SAVE_KEY);

        //保存されていなければ新規生成
        _shouldCreateAccount = string.IsNullOrEmpty(id);
        return _shouldCreateAccount ? GenerateCustomID() : id;
    }

    //IDの保存
    private void SaveCustomID()
    {
        PlayerPrefs.SetString(CUSTOM_ID_SAVE_KEY, _customID);
    }


    //=================================================================================
    // カスタムIDの生成
    //=================================================================================

    //IDに使用する文字
    private static readonly string ID_CHARACTERS = "0123456789abcdefghijklmnopqrstuvwxyz";

    //IDを生成する
    private string GenerateCustomID()
    {
        int idLength = 32;//IDの長さ
        StringBuilder stringBuilder = new StringBuilder(idLength);
        var random = new System.Random();

        //ランダムにIDを生成
        for (int i = 0; i < idLength; i++)
        {
            stringBuilder.Append(ID_CHARACTERS[random.Next(ID_CHARACTERS.Length)]);
        }

        return stringBuilder.ToString();
    }

    //=================================================================================
    // ユーザーデータ
    //=================================================================================

    public static void GetUserData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnSuccess, OnError);

        void OnSuccess(GetUserDataResult result)
        {
            usersData.UserName = result.Data["Name"].Value;
            usersData.Game = int.Parse(result.Data["Game"].Value);
            usersData.Win = int.Parse(result.Data["Win"].Value);
            usersData.Lose = int.Parse(result.Data["Lose"].Value);
            usersData.Rating = double.Parse(result.Data["Rating"].Value);
        }

        void OnError(PlayFabError error)
        {
            Debug.Log("GetUserData: Fail...");
            Debug.Log(error.GenerateErrorReport());
        }
    }

    private void NewUserData()
    {
        var request = new UpdateUserDataRequest()
        {
            // 初期値
            Data = new Dictionary<string, string>
            {
                {"Name", "UserName" },
                {"Rating", "1500"},
                {"Game", "0"},
                {"Win", "0"},
                {"Lose", "0"},
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnError);

        void OnSuccess(UpdateUserDataResult result)
        {
            Debug.Log("CreateUseData: Success!");
            GetUserData();
        }

        void OnError(PlayFabError error)
        {
            Debug.Log("CreateUserData: Fail...");
            Debug.Log(error.GenerateErrorReport());
        }
    }

    public static void UpdateUserData()
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>
            {
                {"Name"  , usersData.UserName},
                {"Rating", usersData.Rating.ToString()},
                {"Game"  , usersData.Game.ToString()},
                {"Win"   , usersData.Win.ToString()},
                {"Lose"  , usersData.Lose.ToString()},
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnError);

        void OnSuccess(UpdateUserDataResult result)
        {
            Debug.Log("UpdateUseData: Success!");
        }

        void OnError(PlayFabError error)
        {
            Debug.Log("Fail...");
            Debug.Log(error.GenerateErrorReport());
        }
    }

    // 対局後に使用
    public static void UpdateUserData(bool isWin)
    {
        double calcRating(double ratingA, double ratingB)
        {
            // 定数
            double k = 32f;

            // Bの期待勝率
            double win_rate = 1 / (Math.Pow(10, (ratingA - ratingB) / 400) + 1);

            return isWin
                ? k * win_rate           // Aが勝ったとき
                : k * (1.00 - win_rate); // Bが勝ったとき
        }

        double diff = calcRating(usersData.Rating, usersData.OpponentRating);

        double oldRating = usersData.Rating;
        
        usersData.Game += 1;
        if (isWin)
        {
            usersData.Win += 1;
            usersData.Rating += diff;
        }
        else
        {
            usersData.Lose += 1;
            usersData.Rating -= diff;
        }

        Debug.LogFormat("{0} -> {1}", oldRating, usersData.Rating);

        UpdateUserData();
    }

    private void DisplayAccount()
    {
        accountCanvas.SetActive(true);
        titleCanvas.SetActive(false);

        userNameField.text = usersData.UserName;
        ratingText.text = Math.Round(usersData.Rating, MidpointRounding.AwayFromZero).ToString();
        winText.text = usersData.Win.ToString();
        loseText.text = usersData.Lose.ToString();
    }

    private void DisplayTitle()
    {
        if (userNameField.text != usersData.UserName)
        {
            if (!String.IsNullOrWhiteSpace(userNameField.text))
            {
                usersData.UserName = userNameField.text;
            }
        }

        accountCanvas.SetActive(false);
        titleCanvas.SetActive(true);

        UpdateUserData();
    }

    //=================================================================================
    // マッチング
    //=================================================================================

    private void Matchmaking()
    {
        //matchingText.text = "マッチメイキングチケットをキューに積みます...\n";

        // プレイヤーの情報を作ります。
        var matchmakingPlayer = new MatchmakingPlayer
        {
            // Entityは下記のコードで決め打ちで大丈夫です。
            Entity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = PlayFabSettings.staticPlayer.EntityId,
                Type = PlayFabSettings.staticPlayer.EntityType
            },

            Attributes = new MatchmakingPlayerAttributes
            {
                // このプレイヤーは Rating ±1000 のプレイヤーとしかマッチングしない
                DataObject = new { Rating = usersData.Rating }
            }
        };

        var request = new CreateMatchmakingTicketRequest
        {
            // 先程作っておいたプレイヤー情報です。
            Creator = matchmakingPlayer,
            // マッチングできるまで待機する秒数を指定します。最大600秒です。
            GiveUpAfterSeconds = 60,
            // GameManagerで作ったキューの名前を指定します。
            QueueName = _queueName,
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnCreateMatchmakingTicketSuccess, OnFailure);

        void OnCreateMatchmakingTicketSuccess(CreateMatchmakingTicketResult result)
        {
            //matchingText.text = "マッチメイキングチケットをキューに積みました！\n\n";

            // キューに積んだチケットの状態をマッチングするかタイムアウトするまでポーリングします。
            var getMatchmakingTicketRequest = new GetMatchmakingTicketRequest
            {
                TicketId = result.TicketId,
                QueueName = request.QueueName,
            };

            StartCoroutine(Polling(getMatchmakingTicketRequest));
        }
    }

    IEnumerator Polling(GetMatchmakingTicketRequest request)
    {
        // ポーリングは1分間に10回まで許可されているので、6秒間隔で実行するのがおすすめです。
        var seconds = 6f;
        var MatchedOrCanceled = false;

        while (true)
        {
            if (MatchedOrCanceled)
            {
                yield break;
            }

            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGetMatchmakingTicketSuccess, OnFailure);
            yield return new WaitForSeconds(seconds);
        }

        void OnGetMatchmakingTicketSuccess(GetMatchmakingTicketResult result)
        {
            switch (result.Status)
            {
                case "Matched":
                    MatchedOrCanceled = true;
                    matchingText.text = "対戦相手が見つかりました！";
                    _matchID = result.MatchId;
                    GetMatchInfo();
                    return;

                case "Canceled":
                    MatchedOrCanceled = true;
                    matchingText.text = "対戦相手が見つかりませんでした";
                    return;

                default:
                    matchingText.text = "マッチング中...";
                    return;
            }
        }
    }

    void OnFailure(PlayFabError error)
    {
        Debug.Log($"{error.ErrorMessage}");
    }

    private void GetMatchInfo()
    {
        var request = new GetMatchRequest
        {
            MatchId = _matchID,
            ReturnMemberAttributes = true,
            QueueName = _queueName,
        };
        PlayFabMultiplayerAPI.GetMatch(request, OnSuccess, OnError);

        void OnSuccess(GetMatchResult result)
        {
            foreach(var member in result.Members)
            {
                if (member.Entity.Id != PlayFabSettings.staticPlayer.EntityId)
                {
                    var data = (PlayFab.Json.JsonObject)member.Attributes.DataObject;
                    usersData.OpponentRating = double.Parse(data["Rating"].ToString());
                    break;
                }
            }

            // MatchIDで部屋を作って対戦
            photonManager.JoinOrCreateRoom(_matchID);
        }

        void OnError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
        }
    }
}