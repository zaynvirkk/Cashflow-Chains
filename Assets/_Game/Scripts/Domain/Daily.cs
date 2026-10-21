using System;
namespace LocalGame.Data {
 public static class DailyPuzzle {
  public static int Level(DateTime utc){if(utc.Kind!=DateTimeKind.Utc)throw new ArgumentException("UTC required");long day=(utc.Date-new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc)).Days;return (int)(((day*37+17)%72+72)%72);}
  public static string Label(DateTime utc)=>utc.ToString("dd MMM yyyy")+" · UTC";
 }
}
