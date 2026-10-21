using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using LocalGame.Data;
namespace LocalGame {
 [Serializable] public sealed class StoreConfiguration {public bool EnableGooglePlay;public string GooglePublicKeyBase64="";}
 public sealed class LocalStore:MonoBehaviour {
  StoreController controller;Profile profile;Func<bool> save;string sku;StoreConfiguration config;Product product;bool connecting;
  public string Status{get;private set;}="Purchases are unavailable in this local edition.";
  public string Price=>Ready?product.metadata.localizedPriceString:null;public bool Ready=>product!=null&&product.availableToPurchase&&config!=null&&config.EnableGooglePlay;
  public bool Owned=>profile!=null&&profile.Entitlements.Contains(sku);
  public void Setup(Profile p,Func<bool> persist,bool city){profile=p;save=persist;sku=city?"cc.skin.neon":"bb.cosmetic.noir";}
  public async Task Connect(){await Task.CompletedTask;
   if(connecting||controller!=null)return;var text=Resources.Load<TextAsset>("StoreConfiguration");
   if(!text){Status="The store is not enabled in this build.";return;}try{config=JsonUtility.FromJson<StoreConfiguration>(text.text);}catch(Exception){Status="Store configuration is invalid.";return;}
#if !UNITY_ANDROID || UNITY_EDITOR
   Status="Native store purchases require the Google Play edition. No fake store is used.";return;
#else
   if(config==null||!config.EnableGooglePlay||string.IsNullOrWhiteSpace(config.GooglePublicKeyBase64)){Status="The store is not configured. No purchase can begin.";return;}
   try{new UnityEngine.Purchasing.Security.CrossPlatformValidator(Convert.FromBase64String(config.GooglePublicKeyBase64),Application.identifier);}catch(Exception){Status="Store validation configuration is invalid. No purchase can begin.";return;}
   connecting=true;try{
    controller=UnityIAPServices.StoreController();
    controller.OnStoreConnected+=()=>controller.FetchProducts(new List<ProductDefinition>{new ProductDefinition(sku,ProductType.NonConsumable)});
    controller.OnStoreDisconnected+=failure=>{product=null;Status="Store connection unavailable. Your game remains playable.";};
    controller.OnProductsFetched+=products=>{product=products.FirstOrDefault(p=>p.definition.id==sku);Status=product==null?"This cosmetic is unavailable.":"Optional cosmetic. No purchase is needed to play.";controller.FetchPurchases();};
    controller.OnProductsFetchFailed+=failure=>{product=null;Status="The store could not load this product. Try again later.";};
    controller.OnPurchasesFetched+=orders=>{foreach(var order in orders.ConfirmedOrders)Deliver(order);foreach(var order in orders.PendingOrders)Pending(order);};
    controller.OnPurchasesFetchFailed+=failure=>Status="Purchase history unavailable. Nothing was changed.";
    controller.OnPurchasePending+=Pending;controller.OnPurchaseFailed+=failure=>Status="Purchase was not completed. Nothing was granted.";
    controller.OnPurchaseDeferred+=order=>Status="Awaiting platform approval. No content has been granted.";
    controller.OnPurchaseConfirmed+=order=>Status=order is ConfirmedOrder?"Cosmetic delivered and saved.":"Confirmation failed. The store can safely redeliver.";
    controller.OnAuthAccountChanged+=()=>{product=null;controller.FetchProducts(new List<ProductDefinition>{new ProductDefinition(sku,ProductType.NonConsumable)});};
    await controller.Connect();
   }catch(Exception){controller=null;product=null;Status="Could not connect to the store.";}finally{connecting=false;}
#endif
  }
  public bool Buy(){if(!Ready||Owned)return false;controller.PurchaseProduct(product);Status="Waiting for the platform…";return true;}
  public bool Restore(){if(controller==null){Status="Restore is unavailable until the real store is connected.";return false;}controller.RestoreTransactions((ok,message)=>Status=ok?"Restore requested. Verified entitlements will be saved.":"Restore did not complete. Nothing was changed.");return true;}
  void Pending(PendingOrder order){if(Deliver(order))controller.ConfirmPurchase(order);}
  bool Deliver(Order order){
#if UNITY_ANDROID && !UNITY_EDITOR
   try{
    if(order?.Info==null||order.CartOrdered.Items().Count!=1||order.CartOrdered.Items()[0].Product.definition.id!=sku)return false;
    var validator=new UnityEngine.Purchasing.Security.CrossPlatformValidator(Convert.FromBase64String(config.GooglePublicKeyBase64),Application.identifier);
    var receipts=validator.Validate(order.Info.Receipt);var valid=receipts.OfType<UnityEngine.Purchasing.Security.GooglePlayReceipt>().SingleOrDefault(r=>r.productID==sku&&r.transactionID==order.Info.TransactionID&&r.purchaseState==UnityEngine.Purchasing.Security.GooglePurchaseState.Purchased);
    if(valid==null||string.IsNullOrEmpty(valid.transactionID)){Status="Receipt could not be verified. Nothing was granted.";return false;}
    profile.GrantCosmetic(sku,valid.transactionID);if(!save()){Status="Save unavailable. Purchase is unconfirmed and can be redelivered.";return false;}return true;
   }catch(Exception){Status="Receipt verification failed. No purchase was confirmed.";return false;}
#else
   Status="This build cannot verify platform receipts.";return false;
#endif
  }
 }
}
