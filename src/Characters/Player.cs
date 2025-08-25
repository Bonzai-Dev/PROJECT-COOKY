using Godot;
using PhantomCamera;

public partial class Player : Character {
  [Export] private Node3D Camera;
  [Export] private float MouseSensitivity = 0.1f;

  private Vector2 mouseDelta;
  private Vector2 mouseRotation;
  [Export] private bool FreecamEnabled = true;

  public override void _Ready() {
    Input.SetMouseMode(Input.MouseModeEnum.Captured);
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseMotion mouseEvent)
      mouseDelta = mouseEvent.ScreenRelative * MouseSensitivity * (float)GetProcessDeltaTime();
    if (Input.IsActionJustPressed("movement_jump") && OnGround())
      moveDirection = new Vector3(moveDirection.X, JumpForce, moveDirection.Y);
  }

  public override void _Process(double delta) {
    var rightDirection = Model.GlobalTransform.Basis.X.Normalized();
    var forwardDirection = Model.GlobalTransform.Basis.Z.Normalized();
    
    mouseRotation = new Vector2(
      mouseRotation.X - mouseDelta.Y,
      mouseRotation.Y - mouseDelta.X
    );
    moveDirection = (
      rightDirection * Input.GetAxis("movement_left", "movement_right") +
      forwardDirection * Input.GetAxis("movement_forward", "movement_backward")
    ).Normalized();
    
    Camera.AsPhantomCamera3D().FollowTarget = (Node3D)FindChild("Camera");
    if (FreecamEnabled) {
      moveDirection = (
        moveDirection + Camera.GlobalTransform.Basis.Z.Normalized() *
        Input.GetAxis("movement_forward", "movement_backward")
      ).Normalized();
    
      if (FreecamEnabled) {
        Camera.AsPhantomCamera3D().FollowTarget = null;
        Camera.Position += moveDirection / 2;
      }
    }
    
    Camera.Rotation = new Vector3(Mathf.Clamp(mouseRotation.X, -Mathf.Pi / 2, Mathf.Pi / 2), mouseRotation.Y, 0);
    Model.Rotation = new Vector3(0, mouseRotation.Y, 0);
    mouseDelta = Vector2.Zero;
  }

  public override void _PhysicsProcess(double delta) {
    var slideDirection = SlideDirection();
    var forceDirection = (moveDirection + slideDirection).Normalized();

    if (!FreecamEnabled) {
      ApplyForce(forceDirection * Acceleration + new Vector3(0, moveDirection.Y * JumpForce, 0));
    }
    
    // Logic for when the player is in the air
    if (!OnGround())
      ApplyForce(new Vector3(-moveDirection.X, 0, -moveDirection.Z) * CounterForce);
    else
      ApplyForce(new Vector3(-LinearVelocity.X * Deceleration, 0, -LinearVelocity.Z * Deceleration));
    
    // Logic for adding friction to player to prevent sliding
    if (Moving())
      PhysicsMaterialOverride.Friction = 0;
    else
      PhysicsMaterialOverride.Friction = 0.5f;
    
    // Limiting velocity to speed
    if (new Vector2(LinearVelocity.X, LinearVelocity.Z).Length() >= Speed) {
      var normalizedVelocity = LinearVelocity.Normalized();
      LinearVelocity = new Vector3(normalizedVelocity.X * Speed, LinearVelocity.Y, normalizedVelocity.Z * Speed);
    }
  }
}
