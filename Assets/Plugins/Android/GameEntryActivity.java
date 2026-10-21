package com.virkgames.platform;
import android.os.Bundle;
import android.view.KeyEvent;
import androidx.activity.OnBackPressedCallback;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerGameActivity;
/** Routes platform Back into game UI without requiring a keyboard device. */
public final class GameEntryActivity extends UnityPlayerGameActivity {
 @Override protected void onCreate(Bundle state) {
  super.onCreate(state);
  getOnBackPressedDispatcher().addCallback(this,new OnBackPressedCallback(true){
   @Override public void handleOnBackPressed(){UnityPlayer.UnitySendMessage("Game","OnAndroidBack","");}
  });
 }
 // GameActivity consumes keys through native input first. Route hardware Back
 // through the same dispatcher as gesture Back, once on release.
 @Override public boolean dispatchKeyEvent(KeyEvent event){
  if(event.getKeyCode()==KeyEvent.KEYCODE_BACK){
   if(event.getAction()==KeyEvent.ACTION_UP&&!event.isCanceled())getOnBackPressedDispatcher().onBackPressed();
   return true;
  }
  return super.dispatchKeyEvent(event);
 }
}
