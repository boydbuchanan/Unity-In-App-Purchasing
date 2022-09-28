using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;


public class StoreListener : IStoreListener
{
    private static StoreListener instance;

    private Dictionary<string, PurchaseButton> activeButtons = new Dictionary<string, PurchaseButton>();

    private static bool unityPurchasingInitialized;

    protected IStoreController controller;

    protected IExtensionProvider extensions;

    public ConfigurationBuilder m_Builder;

    public ProductCatalog catalog;

    public static bool initializationComplete;

    public static StoreListener Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new StoreListener();
            }
            return instance;
        }
    }

    public IStoreController StoreController => controller;

    [RuntimeInitializeOnLoadMethod]
    public static void InitializePurchasingOnLoad()
    {
        if(!unityPurchasingInitialized){
            instance = new StoreListener();
            InitializePurchasing();
        }
    }

    public static void InitializePurchasing()
    {
        StandardPurchasingModule standardPurchasingModule = StandardPurchasingModule.Instance();
        standardPurchasingModule.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(standardPurchasingModule);
        IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, instance.catalog);
        instance.m_Builder = builder;
        UnityPurchasing.Initialize(instance, builder);
        unityPurchasingInitialized = true;
    }

    public T GetStoreConfiguration<T>() where T : IStoreConfiguration
    {
        return m_Builder.Configure<T>();
    }

    public T GetStoreExtensions<T>() where T : IStoreExtension
    {
        return extensions.GetExtension<T>();
    }

    private StoreListener()
    {
        catalog = ProductCatalog.LoadDefaultCatalog();
    }

    private static bool ShouldAutoInitUgs()
    {
        
        return instance.catalog.enableCodelessAutoInitialization;
    }

    public bool HasProductInCatalog(string productID)
    {
        return catalog.allProducts.Any(x => x.id == productID);
    }

    public ProductCatalogItem GetLocalProduct(string productID)
    {
        return catalog.allProducts.FirstOrDefault(x => x.id == productID);
    }

    public Product GetProduct(string productID)
    {
        if (controller != null && controller.products != null && !string.IsNullOrEmpty(productID))
        {
            return controller.products.WithID(productID);
        }
        
        return null;
    }

    public void AddButton(PurchaseButton button)
    {
        if(activeButtons.ContainsKey(button.ID)){
            activeButtons[button.ID] = button;
        }else{
            activeButtons.Add(button.ID, button);
        }
    }

    public void RemoveButton(PurchaseButton button)
    {
        activeButtons.Remove(button.ID);
    }

    public void InitiatePurchase(string productID)
    {
        if (controller == null)
        {
            Debug.LogError("Purchase failed because Purchasing was not initialized correctly");
            InAppPurchasingService.Instance.PurchaseHandler.OnPurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable);
        }
        else
        {
            controller.InitiatePurchase(productID);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        initializationComplete = true;
        this.controller = controller;
        this.extensions = extensions;
        foreach (PurchaseButton activeButton in activeButtons.Values)
        {
            activeButton.UpdateText();
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Purchasing failed to initialize. Reason: {error.ToString()}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        PurchaseProcessingResult result = InAppPurchasingService.Instance.PurchaseHandler.OnPurchaseComplete(e.purchasedProduct);

        return result;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        InAppPurchasingService.Instance.PurchaseHandler.OnPurchaseFailed(product, reason);
    }
}
