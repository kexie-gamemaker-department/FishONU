using FishONU.Network;
using FishONU.Player;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using R3;
using R3.Triggers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishONU.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("引用")][SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_InputField addressInputField;
        [SerializeField] private TMP_InputField portInputField;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_Text errorText;

        [SerializeField] private Button fullScreenToggleButton;
        [SerializeField] private TMP_Dropdown WindowSizeDropdown;

        [SerializeField] private BaseNetworkManager manager;
        [SerializeField] private TelepathyTransport portTransport;

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

            fullScreenToggleButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (Screen.fullScreen)
                    {
                        Screen.fullScreen = false;
                    }
                    else
                    {
                        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
                    }
                })
                .AddTo(ref d);

            WindowSizeDropdown.OnValueChangedAsObservable()
                .Subscribe(index =>
                {
                    if (Screen.fullScreen) return;
                    switch (index)
                    {
                        case 0:
                            Screen.SetResolution(2160, 1440, false);
                            break;
                        case 1:
                            Screen.SetResolution(1920, 1080, false);
                            break;
                        case 2:
                            Screen.SetResolution(1280, 720, false);
                            break;
                        case 3:
                            Screen.SetResolution(640, 360, false);
                            break;
                    }
                })
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

                    string host = string.IsNullOrWhiteSpace(addressInputField.text)
                        ? "localhost"
                        : addressInputField.text;
                    var portString = portInputField.text == "" ? "7777" : portInputField.text;

                    bool isPortValid = ushort.TryParse(portString, out ushort port);
                    if (!isPortValid)
                    {
                        errorText.text = "端口号无效。";
                        return;
                    }

                    string scheme = "kcp"; // 默认值
                    if (Transport.active is KcpTransport) scheme = "kcp";
                    else if (Transport.active is TelepathyTransport) scheme = "tcp4";
                    else if (Transport.active is SimpleWebTransport) scheme = "ws";

                    UriBuilder builder = new UriBuilder
                    {
                        Scheme = scheme,
                        Host = host,
                        Port = port
                    };

                    Debug.Log("尝试链接到 " + builder.Uri);

                    manager.StartClient(builder.Uri);
                })
                .AddTo(ref d);

            manager.OnClientDisconnectedAsObservable()
                .SelectMany(_ => // 多次断开时重新返回 Observable 进行计时
                {
                    errorText.text = "无法链接到服务器。";
                    Debug.Log("无法链接到服务器。");
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