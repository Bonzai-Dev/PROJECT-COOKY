using Godot;

public partial class Game : Node {
  private static Control userInterface;
  private static CanvasLayer hud;
  private static ColorRect fadeScreen;
  private static CanvasModulate fadePointer;
  private static Tween fadeScreenTween;
  private static Tween hudTween;
  
  public override void _Ready() {
    userInterface = GetTree().CurrentScene.GetNode<Control>("UI");
    hud = userInterface.GetNode<CanvasLayer>("Hud");
    fadeScreen = hud.GetNode<ColorRect>("Fade/Fade");
    fadePointer = hud.GetNode<CanvasModulate>("Pointer/Fade");
    fadeScreenTween = GetTree().CreateTween().BindNode(fadeScreen).SetTrans(Tween.TransitionType.Quint);
    hudTween = GetTree().CreateTween().BindNode(fadePointer).SetTrans(Tween.TransitionType.Quint);
  }

  public static Control GetUserInterface() {
    return userInterface;
  }
  
  public static void FadeIn() {
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 1), 1.0f
    );
    hudTween.TweenProperty(
      fadePointer, "color", new Color(0, 0, 0, 1), 1.0f
    );
  }

  public static void FadeOut() {
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 0), 1.0f
    );
    hudTween.TweenProperty(
      fadePointer, "color", new Color(255, 255, 255), 1.0f
    );
  }
}
