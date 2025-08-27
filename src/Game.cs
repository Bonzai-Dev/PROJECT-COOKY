using Godot;

public partial class Game : Node {
  private static CanvasLayer userInterface;
  private static ColorRect fadeScreen;
  private static Tween fadeScreenTween;
  
  public override void _Ready() {
    userInterface = GetTree().CurrentScene.GetNode<CanvasLayer>("UI");
    fadeScreen = userInterface.GetNode<ColorRect>("Fade");
    fadeScreenTween = GetTree().CreateTween().BindNode(fadeScreen).SetTrans(Tween.TransitionType.Quint);
  }

  public static CanvasLayer GetUserInterface() {
    return userInterface;
  }
  
  public static void FadeIn() {
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 1), 1.0f
    );
  }

  public static void FadeOut() {
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 0), 1.0f
    );
  }
}
