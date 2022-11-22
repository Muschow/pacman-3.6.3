using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PacmanScript : CharacterScript
{
    private Node2D raycasts;
    private Vector2 nextDir = Vector2.Zero;
    private Vector2 moveDir = Vector2.Zero;
    private HBoxContainer userInterface;
    [Export] private int lives = 3;

    [Signal] public delegate void UpdateTravelDist(float newDist);

    // Called when the node enters the scene tree for the first time.
    public PacmanScript()
    {
        speed = 400; //originally 100 speed
        speed = speed * Globals.gameSpeed; //need to figure out a way to have this inside of the character class/globals or something
    }

    public void GetInput()
    {
        if (Input.IsActionJustPressed("move_up"))
        {
            nextDir = Vector2.Up;
        }
        else if (Input.IsActionJustPressed("move_down"))
        {
            nextDir = Vector2.Down;
        }
        else if (Input.IsActionJustPressed("move_right"))
        {
            nextDir = Vector2.Right;
        }
        else if (Input.IsActionJustPressed("move_left"))
        {
            nextDir = Vector2.Left;
        }

        if ((bool)raycasts.Call("RaysAreColliding", nextDir) == false)
        {
            moveDir = nextDir;
        }
        //CheckCollision(); //merge checkCollision code with GetInput
        //moveVelocity = moveDir * speed;



    }


    private Vector2 Move(Vector2 moveDir, float speed) //change moveDir and speed
    {
        Vector2 moveVelocity = moveDir * speed;

        Vector2 masVector = MoveAndSlide(moveVelocity, Vector2.Up);

        PlayAndPauseAnim(masVector);

        return masVector;
    }

    public void OnHurtBoxAreaEntered(Area area) //do more stuff with this
    {
        lives--;
        userInterface.GetNode<Label>("LifeCounter").Text = "lives:" + lives + " ";
        CallDeferred("EnableInvincibility", 3);
    }

    private void EnableInvincibility(float time)
    {
        GetNode<CollisionShape2D>("HurtBox/CollisionShape2D").Disabled = true;
        GetNode<Timer>("HurtBox/InvincibleTimer").Start(time);
    }

    public void OnInvincibleTimerTimeout()
    {
        GetNode<CollisionShape2D>("HurtBox/CollisionShape2D").Disabled = false;
    }

    Vector2 oldPos;
    public void UpdateTravelDistance()
    {
        GD.Print("position" + Position.y);
        GD.Print("oldpos" + oldPos.y);
        if (Position.y < oldPos.y)
        {
            oldPos = Position;
            //GD.Print(oldPos.y);
            GD.Print(Mathf.Round(((oldPos) / 32).y)); //oldPos/32.y - (mazeStartLoc+mazeHeight)

            //EmitSignal("UpdateTravelDist", mazeTm.WorldToMap(oldPos - Position).y);
            //GetNode<Node2D>("/root/Game").Set("travelDist", mazeTm.WorldToMap(oldPos - Position).y);
        }
    }



    public override void _Ready()
    {
        GD.Print("pacman ready");
        mazeTm = GetNode<TileMap>("/root/Game/MazeContainer/Maze/MazeTilemap");
        raycasts = GetNode<Node2D>("RayCasts"); //maybe have a pacmanInit method with all this crap in
        userInterface = GetNode<HBoxContainer>("/root/Game/CanvasLayer/HBoxContainer");


        userInterface.GetNode<Label>("LifeCounter").Text = "lives:" + lives + " "; //put all the labels with initial values in a function like this and call the function in ready

        Position = new Vector2(1, (int)mazeTm.Get("mazeOriginY") + (int)mazeTm.Get("height") - 3) * 32 + new Vector2(16, 16);

        oldPos = Position;

        GD.Print("pman ps", Position);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        Vector2 masVector = Move(moveDir, speed);
        MoveAnimManager(masVector);
        UpdateTravelDistance();
    }
}
