using Godot;
using System;

public class Player : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private int _moveSpeed = 80;

    private AnimationTree _animTree;
    private AnimationNodeStateMachinePlayback _animStateMachine;
    private Vector2 _movement = new Vector2();
    private Vector2 _direction = new Vector2();

    private const string _BombResource = "res://Nodes/Bomb.tscn";
    private PackedScene _packedSceneBomb;

    private int _amountOfBombs = 5;
    public int amountOfBombs = 5;

    private bool _isInvincible = true;
    private AnimatedSprite _sprite;
    private bool _isFlickerOn = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animTree = (AnimationTree)GetNode("AnimationTree");
        _animStateMachine = (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/playback");

        _packedSceneBomb = ResourceLoader.Load<PackedScene>(_BombResource);
        GD.Print("Player ready!");
        Timer timer_reload = GetNode<Timer>("Reload");
        timer_reload.Connect("timeout", this, "Reload");
        Timer timer_invincibility = GetNode<Timer>("Invincibility");
        timer_invincibility.Connect("timeout", this, "RemoveInvincibility");
        Timer timer_invincibility_flicker = GetNode<Timer>("InvincibilityFlicker");
        timer_invincibility_flicker.Connect("timeout", this, "Flicker");
        _sprite = (AnimatedSprite)this.GetNode("./AnimatedSprite");
    }

    private void Reload(){
        if (_amountOfBombs < amountOfBombs){
            _amountOfBombs++;
        }
    }

    private void Flicker(){
        if (_isInvincible){
            if (_isFlickerOn){
                _sprite.Modulate = new Color(1,1,1,0.5f);
                _isFlickerOn = false;
            }
            else {
                _sprite.Modulate = new Color(1,1,1,1f);
                _isFlickerOn = true;
            }
        }
    else{
        if (!_isFlickerOn){
            _isFlickerOn = true;
            _sprite.Modulate = new Color(1,1,1,1f);
        }
    }
    }

    private void RemoveInvincibility(){
        _isInvincible = false;
    }

    private void Hit(){
    }

//    public override void _Input(InputEvent @event)
//    {
//        // Mouse in viewport coordinates.
//        if (@event is InputEventMouseButton eventMouseButton)
//        {
//            //            GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
//            //            GD.Print("Viewport Resolution is: ", GetViewportRect().Size);
//            //            GD.Print("MousePositon is: ", GetViewport().GetMousePosition());
//            GD.Print("Direction Mouse: ", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            _animTree.Set("parameters/Idle/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            _animTree.Set("parameters/Walk/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//        }
//        else if (@event is InputEventMouseMotion eventMouseMotion)
//        {
//           // _animTree.Set("parameters/Idle/blend_position", Position.DirectionTo(eventMouseMotion.Position).Normalized());
//            // _animTree.Set("parameters/Walk/blend_position", Position.DirectionTo(eventMouseButton.Position).Normalized());
//            //            GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
//            //           GD.Print("Mouse Motion at RELATIVE: ", eventMouseMotion.Relative);
//        }
//        // Print the size of the viewport.
//    }

    public override void _PhysicsProcess(float delta)
    {
        _movement.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        _movement.y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        _direction = _movement.Normalized();
        var possibleCollision = MoveAndCollide(_direction * _moveSpeed * delta);

        // Update animation tree based on direction
        string animStateAnimation = _direction == Vector2.Zero ? "Idle" : "Walk";
        _animStateMachine.Travel(animStateAnimation);
        if (animStateAnimation == "Walk"){
            _animTree.Set("parameters/Idle/blend_position", _direction);
            _animTree.Set("parameters/Walk/blend_position", _direction);
        }

        if (possibleCollision != null){
            KinematicCollision2D collision = (KinematicCollision2D)possibleCollision;
            Node collider = (Node)collision.Collider;
            GD.Print("Collides with: ", collider.Name);
            if (collider.Name.StartsWith("Flame")){
            }
            else if (collider.Name.StartsWith("Prize")){
            }
            else if (collider.Name.StartsWith("Powerup_speed")){
            }
            else if (collider.Name.StartsWith("Powerup_bomb")){
            }
            else if (collider.Name.StartsWith("Powerup_flame")){
                if (!(_isInvincible)){
                    Hit();
                }
            }
        }

        if (Input.IsActionPressed("place_bomb") && _amountOfBombs > 0)
        {
            _amountOfBombs--;
            Bomb newBomb = _packedSceneBomb.Instance() as Bomb;
            newBomb.Init(5);
            // Change position to center
            // cell size is 32, subtract module then add half cell size (16)
            Vector2 centeredPosition = new Vector2();
            centeredPosition.x = (float)(Math.Round(Position.x  / 32) * 32);
            centeredPosition.y = (float)(Math.Round(Position.y  / 32) * 32);
            newBomb.Position = centeredPosition;
            GD.Print("Bomb position at: ", Position.x, " ", Position.y);
            GD.Print("Bomb centered position at: ", centeredPosition.x, " ", centeredPosition.y);
            GetTree().Root.AddChild(newBomb);
        }

    }
}
