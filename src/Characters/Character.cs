using Godot;

public abstract partial class Character : RigidBody3D {
  [Export] protected float Speed = 5;
  [Export] protected float Acceleration = 50;
  [Export] protected float Deceleration = 5;
  [Export] protected float SlipAngle = 10;
  [Export] protected float JumpForce = 20;
  [Export] protected float CounterForce = 45;

  [Export] protected Node3D Model;
  [Export] protected ShapeCast3D GroundCheck;

  [Export] public float Health = 100;
  public float CurrentHealth { get; private set; }
  protected Vector3 moveDirection;

  protected Vector3 SlideDirection() {
    return moveDirection.Slide(GroundNormal()).Normalized();
  }

  protected Vector3 GroundNormal() {
    var groundNormal = Vector3.Zero;
    if (GroundCheck.CollisionResult.Count > 0)
      groundNormal = GroundCheck.GetCollisionNormal(0);
    return groundNormal;
  }

  protected bool Moving() {
    return moveDirection.Length() != 0;
  }

  protected bool OnGround() {
    return (GroundCheck.CollisionResult.Count > 0 && Mathf.Abs(LinearVelocity.Y) <= 0.1f) || OnSlope();
  }

  protected bool OnSlope() {
    var groundAngle = Mathf.RadToDeg(Vector3.Up.AngleTo(GroundNormal()));
    return groundAngle < SlipAngle && groundAngle > 0;
  }

  public void TakeDamage(float amount) {
    CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
  }

  public void Heal(float amount) {
    CurrentHealth = Mathf.Max(Health, CurrentHealth + amount);
  }
}