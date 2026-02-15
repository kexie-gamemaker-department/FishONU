using FishONU.Network;
using FishONU.Player;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_InputField addressInputField;
        [SerializeField] private TMP_InputField portInputField;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_Text errorText;

        [SerializeField] private BaseNetworkManager manager;

        private void Start()
        {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerDisplayName", IdentifierHelper.RandomIdentifier());

            var d = Disposable.CreateBuilder();

            playerNameInputField.OnValueChangedAsObservable()
                .Select(name => !string.IsNullOrEmpty(name))
                .SubscribeToInteractable(hostButton)
                .AddTo(ref d);

            playerNameInputField.OnValueChangedAsObservable()
                .Select(name => !string.IsNullOrEmpty(name))
                .SubscribeToInteractable(joinButton)
                .AddTo(ref d);

            hostButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    SavePlayerData();
                    manager.StartHost();
                })
                .AddTo(ref d);

            joinButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    SavePlayerData();
                    manager.networkAddress = string.IsNullOrEmpty(addressInputField.text) ? "localhost" : addressInputField.text;
                    manager.StartClient();
                })
                .AddTo(ref d);

            manager.OnClientDisconnectedAsObservable()
                .SelectMany(_ => // 多次断开时重新返回 Observable 进行计时
                {
                    errorText.text = "无法链接到服务器...";
                    return Observable.Timer(TimeSpan.FromSeconds(3)); // 返回一个3秒的计时器
                })
                .Subscribe(_ => errorText.text = "")
                .AddTo(ref d);


            d.RegisterTo(destroyCancellationToken);
        }

        private void SavePlayerData()
        {
            // TODO: 更好的序列化
            PlayerPrefs.SetString("PlayerDisplayName", playerNameInputField.text);
        }
    }
}
