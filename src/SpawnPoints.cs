using Godot;

public partial class SpawnPoints : Node3D {
  [Export] public Godot.Collections.Array<Node3D> SpawningPoints { get; private set; }
}
