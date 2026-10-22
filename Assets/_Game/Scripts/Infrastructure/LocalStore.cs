using System;using System.Collections.Generic;using System.Linq;using System.Threading.Tasks;using System.Security.Cryptography;using System.Text;using UnityEngine;using UnityEngine.Purchasing;using LocalGame.Data;
namespace LocalGame {
 [Serializable] public sealed class StoreConfiguration {public bool EnableGooglePlay;public string GooglePublicKeyBase64="";}
 // Only this adapter crosses the platform purchase boundary. No Fake Store in a player build.
 public sealed class LocalStore:MonoBehaviour {
  StoreController controller;Profile profile;Func<bool> save;StoreConfiguration config;readonly Dictionary<string,Product> products=new Dictionary<string,Product>();bool connecting,inFlight,waitingApproval,disposed;string legacySku="cc.skin.neon";
  public event Action Changed;public string Status{get;private set;}="Google Play purchases are not connected in this build.";
  public string Price=>PriceFor(legacySku);public bool Ready=>ReadyFor(legacySku);public bool Owned=>profile!=null&&profile.Entitlements.Contains(legacySku);
  public void Setup(Profile p,Func<bool> persist,bool city=true){profile=p;save=persist;}
  void Notice(string text){if(disposed)return;Status=text;Changed?.Invoke();}
  public string PriceFor(string id)=>ReadyFor(id)?products[id].metadata.localizedPriceString:null;
  public bool HasOutstandingTransaction=>inFlight||waitingApproval;
  public bool ReadyFor(string id)=>!disposed&&!inFlight&&!waitingApproval&&controller!=null&&products.TryGetValue(id,out var product)&&product.availableToPurchase&&product.metadata!=null&&product.metadata.localizedPrice>0&&!string.IsNullOrEmpty(product.metadata.localizedPriceString);
  List<ProductDefinition> Definitions()=>CommerceCatalog.All.Select(p=>new ProductDefinition(p.Id,p.Consumable?ProductType.Consumable:ProductType.NonConsumable)).ToList();
  public async Task Connect(){await Task.CompletedTask;if(connecting||controller!=null)return;var text=Resources.Load<TextAsset>("StoreConfiguration");if(!text)return;try{config=JsonUtility.FromJson<StoreConfiguration>(text.text);}catch(Exception){Notice("Store configuration could not be read. Your game remains playable.");return;}
#if !UNITY_ANDROID || UNITY_EDITOR
   Notice("Purchases use Google Play in the Android edition. Nothing can be purchased on this laptop.");return;
#else
   if(config==null||!config.EnableGooglePlay||string.IsNullOrWhiteSpace(config.GooglePublicKeyBase64))return;
   try{new UnityEngine.Purchasing.Security.CrossPlatformValidator(Convert.FromBase64String(config.GooglePublicKeyBase64),Application.identifier);}catch(Exception){Notice("Google Play validation is not configured. No purchase can begin.");return;}
   connecting=true;Notice("Connecting to Google Play…");try{controller=UnityIAPServices.StoreController();controller.OnStoreConnected+=Connected;controller.OnStoreDisconnected+=Disconnected;controller.OnProductsFetched+=ProductsFetched;controller.OnProductsFetchFailed+=ProductsFailed;controller.OnPurchasesFetchFailed+=HistoryFailed;controller.OnPurchasesFetched+=History;controller.OnPurchasePending+=Pending;controller.OnPurchaseDeferred+=Deferred;controller.OnPurchaseFailed+=Failed;controller.OnPurchaseConfirmed+=Confirmed;controller.OnCheckEntitlement+=Entitled;controller.OnAuthAccountChanged+=AccountChanged;await controller.Connect();}catch(Exception){Detach();controller=null;products.Clear();Notice("Google Play could not connect. Try again later.");}finally{connecting=false;}
#endif
  }
  void Connected(){if(!disposed)controller.FetchProducts(Definitions());}
  void Disconnected(StoreConnectionFailureDescription failure){if(disposed)return;products.Clear();Notice("Google Play is offline. Core play and saved ownership remain available.");}
  void ProductsFetched(List<Product> items){if(disposed)return;products.Clear();foreach(var item in items)products[item.definition.id]=item;Notice("Prices loaded from Google Play.");controller.FetchPurchases();}
  void ProductsFailed(ProductFetchFailed failure){if(disposed)return;products.Clear();Notice("Google Play could not load prices. Nothing can be charged.");}
  void HistoryFailed(PurchasesFetchFailureDescription failure){Notice("Purchase history is unavailable. Existing ownership was preserved.");}
  void Deferred(DeferredOrder order){if(disposed)return;inFlight=false;waitingApproval=true;Notice("Awaiting platform approval. No content has been granted.");}
  void Failed(FailedOrder order){if(disposed)return;inFlight=waitingApproval=false;Notice("Purchase was canceled or could not complete. Nothing was granted.");}
  void Confirmed(Order order){if(disposed)return;inFlight=waitingApproval=false;Notice(order is ConfirmedOrder?"Purchase delivered and saved.":"Confirmation did not finish. Google Play can safely redeliver.");}
  void Entitled(Entitlement entitlement){if(!disposed&&entitlement.Status==EntitlementStatus.Unknown)Notice("Ownership could not be checked. Existing access was preserved.");}
  void AccountChanged(){if(disposed)return;products.Clear();inFlight=waitingApproval=false;controller.FetchProducts(Definitions());}
  void OnDestroy(){disposed=true;Detach();Changed=null;products.Clear();}
  void Detach(){if(controller==null)return;controller.OnStoreConnected-=Connected;controller.OnStoreDisconnected-=Disconnected;controller.OnProductsFetched-=ProductsFetched;controller.OnProductsFetchFailed-=ProductsFailed;controller.OnPurchasesFetchFailed-=HistoryFailed;controller.OnPurchasesFetched-=History;controller.OnPurchasePending-=Pending;controller.OnPurchaseDeferred-=Deferred;controller.OnPurchaseFailed-=Failed;controller.OnPurchaseConfirmed-=Confirmed;controller.OnCheckEntitlement-=Entitled;controller.OnAuthAccountChanged-=AccountChanged;}
  public bool Buy()=>Buy(legacySku);
  public bool Buy(string id){if(profile==null||!ReadyFor(id)||!CommerceCatalog.Eligible(profile,id))return false;inFlight=true;Notice("Waiting for the Google Play purchase sheet…");try{controller.PurchaseProduct(products[id]);return true;}catch(Exception){inFlight=false;Notice("The purchase sheet could not open. Nothing was granted.");return false;}}
  public bool Restore(){if(disposed||controller==null){Notice("Restore requires the Android edition connected to the same Google Play account.");return false;}controller.RestoreTransactions((ok,message)=>{if(disposed)return;Notice(ok?"Restore requested. Ownership will be checked before any change.":"Restore did not complete. Existing ownership was preserved.");if(ok)controller.FetchPurchases();});return true;}
  void Pending(PendingOrder order){if(disposed)return;inFlight=true;waitingApproval=false;if(Deliver(order,out _))controller.ConfirmPurchase(order);}
  void History(Orders orders){if(disposed)return;waitingApproval=orders.DeferredOrders.Count>0;if(orders.PendingOrders.Count>0)inFlight=true;else if(!waitingApproval)inFlight=false;var owned=new List<string>();bool verified=true;foreach(var order in orders.ConfirmedOrders){if(Deliver(order,out var sku)){if(!CommerceCatalog.Get(sku).Consumable)owned.Add(sku);}else verified=false;}foreach(var order in orders.PendingOrders){if(Deliver(order,out var sku)){if(!CommerceCatalog.Get(sku).Consumable)owned.Add(sku);controller.ConfirmPurchase(order);}else verified=false;}if(!verified){Notice("Some purchase history could not be verified. Existing ownership was preserved.");return;}string previous=JsonUtility.ToJson(profile);profile.ReconcilePurchases(owned,true);if(!save()){JsonUtility.FromJsonOverwrite(previous,profile);Notice("Could not save restored ownership. Try again before changing devices.");return;}Changed?.Invoke();}
  bool Deliver(Order order,out string sku){sku=null;if(disposed)return false;
#if UNITY_ANDROID && !UNITY_EDITOR
   try{if(order?.Info==null||order.CartOrdered.Items().Count!=1||order.CartOrdered.Items()[0].Quantity!=1)return false;sku=order.CartOrdered.Items()[0].Product.definition.id;var item=CommerceCatalog.Get(sku);var validator=new UnityEngine.Purchasing.Security.CrossPlatformValidator(Convert.FromBase64String(config.GooglePublicKeyBase64),Application.identifier);var receipts=validator.Validate(order.Info.Receipt);string wanted=sku;var valid=receipts.OfType<UnityEngine.Purchasing.Security.GooglePlayReceipt>().SingleOrDefault(r=>r.productID==wanted&&r.transactionID==order.Info.TransactionID&&r.purchaseState==UnityEngine.Purchasing.Security.GooglePurchaseState.Purchased);if(valid==null||string.IsNullOrEmpty(valid.transactionID)){Notice("The purchase receipt could not be verified. Nothing was granted.");return false;}string reference;using(var hash=SHA256.Create())reference=Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(valid.transactionID)));if(PurchaseDelivery.PersistVerified(profile,sku,reference,save))return true;Notice("Save unavailable. The purchase remains unconfirmed for safe redelivery.");return false;}catch(Exception){Notice("Purchase verification failed. No order was confirmed.");return false;}
#else
   Notice("This build cannot verify Google Play purchases.");return false;
#endif
  }
 }
}
