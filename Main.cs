using Godot;
using System;

public partial class Main : Node
{

	[Export]
	public PackedScene MobScene {get;set;}
	private int _score;

	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();
		var hud = GetNode<CanvasLayer>("HUD");
		hud.Call("show_game_over"); 

		GetNode<AudioStreamPlayer>("Music").Stop();
		GetNode<AudioStreamPlayer>("DeathSound").Play();
	}

	public void NewGame()
	{
		_score = 0;
		var hud = GetNode<CanvasLayer>("HUD");
		hud.Call("update_score", _score);
		hud.Call("show_message", "Get Ready");

		var player = GetNode<Player>("Player");
		var startPosition = GetNode<Marker2D>("StartPosition");
		player.Start(startPosition.Position);

		GetNode<Timer>("StartTimer").Start();

		GetTree().CallGroup("mobs", Node.MethodName.QueueFree);

		GetNode<AudioStreamPlayer>("Music").Play();
	}

	private void OnScoreTimerTimeout()
	{
		_score++;
		GetNode<CanvasLayer>("HUD").Call("update_score", _score); 
	}

	private void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();
	}

	private void OnMobTimerTimeout()
	{
		// Create a new instance of the Mob scene.
		Mob mob = MobScene.Instantiate<Mob>();

		// Choose a random location on Path2D.
		var mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
		mobSpawnLocation.ProgressRatio = GD.Randf();

		// Set the mob's direction perpendicular to the path direction.
		float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

		// Set the mob's position to a random location.
		mob.Position = mobSpawnLocation.Position;

		// Add some randomness to the direction.
		direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
		mob.Rotation = direction;

		// Choose the velocity.
		var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
		mob.LinearVelocity = velocity.Rotated(direction);

		// Spawn the mob by adding it to the Main scene.
		AddChild(mob);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var player = GetNode<Player>("Player");
		player.Hit += GameOver; 

		//connect GDScript HUD isgnal to this C# NewGame 
		var hud = GetNode<CanvasLayer>("HUD");
		hud.Connect("start_game", Callable.From(NewGame));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
