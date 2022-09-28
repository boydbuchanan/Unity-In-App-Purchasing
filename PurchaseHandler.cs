using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

public class PurchaseHandler : MonoBehaviour {

    [Serializable]
    public class OnPurchaseCompletedEvent : UnityEvent<Product>
    {
    }

    [Serializable]
    public class OnPurchaseFailedEvent : UnityEvent<Product, PurchaseFailureReason>
    {
    }

    [Tooltip("Event fired after a successful purchase of this product.")]
    public OnPurchaseCompletedEvent onPurchaseComplete;

    [Tooltip("Event fired after a failed purchase of this product.")]
    public OnPurchaseFailedEvent onPurchaseFailed;

    public PurchaseProcessingResult OnPurchaseComplete(Product product){
        PurchaseProcessingResult result = PurchaseProcessingResult.Pending;

        result = product.definition.type == ProductType.Consumable ? PurchaseProcessingResult.Complete : PurchaseProcessingResult.Pending;
        Debug.Log($"Thanks for your purchase of '{product.metadata.localizedTitle}'!");
        
        onPurchaseComplete?.Invoke(product);

        return result;
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogError($"Failed purchase not correctly handled for product {product.definition.id}.");
        onPurchaseFailed?.Invoke(product, reason);
    }
}