// Copyright (c) coherence ApS.
// See the license file in the package root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coherence.Cloud;
using Coherence.Connection;
using Coherence.Toolkit;
using Coherence.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Coherence.Samples.WorldDialog
{
    using Runtime;
    using UnityEngine.EventSystems;

    public class WorldDialogUI : MonoBehaviour
    {
        #region References
        [Header("References")]
        public GameObject connectDialog;
        public GameObject disconnectDialog;
        public GameObject noWorldsObject;
        public GameObject loadingSpinner;
        public Button refreshWorldsButton;
        public Button joinButton;
        public ConnectDialogWorldView templateWorldView;
        public Text worldTitleText;
        public GameObject popupDialog;
        public Text popupText;
        public Text popupTitleText;
        public Button popupDismissButton;
        #endregion

        private CoherenceBridge bridge;
        private PlayerAccount playerAccount;
        private IReadOnlyList<WorldData> availableCloudWorlds = new List<WorldData>();
        private Coroutine cloudServiceReady;
        private string initialWorldTitle;
        private WorldData localWorld;
        private Coroutine localWorldRefresher;
        private ListView worldsListView;

        private WorldsService CloudWorlds => bridge.CloudService?.Worlds;

        #region Unity Events
        private void OnEnable()
        {
            if (!bridge && !CoherenceBridgeStore.TryGetBridge(gameObject.scene, out bridge))
            {
                Debug.LogError($"{nameof(CoherenceBridge)} required on the scene.\n" +
                               "Add one via 'GameObject > coherence > Coherence Bridge'.");
                return;
            }

            if (!FindAnyObjectByType<EventSystem>())
            {
                Debug.LogError($"{nameof(EventSystem)} required on the scene.\n" +
                               "Add one via 'GameObject > UI > Event System'.");
            }

            bridge.onConnected.AddListener(_ => UpdateDialogsVisibility());
            bridge.onDisconnected.AddListener((_, _) => UpdateDialogsVisibility());
            bridge.onConnectionError.AddListener(OnConnectionError);
            noWorldsObject.SetActive(true);
            joinButton.interactable = false;

            if (!string.IsNullOrEmpty(RuntimeSettings.Instance.ProjectID))
            {
                cloudServiceReady = StartCoroutine(WaitForCloudService());
            }
            else
            {
                refreshWorldsButton.gameObject.SetActive(false);
            }

            localWorldRefresher = StartCoroutine(LocalWorldRefresher());
            UpdateDialogsVisibility();
        }

        private void OnDisable()
        {
            if (localWorldRefresher != null)
            {
                StopCoroutine(localWorldRefresher);
            }

            if (cloudServiceReady != null)
            {
                StopCoroutine(cloudServiceReady);
            }
        }

        private void Awake()
        {
            if (SimulatorUtility.IsSimulator)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy() => playerAccount?.Dispose();

        private void Start()
        {
            initialWorldTitle = worldTitleText.text;
            refreshWorldsButton.onClick.AddListener(RefreshWorlds);
            joinButton.onClick.AddListener(OnClickJoin);
            popupDismissButton.onClick.AddListener(HideError);

            popupDialog.SetActive(false);
            templateWorldView.gameObject.SetActive(false);
            worldsListView = new ListView
            {
                Template = templateWorldView,
                onSelectionChange = view =>
                {
                    joinButton.interactable = view != default && view.WorldData.WorldId != default(WorldData).WorldId;
                }
            };

            if (bridge.PlayerAccountAutoConnect is not CoherenceBridgePlayerAccount.AutoLoginAsGuest)
            {
                ConnectToCoherenceCloud();
            }
        }
        #endregion

        #region Cloud Requests
        private void ConnectToCoherenceCloud()
        {
            PlayerAccount.OnMainChanged += OnMainPlayerAccountChanged;
            OnMainPlayerAccountChanged(PlayerAccount.Main);
            CoherenceCloud.LoginAsGuest().OnFail(error =>
            {
                var errorMessage = $"Logging in to coherence Cloud Failed.\n{error}";
                ShowError("Logging in Failed", errorMessage);
                Debug.LogError(errorMessage);
            });


            void OnMainPlayerAccountChanged(PlayerAccount mainPlayerAccount)
            {
                if (mainPlayerAccount is null)
                {
                    return;
                }

                playerAccount = mainPlayerAccount;
                PlayerAccount.OnMainChanged -= OnMainPlayerAccountChanged;
                if (bridge.PlayerAccountAutoConnect is CoherenceBridgePlayerAccount.None)
                {
                    bridge.CloudService = mainPlayerAccount.Services;
                }
            }
        }

        private IEnumerator WaitForCloudService()
        {
            ShowLoadingState();

            while (CloudWorlds is not { IsLoggedIn : true })
            {
                yield return null;
            }

            HideLoadingState();

            RefreshWorlds();
            cloudServiceReady = null;
        }

        private void RefreshWorlds()
        {
            if (CloudWorlds is { IsLoggedIn : true })
            {
                ShowLoadingState();
                CloudWorlds.FetchWorlds(OnWorldsFetched);
            }
        }
        #endregion

        #region Request Callbacks
        private void OnClickJoin()
        {
            ShowLoadingState();
            bridge.JoinWorld(worldsListView.Selection.WorldData);
        }

        private void OnWorldsFetched(RequestResponse<IReadOnlyList<WorldData>> requestResponse)
        {
            HideLoadingState();

            if (requestResponse.Status != RequestStatus.Success)
            {
                var errorMessage = GetErrorFromResponse(requestResponse);
                ShowError("Error fetching worlds", errorMessage);
                Debug.LogException(requestResponse.Exception);
                return;
            }

            availableCloudWorlds = requestResponse.Result;
            RefreshWorldsListView();
        }

        public void Disconnect()
        {
            bridge.Disconnect();
        }
        #endregion

        #region Local World
        private IEnumerator LocalWorldRefresher()
        {
            while (true)
            {
                var task = ReplicationServerUtils.PingHttpServerAsync(RuntimeSettings.Instance.LocalHost,
                    RuntimeSettings.Instance.WorldsAPIPort);
                yield return new WaitUntil(() => task.IsCompleted);

                var result = task.Result;

                var lastWorld = localWorld;
                localWorld = result ? WorldData.GetLocalWorld(RuntimeSettings.Instance.LocalHost) : default;

                if (lastWorld.WorldId != localWorld.WorldId)
                {
                    RefreshWorldsListView();
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
        #endregion

        #region Error Handling
        private void ShowError(string title, string message = "Unknown Error")
        {
            popupDialog.SetActive(true);
            popupTitleText.text = title;
            popupText.text = message;
        }

        private void HideError()
        {
            popupDialog.SetActive(false);
        }

        private void OnConnectionError(CoherenceBridge bridge, ConnectionException connectionException)
        {
            HideLoadingState();

            Debug.LogException(connectionException);
            var errorMessage = GetConnectionError(connectionException);
            ShowError("Connection error", errorMessage);
        }

        private static string GetConnectionError(ConnectionException connectionException)
        {
            return connectionException switch
            {
                ConnectionClosedException closedException => "Connection closed unexpectedly.",
                ConnectionTimeoutException timeoutException => $"Connection timed out after {timeoutException.After:g}.",
                ConnectionDeniedException deniedException => $"Connection denied: {deniedException.CloseReason}.",
                _ => connectionException.Message,
            };
        }

        private static string GetErrorFromResponse<T>(RequestResponse<T> requestResponse)
        {
            if (requestResponse.Exception is not RequestException requestException)
            {
                return default;
            }

            return requestException.ErrorCode switch
            {
                ErrorCode.InvalidCredentials => "Invalid authentication credentials, please login again.",
                ErrorCode.TooManyRequests => "Too many requests. Please try again in a moment.",
                ErrorCode.ProjectNotFound => "Project not found. Please check that the runtime key is properly setup.",
                ErrorCode.SchemaNotFound => "Schema not found. Please check if the schema currently used by the project matches the one used by the replication server.",
                ErrorCode.RSVersionNotFound => "Replication server version not found. Please check that the version of the replication server is valid.",
                ErrorCode.SimNotFound => "Simulator not found. Please check that the slug and the schema are valid and that the simulator has been uploaded.",
                ErrorCode.MultiSimNotListening => "The multi-room simulator used for this room is not listening on the required ports. Please check your multi-room sim setup.",
                ErrorCode.RoomsSimulatorsNotEnabled => "Simulator not enabled. Please make sure that simulators are enabled in the coherence Dashboard.",
                ErrorCode.RoomsSimulatorsNotUploaded => "Simulator not uploaded. You can use the coherence Hub to build and upload Simulators.",
                ErrorCode.RoomsVersionNotFound => "Version not found. Please make sure that client uses the correct 'sim-slug'.",
                ErrorCode.RoomsSchemaNotFound => "Schema not found. Please check if the schema currently used by the project matches the one used by the replication server.",
                ErrorCode.RoomsRegionNotFound => "Region not found. Please make sure that the selected region is enabled in the Dev Portal.",
                ErrorCode.RoomsInvalidTagOrKeyValueEntry => "Validation of tag and key/value entries failed. Please check if number and size of entries is within limits.",
                ErrorCode.RoomsCCULimit => "Room ccu limit for project exceeded.",
                ErrorCode.RoomsNotFound => "Room not found. Please refresh room list.",
                ErrorCode.RoomsInvalidSecret => "Invalid room secret. Please make sure that the secret matches the one received on room creation.",
                ErrorCode.RoomsInvalidMaxPlayers => "Room Max Players must be a value between 1 and the upper limit configured on the project dashboard.",
                ErrorCode.InvalidMatchMakingConfig => "Invalid matchmaking configuration. Please make sure that the matchmaking feature was properly configured in the Dev Portal.",
                ErrorCode.ClientPermission => "The client has been restricted from accessing this feature. Please check the game services settings on the Dev Portal.",
                ErrorCode.CreditLimit => "Monthly credit limit exceeded. Please check your organization credit usage in the Dev Portal.",
                ErrorCode.InDeployment => "One or more online resources are currently being provisioned. Please retry the request.",
                ErrorCode.FeatureDisabled => "Requested feature is disabled, make sure you enable it in the Game Services section of your coherence Dashboard.",
                ErrorCode.InvalidRoomLimit => "Room max players limit must be between 1 and 100.",
                ErrorCode.LobbyInvalidAttribute => "A specified Attribute is invalid.",
                ErrorCode.LobbyNameTooLong => "Lobby name must be shorter than 64 characters.",
                ErrorCode.LobbyTagTooLong => "Lobby tag must be shorter than 16 characters.",
                ErrorCode.LobbyNotFound => "Requested Lobby wasn't found.",
                ErrorCode.LobbyAttributeSizeLimit => "A specified Attribute has surpassed the allowed limits. Lobby limit: 2048. Player limit: 256. Attribute size is calculated off key length + value length of all attributes combined.",
                ErrorCode.LobbyNameAlreadyExists => "A lobby with this name already exists.",
                ErrorCode.LobbyRegionNotFound => "Specified region for this Lobby wasn't found.",
                ErrorCode.LobbyInvalidSecret => "Invalid secret specified for lobby.",
                ErrorCode.LobbyFull => "This lobby is currently full.",
                ErrorCode.LobbyActionNotAllowed => "You're not allowed to perform this action on the lobby.",
                ErrorCode.LobbyInvalidFilter => "The provided filter is invalid. You can use Filter.ToString to debug the built filter you're sending.",
                ErrorCode.LobbyNotCompatible => "Schema not found. Please check if the schema currently used by the project matches the one used by the replication server.",
                ErrorCode.LobbySimulatorNotEnabled => "Simulator not enabled. Please make sure that simulators are enabled in the coherence Dashboard.",
                ErrorCode.LobbySimulatorNotUploaded => "Simulator not uploaded. You can use the coherence Hub to build and upload Simulators.",
                ErrorCode.LobbyLimit => "You cannot join more than three lobbies simultaneously.",
                ErrorCode.LoginInvalidUsername => "Username given is invalid. Only alphanumeric, dashes and underscore characters are allowed. It must start with a letter and end with a letter/number. No double dash/underscore characters are allowed (-- or __).",
                ErrorCode.LoginInvalidPassword => "Password given is invalid. Password cannot be empty.",
                ErrorCode.RestrictedModeCapReached => "Total player capacity for restricted mode server reached.",
                ErrorCode.LoginDisabled => "This authentication method is disabled.",
                ErrorCode.InvalidConfig => "This authentication method has not been configured properly and thus can not be used.",
                ErrorCode.LoginInvalidApp => "The provided App ID is invalid.",
                ErrorCode.OneTimeCodeExpired => "The one-time code has already expired.",
                ErrorCode.OneTimeCodeNotFound => "The one-time code was not found.",
                ErrorCode.LoginNotFound => "No player account has been linked to the authentication method that was used.",
                _ => requestException.Message,
            };
        }
        #endregion

        #region Update UI
        private void RefreshWorldsListView()
        {
            var allWorlds = availableCloudWorlds.ToList();
            if (localWorld.WorldId != default(WorldData).WorldId)
            {
                allWorlds.Add(localWorld);
            }

            noWorldsObject.SetActive(allWorlds.Count == 0);

            worldsListView.SetSource(allWorlds);
            worldTitleText.text = $"{initialWorldTitle} ({allWorlds.Count})";
        }

        private void UpdateDialogsVisibility()
        {
            HideLoadingState();
            connectDialog.SetActive(!bridge.IsConnected);
            disconnectDialog.SetActive(bridge.IsConnected);
        }

        private void HideLoadingState()
        {
            loadingSpinner.SetActive(false);
            joinButton.interactable = worldsListView != null && worldsListView.Selection != default
                                                             && worldsListView.Selection.WorldData.WorldId != default(WorldData).WorldId;
        }

        private void ShowLoadingState()
        {
            loadingSpinner.SetActive(true);
            joinButton.interactable = false;
            noWorldsObject.SetActive(false);
        }
        #endregion
    }

    internal class ListView
    {
        public ConnectDialogWorldView Template;
        public Action<ConnectDialogWorldView> onSelectionChange;

        public ConnectDialogWorldView Selection
        {
            get => selection;
            set
            {
                if (selection != value)
                {
                    selection = value;
                    lastSelectedId = selection == default ? default : selection.WorldData.WorldId;
                    onSelectionChange?.Invoke(Selection);
                    foreach (var viewRow in Views)
                    {
                        viewRow.IsSelected = selection == viewRow;
                    }
                }
            }
        }

        public List<ConnectDialogWorldView> Views { get; }
        private ConnectDialogWorldView selection;
        private ulong lastSelectedId;

        public ListView(int capacity = 50)
        {
            Views = new List<ConnectDialogWorldView>(capacity);
        }

        public void SetSource(IReadOnlyList<WorldData> dataSource)
        {
            Selection = default;
            Clear();

            if (dataSource.Count <= 0)
            {
                return;
            }

            var sortedData = dataSource.ToList();
            sortedData.Sort((worldA, worldB) =>
            {
                var strCompare = String.CompareOrdinal(worldA.Name, worldB.Name);
                if (strCompare != 0)
                {
                    return strCompare;
                }

                return (int)(worldA.WorldId - worldB.WorldId);
            });

            foreach (var data in sortedData)
            {
                var view = MakeViewItem(data);
                Views.Add(view);
                if (data.WorldId == lastSelectedId)
                {
                    Selection = view;
                }
            }
        }

        private ConnectDialogWorldView MakeViewItem(WorldData data, bool isSelected = false)
        {
            ConnectDialogWorldView view = Object.Instantiate(Template, Template.transform.parent);
            view.WorldData = data;
            view.IsSelected = isSelected;
            view.OnClick = () => Selection = view;
            view.gameObject.SetActive(true);
            return view;
        }

        public void Clear()
        {
            Selection = default;
            foreach (var view in Views)
            {
                Object.Destroy(view.gameObject);
            }
            Views.Clear();
        }
    }
}
