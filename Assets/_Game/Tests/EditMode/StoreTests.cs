using System;
using NUnit.Framework;
using UnityEngine;
using LocalGame;
using LocalGame.Data;
public class StoreTests {
 [Test] public void MissingRealStoreConfigFailsClosedWithoutPricesOrGrants(){var obj=new GameObject("store-test");try{var service=obj.AddComponent<LocalStore>();var profile=new Profile();int saves=0;service.Setup(profile,()=>{saves++;return true;},true);service.Connect().GetAwaiter().GetResult();Assert.False(service.Ready);Assert.IsNull(service.Price);Assert.False(service.Buy());Assert.False(service.Restore());Assert.AreEqual(0,profile.Entitlements.Count);Assert.AreEqual(0,saves);}finally{UnityEngine.Object.DestroyImmediate(obj);}}
 [Test] public void PureDomainCosmeticDeliveryIsIdempotentFixtureOnly(){var profile=new Profile();Assert.True(profile.GrantCosmetic("cc.skin.neon","fixture-only-order"));Assert.False(profile.GrantCosmetic("cc.skin.neon","fixture-only-order"));Assert.AreEqual(1,profile.Entitlements.Count);Assert.AreEqual(3,profile.Theme);Assert.Throws<ArgumentException>(()=>profile.SetTheme(4));Assert.AreEqual(350,profile.Credits);Profile.Validate(profile);Assert.Throws<ArgumentException>(()=>profile.GrantCosmetic("unknown","fixture2"));Assert.Throws<ArgumentException>(()=>profile.GrantCosmetic("cc.skin.neon",""));}
}
