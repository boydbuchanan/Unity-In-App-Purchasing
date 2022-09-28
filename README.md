# Unity In App Purchasing for UI Toolkit

**Configure In App Pruchasing Catalog**
1. Open Package Manager
1. Enable experimental packages as latest version of IAP may not be available
1. Install com.unity.purchasing version 4.4+
1. Add products to IAP Catalog
1. com.{company}.{app}.{product} (Use lowercase as stores will complain)

**Setup UI**

1. Copy into a folder in Assets (Ex: Assets/Scripts/Purchasing)
1. Update folder in PurchaseButton if different
1. Open UI Builder
1. Add Purchase Button from Project Tab -> Custom Controls (C#)
1. Copy & Paste product ID

**Customize**

Update UXML / USS

InAppPurchasingService is a singleton, convert to your own implementation.

Add custom handler logic to Purchase Handler

**Notes**

The process for deploying this to stores is convoluted. The stores need to verify the app with permissions and the butons before you can fully implement the products. You will need to build and deploy to stores, before you can fully add the product ids.

Add one test product and then build and deploy to stores. Once you have one verified and working, the rest will be easy to implement.