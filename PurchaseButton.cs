using System;
using Kitchen.Sync;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using UnityEditor;

[UnityEngine.Scripting.Preserve]
public class PurchaseButton : VisualElement
{
    private const string folder = "Assets/Scripts/Purchasing";

    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<PurchaseButton, UxmlTraits> { 
        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            // Load Uxml Template from GameObject Reference

            // Load data from UxmlTraits
            VisualElement root = base.Create(bag, cc);
            visualTree.CloneTree(root);
            // Load the item from the root
            PurchaseButton element = root.Q<PurchaseButton>();
            // Run initialization methods
            element.Init();
            element.UpdateText();
            return element;
        }
    }

    [UnityEngine.Scripting.Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlStringAttributeDescription ProductId = new() { name = "ProductId" };
        readonly UxmlEnumAttributeDescription<ButtonType> PurchaseType = new() { name = "Type" };
        readonly UxmlStringAttributeDescription IconClass = new() { name = "Icon" };
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }
        public override void Init( VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext )
        {
            base.Init( visualElement, attributes, creationContext );
            var element = visualElement as PurchaseButton;
            if (element != null)
            {
                VisualTreeAsset visualTree = InAppPurchasingService.Instance.PurchaseButtonTemplate;
                #if UNITY_EDITOR
                if(visualTree == null){
                    // If not set, then load directly from file
                    visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{folder}/PurchaseButton.uxml");
                    //StyleSheet asset = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{folder}/Purchase.uss");
                }
                #endif
                visualTree.CloneTree(element);

                //element.styleSheets.Add(stylesheet);
                element.ID = ProductId.GetValueFromBag( attributes, creationContext );
                element.buttonType = PurchaseType.GetValueFromBag( attributes, creationContext );
                element.IconClass = IconClass.GetValueFromBag( attributes, creationContext );
                
                element.Init();
                element.UpdateText();
            }
        }
    }

    Label Title;
    Label Description;
    Label Price;
    Button IAPButton;
    VisualElement Icon;

    string IconClass;

    public void Init(){
        Title = this.Q<Label>("PurchaseTitle");
        Description = this.Q<Label>("PurchaseDesc");
        Price = this.Q<Label>("PurchasePrice");
        Icon = this.Q<VisualElement>("PurchaseIcon");

        if(Icon != null && !Icon.ClassListContains(IconClass)){
            Icon.AddToClassList(IconClass);
        }
        IAPButton = this.Q<Button>("PurchaseButton");
        if(IAPButton != null)
            IAPButton.clicked += Clicked;

        OnEnable();
    }

    private void CheckProductId()
    {
        if (string.IsNullOrEmpty(ID))
        {
            Debug.LogError("IAPButton productId is empty");
        }
        if (!StoreListener.Instance.HasProductInCatalog(ID))
        {
            Debug.LogWarning("The product catalog has no product with the ID \"" + ID + "\"");
        }
    }

    public void Clicked(){
        if (buttonType == ButtonType.Purchase)
        {
            PurchaseProduct();
            CheckProductId();
        }
        else if (buttonType == ButtonType.Restore)
        {
            Restore();
        }
    }

    public enum ButtonType
    {
        Purchase,
        Restore
    }

    [HideInInspector]
    public string ID;

    [Tooltip("The type of this button, can be either a purchase or a restore button.")]
    public ButtonType buttonType = ButtonType.Purchase;

    [Tooltip("[Optional] Displays the localized title from the app store.")]
    public string titleText;

    [Tooltip("[Optional] Displays the localized description from the app store.")]
    public string descriptionText;

    [Tooltip("[Optional] Displays the localized price from the app store.")]
    public string priceText;

    private void OnEnable()
    {
        if (buttonType == ButtonType.Purchase)
        {
            StoreListener.Instance.AddButton(this);
        }
    }

    private void OnDisable()
    {
        if (buttonType == ButtonType.Purchase)
        {
            StoreListener.Instance.RemoveButton(this);
        }
    }

    private void PurchaseProduct()
    {
        if (buttonType == ButtonType.Purchase)
        {
            StoreListener.Instance.InitiatePurchase(ID);
        }
    }

    private void Restore()
    {
        if (buttonType == ButtonType.Restore)
        {
            if (Application.platform == RuntimePlatform.WSAPlayerX86 || Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerARM)
            {
                StoreListener.Instance.GetStoreExtensions<IMicrosoftExtensions>().RestoreTransactions();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.tvOS)
            {
                StoreListener.Instance.GetStoreExtensions<IAppleExtensions>().RestoreTransactions(OnTransactionsRestored);
            }
            else if (Application.platform == RuntimePlatform.Android && StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay)
            {
                StoreListener.Instance.GetStoreExtensions<IGooglePlayStoreExtensions>().RestoreTransactions(OnTransactionsRestored);
            }
            else
            {
                Debug.LogWarning(Application.platform.ToString() + " is not a supported platform for the restore button");
            }
        }
    }

    private void OnTransactionsRestored(bool success)
    {
    }

    internal void UpdateText() {
        if (Application.isEditor)
            UpdateEditorText();
        else
            UpdateRuntimeText();
    }
    
    internal void UpdateEditorText()
    {
        if(string.IsNullOrEmpty(ID))
            return;

        if(!StoreListener.Instance.HasProductInCatalog(ID))
            return;

        if(Title == null || Description == null || Price == null || IAPButton == null)
            Init();

        ProductCatalogItem product = StoreListener.Instance.GetLocalProduct(ID);
        if (product != null)
        {
            titleText = product.defaultDescription.Title;
            if(Title != null)
                Title.text = titleText;

            descriptionText = product.defaultDescription.Description;
            if(Description != null)
                Description.text = descriptionText;

            priceText = product.googlePrice.value.ToString();
            if(Price != null)
                Price.text = priceText;
        }
    }

    internal void UpdateRuntimeText()
    {
        if(string.IsNullOrEmpty(ID))
            return;

        if(!StoreListener.Instance.HasProductInCatalog(ID))
            return;

        if(Title == null || Description == null || Price == null || IAPButton == null)
            Init();

        Product product = StoreListener.Instance.GetProduct(ID);
        if (product != null)
        {
            titleText = product.metadata.localizedTitle;
            if(Title != null)
                Title.text = titleText;

            descriptionText = product.metadata.localizedDescription;
            if(Description != null)
                Description.text = descriptionText;

            priceText = product.metadata.localizedPriceString;
            if(Price != null)
                Price.text = priceText;
        }else{
            Debug.LogError($"Product Id does not exist in App Store for {Application.platform}");
        }
    }

}