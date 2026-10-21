using System;using LocalGame.Data;using UnityEngine;
namespace LocalGame {
 // The platform adapter must verify the signed receipt before entering this transaction.
 public static class PurchaseDelivery {
  public static bool PersistVerified(Profile profile,string sku,string reference,Func<bool> persist){string before=JsonUtility.ToJson(profile);try{profile.ApplyPurchase(sku,reference);Profile.Validate(profile);if(persist())return true;}catch(Exception){}JsonUtility.FromJsonOverwrite(before,profile);return false;}
 }
}
