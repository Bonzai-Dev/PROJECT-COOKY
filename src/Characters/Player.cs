using Godot;
using PhantomCamera;

public partial class Player : Character {
  [Export] private Node3D Camera;
  [Export] private float MouseSensitivity = 0.1f;
  [Export] private float GrabRange = 5f;
  [Export] private bool FreecamEnabled;
  
  private Node3D head;
  private RayCast3D lookatRaycast;
  private Vector2 mouseDelta;
  private Vector2 mouseRotation;
  private RigidBody3D grabbedItem;

  public override void _Ready() {
    Input.SetMouseMode(Input.MouseModeEnum.Captured);
    head = GetNode<Node3D>("Head");
    lookatRaycast = head.GetNode<RayCast3D>("Lookat");
    lookatRaycast.AddException(this);
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseMotion mouseEvent)
      mouseDelta = mouseEvent.ScreenRelative * MouseSensitivity * (float)GetProcessDeltaTime();
    if (Input.IsActionJustPressed("movement_jump") && OnGround())
      moveDirection = new Vector3(moveDirection.X, JumpForce, moveDirection.Y);
    if (Input.IsActionJustPressed("pickup")) {
      if (grabbedItem == null)
        PickupItem(ItemInSight());
      else
        DropItem();
    }
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
    
    Camera.AsPhantomCamera3D().FollowTarget = head;
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
    head.Rotation = Camera.Rotation;
    lookatRaycast.TargetPosition = new Vector3(0, GrabRange, 0);
    mouseDelta = Vector2.Zero;
  }

  public override void _PhysicsProcess(double delta) {
    if (FreecamEnabled) return;

    if (grabbedItem != null) {
      var rayEnd = lookatRaycast.ToGlobal(lookatRaycast.TargetPosition);
      grabbedItem.SetLinearVelocity((rayEnd - grabbedItem.GlobalTransform.Origin) * 8);
    }
    
    var slideDirection = SlideDirection();
    var forceDirection = (moveDirection + slideDirection).Normalized();
    var appliedForce = new Vector3(
      forceDirection.X * Acceleration, 0, forceDirection.Z * Acceleration
    );
    
    ApplyForce(appliedForce);
    
    if (!OnGround())
      ApplyForce(new Vector3(-moveDirection.X, 0, -moveDirection.Z) * CounterForce);
    else {
      ApplyForce(new Vector3(0, moveDirection.Y * JumpForce, 0));
      ApplyForce(new Vector3(-LinearVelocity.X * Deceleration, 0, -LinearVelocity.Z * Deceleration));
    }
    
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

  private void PickupItem(RigidBody3D item) {
    if (item == null) return;
    grabbedItem = item;
    grabbedItem.Rotation = new Vector3(0, Camera.Rotation.Y, 0);
    grabbedItem.AxisLockAngularX = true;
    grabbedItem.AxisLockAngularY = true;
    grabbedItem.AxisLockAngularZ = true;
    GD.Print("Player grabbed ", item.Name);
  }

  private void DropItem() {
    GD.Print("Player dropped ", grabbedItem.Name);
    grabbedItem.AxisLockAngularX = false;
    grabbedItem.AxisLockAngularY = false;
    grabbedItem.AxisLockAngularZ = false;
    grabbedItem = null;
  }
  
  // Simple raycast check as all the pickable objects are rigidbodies
  private RigidBody3D ItemInSight() {
    if (lookatRaycast.GetCollider() is RigidBody3D item) return item;
    return null;
  }
}
