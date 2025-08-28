using Godot;

[GlobalClass]
public partial class AudioConditionPlayer : Resource
{
    [Export] public float value;
    [Export] public string[] groups;
    [Export] public AudioStream stream;
}
