using Godot;

public partial class Game : Node {
  private static Control userInterface;
  private static CanvasGroup postProcessingEffects;
  private static CanvasLayer hud;
  private static ColorRect fadeScreen;
  private static CanvasModulate fadePointer;
  private static SubViewport gameViewport;

  private static Tween fadeScreenTween;
  private static Tween hudTween;
  
  private static uint levelStartStamina;
  private static uint currentLevel = 1;
  
  private static PlayerMovement player;
  private static Node3D spawnPoint;
  
  private static AudioStreamPlayer vhsStart;
  private static AudioStreamPlayer vhsRewind;
  
  public override void _Ready() {
    userInterface = GetTree().CurrentScene.GetNode<Control>("UI");
    hud = userInterface.GetNode<CanvasLayer>("Hud");
    fadeScreen = hud.GetNode<ColorRect>("Fade/Fade");
    fadePointer = hud.GetNode<CanvasModulate>("Pointer/Fade");
    gameViewport = GetTree().CurrentScene.GetNode<SubViewport>("Game/GameViewport");
    postProcessingEffects = userInterface.GetNode<CanvasGroup>("PostProcessing");
    
    player = gameViewport.GetNode<PlayerMovement>("Player");
    spawnPoint = GetTree().CurrentScene.GetNode<Node3D>("SpawnPoint");

    vhsStart = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("VHSStart");
    vhsRewind = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("VHSRewind");
  }

  public static Control GetUserInterface() {
    return userInterface;
  }

  public static CanvasLayer GetHud() {
    return hud;
  }

  public static CanvasGroup GetPostProcessingEffects() {
    return postProcessingEffects;
  }
  
  public static void FadeIn(float duration = 1.0f) {
    fadeScreenTween = fadeScreen.CreateTween().BindNode(fadeScreen).SetTrans(Tween.TransitionType.Quint);
    hudTween = hud.GetTree().CreateTween().BindNode(fadePointer).SetTrans(Tween.TransitionType.Quint);
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 1), duration
    );
    hudTween.TweenProperty(
      fadePointer, "color", new Color(0, 0, 0, 1), duration
    );
  }

  public static void FadeOut(float duration = 1.0f) {
    fadeScreenTween = fadeScreen.CreateTween().BindNode(fadeScreen).SetTrans(Tween.TransitionType.Quint);
    hudTween = hud.GetTree().CreateTween().BindNode(fadePointer).SetTrans(Tween.TransitionType.Quint);
    fadeScreenTween.TweenProperty(
      fadeScreen, "color", new Color(0, 0, 0, 0), duration
    );
    hudTween.TweenProperty(
      fadePointer, "color", new Color(255, 255, 255), duration
    );
  }

  public static void RestartLevel() {
    vhsStart.Stop();
    vhsRewind.Play();
    FadeIn(2);
    fadeScreenTween.TweenCallback(Callable.From(() => {
      player.Position = spawnPoint.Position;
      vhsStart.Play(); 
      FadeOut();
    }));
  }
  
  public static void NextLevel() {
    currentLevel++;
  }
}
