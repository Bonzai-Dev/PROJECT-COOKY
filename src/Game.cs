using Godot;

public partial class Game : Node {
  private static Control userInterface;
  private static CanvasGroup postProcessingEffects;
  private static CanvasLayer hud;
  private static CanvasLayer menu;
  private static CanvasLayer endScreen;
  private static ColorRect fadeScreen;
  private static CanvasModulate fadePointer;
  private static SubViewport gameViewport;
  private static RichTextLabel levelsPlayed;
  private static Button playButton;
  private static Button exitButton;
  private static RichTextLabel timer;
  
  private static Node3D currentLevelMap;
  private static Node3D cookySpawn;
  private static RigidBody3D cookyPlate;
  private static bool playingTween;
  private static Area3D nextLevelCollider;
  
  private static Tween fadeScreenTween;
  private static Tween hudTween;
  
  private static float levelStartStamina;
  private static uint maxLevels = 4;
  private static uint currentLevel = 1;
  
  private static PlayerMovement player;
  private static Node3D spawnPoint;

  private static AudioStreamPlayer vhsEject;
  private static AudioStreamPlayer vhsStart;
  private static AudioStreamPlayer vhsRewind;
  private static AudioStreamPlayer music;
  
  private static Node mainScene;
  private static Node menuScene;

  private bool quitting;

  public enum GameState {
    MENU,
    PLAYING,
    END
  }
  
  public static GameState CurrentState {get; private set;}

  public override void _Ready() {
    userInterface = GetTree().CurrentScene.GetNode<Control>("UI");
    hud = userInterface.GetNode<CanvasLayer>("Hud");
    menu = userInterface.GetNode<CanvasLayer>("Menu");
    endScreen = userInterface.GetNode<CanvasLayer>("End");
    fadeScreen = userInterface.GetNode<ColorRect>("Effects/Fade");
    postProcessingEffects = userInterface.GetNode<CanvasGroup>("PostProcessing");
    timer = userInterface.GetNode<RichTextLabel>("Effects/Timer");
    
    vhsEject = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("VHSEject");
    vhsStart = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("VHSStart");
    vhsRewind = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("VHSRewind");
    music = GetTree().CurrentScene.GetNode<AudioStreamPlayer>("Music");
    
    fadePointer = hud.GetNode<CanvasModulate>("Pointer/Fade");
    gameViewport = GetTree().CurrentScene.GetNode<SubViewport>("Game/GameViewport");
    levelsPlayed = userInterface.GetNode<RichTextLabel>("Effects/Level");
    levelsPlayed.Text = $"{currentLevel - 1}/4 Footage Played";

    CurrentState = GameState.MENU;
    HideCanvasLayer(hud);
    HideCanvasLayer(endScreen);
    playButton = menu.GetNode<Button>("Play");
    exitButton = endScreen.GetNode<Button>("Exit");
    playButton.Pressed += () => {
      if (playingTween) return;
      playingTween = true;
      vhsStart.Stop();
      vhsRewind.Play();
      FadeIn();
      fadeScreenTween.TweenCallback(Callable.From(() => {
        playingTween = false;
        CurrentState = GameState.PLAYING;
        vhsStart.Play();
        music.Play();
        
        ShowCanvasLayer(hud);
        HideCanvasLayer(menu);
        FadeOut();
      }));

      exitButton.Pressed += () => {
        FadeIn();
        vhsStart.Stop();
        vhsEject.Play();
        quitting = true;
      };
    };
    
    player = gameViewport.GetNode<PlayerMovement>("Player");
    spawnPoint = GetTree().CurrentScene.GetNode<Node3D>("SpawnPoint");
    
    nextLevelCollider = GetTree().CurrentScene.GetNode<Area3D>("Exit");
    nextLevelCollider.BodyShapeEntered += NextLevel;
    cookySpawn = GetTree().CurrentScene.GetNode<Node3D>("CookySpawn");

    levelStartStamina = 100;
    cookyPlate = null;
    SpawnLevel(currentLevel);
    SpawnCooky();
  }

  public override void _Process(double delta) {
    if (CurrentState == GameState.END) {
      music.PitchScale = Mathf.Lerp(music.PitchScale, 0, (float)delta);
      if (music.PitchScale < 0.01f)
        music.Stop();
    }

    if (quitting && !vhsEject.Playing) {
      GetTree().Quit();
    }

    if (CurrentState == GameState.PLAYING) {
      var timeInSeconds = Time.GetTicksMsec() / 1000;
      var minutes = timeInSeconds / 60 % 60;
      var seconds =  timeInSeconds % 60;
      float milliseconds = Time.GetTicksMsec() % 1000;
      timer.Text = $"1978/03/06 {minutes:00}:{seconds:00}.{milliseconds:00}";
    }
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
    if (playingTween) return;
    playingTween = true;
    
    vhsStart.Stop();
    vhsRewind.Play();
    FadeIn(2);
    fadeScreenTween.TweenCallback(Callable.From(() => {
      cookyPlate.Free();
      cookyPlate = null;
      SpawnCooky();
      
      player.Position = spawnPoint.Position;
      player.currentOverTime = 0;
      player.currentStamina = levelStartStamina;
      vhsStart.Play(); 
      FadeOut();
      playingTween = false;
    }));
  }
  
  private static void NextLevel(Rid bodyRid, Node3D body, long bodyShapeIndex, long localShapeIndex) {
    if (currentLevel + 1 > maxLevels) {
      if (playingTween) return;
      playingTween = true;

      FadeIn();
      fadeScreenTween.TweenCallback(Callable.From(() => {
        FadeOut();
        levelsPlayed.Text = "4/4 Footage Played";
        Input.MouseMode = Input.MouseModeEnum.Visible;
        HideCanvasLayer(hud);
        ShowCanvasLayer(endScreen);
        CurrentState = GameState.END;
        playingTween = false;
      }));
      return;
    }
    
    if (playingTween) return;
    playingTween = true;
    
    FadeIn();
    levelStartStamina = player.currentStamina;
    currentLevel++;
    vhsStart.Stop();
    vhsRewind.Play();
    fadeScreenTween.TweenCallback(Callable.From(() => {
      levelsPlayed.Text = $"{currentLevel - 1}/4 Footage Played";
      cookyPlate.Free();
      cookyPlate = null;
      SpawnCooky();

      RemoveCurrentLevel();
      SpawnLevel(currentLevel);
      
      player.Position = spawnPoint.Position;
      vhsStart.Play();
      FadeOut();
      playingTween = false;
    }));
  }

  private static void RemoveCurrentLevel() {
    currentLevelMap.Free();
  }
  
  private static void SpawnLevel(uint level) {
    var scene = GD.Load<PackedScene>($"res://assets/nodes/levels/level_{level}.tscn").Instantiate();
    scene.Name = "Level" + level;
    currentLevelMap = (Node3D)scene;
    gameViewport.AddChild(scene, true);
    GD.Print($"Loading level {"res://assets/nodes/levels/level_" + level}"); 
  }
  
  private static void SpawnCooky() {
    if (cookyPlate == null) {
      var scene = GD.Load<PackedScene>("res://assets/nodes/cookie_plate.tscn").Instantiate();
      scene.Name = "CookyPlate";
      cookyPlate = (RigidBody3D)scene;
      cookyPlate.Position = cookySpawn.Position;
      gameViewport.AddChild(cookyPlate, true);
    }
  }

  private static void HideCanvasLayer(CanvasLayer layer) {
    foreach (var item in layer.GetChildren()) {
      if (item is CanvasLayer) {
        var layerChild = (CanvasLayer)item;
        layerChild.Visible = false;
      }
      else {
        var layerChild = (Control)item;
        layerChild.Visible = false;
      }
    }
  }
  
  private static void ShowCanvasLayer(CanvasLayer layer) {
    foreach (var item in layer.GetChildren()) {
      if (item is CanvasLayer) {
        var layerChild = (CanvasLayer)item;
        layerChild.Visible = true;
      }
      else {
        var layerChild = (Control)item;
        layerChild.Visible = true;
      }
    }
  }
}
