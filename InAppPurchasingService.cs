using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UIElements;

public class InAppPurchasingService : MonoBehaviour
{
    private static InAppPurchasingService _instance;
    public static InAppPurchasingService Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            var instances = FindObjectsOfType<InAppPurchasingService>();
            var count = instances.Length;

            if (count <= 0) {
                _instance = new GameObject($"{nameof(InAppPurchasingService)}").AddComponent<InAppPurchasingService>();
                return _instance;
            }
            if (count == 1)
                return _instance = instances[0];

            for (var i = 1; i < instances.Length; i++)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(instances[i]);
                }
                else
                {
                    Destroy(instances[i]);
                }
            }
            return _instance = instances[0];
        }
    }

    [SerializeField] public VisualTreeAsset PurchaseButtonTemplate;
    [SerializeField] public PurchaseHandler PurchaseHandler;

    public string informationText;

    const string k_Environment = "production";

    void Awake()
    {
        // Uncomment this line to initialize Unity Gaming Services.
        Initialize(OnSuccess, OnError);
    }

    void Initialize(Action onSuccess, Action<string> onError)
    {
        try
        {
            var options = new InitializationOptions().SetEnvironmentName(k_Environment);

            UnityServices.InitializeAsync(options).ContinueWith(task => onSuccess());
        }
        catch (Exception exception)
        {
            onError(exception.Message);
        }
    }

    void OnSuccess()
    {
        var text = "Congratulations!\nUnity Gaming Services has been successfully initialized.";
        informationText = text;
        Debug.Log(text);
        StoreListener.InitializePurchasingOnLoad();
    }

    void OnError(string message)
    {
        var text = $"Unity Gaming Services failed to initialize with error: {message}.";
        informationText = text;
        Debug.LogError(text);
    }

    void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var text =
                "Error: Unity Gaming Services not initialized.\n" +
                "To initialize Unity Gaming Services, open the file \"InitializeGamingServices.cs\" " +
                "and uncomment the line \"Initialize(OnSuccess, OnError);\" in the \"Awake\" method.";
            informationText = text;
            Debug.LogError(text);
        }
    }
}
