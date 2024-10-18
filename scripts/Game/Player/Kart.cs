using Godot;
using System;
using System.Drawing;
using System.Net.WebSockets;

public partial class Kart : RigidBody3D
{

	private Label3D _nametag;
	private Node3D _model;
	private Marker3D _cameraAnchor;
	private SphereShape3D _sphereShape;
	private RayCast3D _raycast;
	private SpotLight3D _headlights;

    private MeshInstance3D _model_body;
    private MeshInstance3D _model_wheel_front_left;
    private MeshInstance3D _model_wheel_front_right;
    private MeshInstance3D _model_wheel_back_left;
    private MeshInstance3D _model_wheel_back_right;


    [Export] public float KartGravity = 9.8f;
    [Export] public bool Local = false;

    [ExportCategory("Kart Settings")]
    [Export(hintString: "suffix:N")] public float EngineForce = 100;
    [Export(hintString: "suffix:ms^-1")] public float MaxSpeed = 60;
    [Export(hintString: "suffix:ms^-1")] public float OffroadMaxSpeed = 20;
    [Export(hintString: "suffix:deg")] public float SteeringAngle = 5;
    [Export(hintString: "suffix:deg/s")] public float TurnSpeed = 10;
    [Export(hintString: "suffix:deg/s")] public float DriftModifier = 1.5f;
    [Export(hintString: "suffix:m/s")] public float MinTurnCarSpeed = 5;



    public Vector3 KartGravityDirection = Vector3.Down;
    public NetworkManager.NetworkPlayerData NetworkData;

    public Vector3 ModelPos
    {
        get => _model.GlobalPosition;
        set => _model.GlobalPosition = value;
    }
    public Vector3 ModelRotation
    {
        get => _model.GlobalRotation;
        set => _model.GlobalRotation = value;
    }

    private int _driftDirection; // -1 ~ 0 ~ 1 : left ~ none ~ right
    private float _driftTime;
    private float _currentMaxSpeed;
	private float _frameSpeed;
	private float _frameTurn;
	private bool _frameStartDrift;
	private bool _frameDrift;
	private bool _offroad;
    private int _age;

	public override void _Ready()
	{
        _nametag = GetNode<Label3D>("Nametag");
        _model = GetNode<Node3D>("Model");
        _sphereShape = GetNode<CollisionShape3D>("CollisionShape3D").Shape as SphereShape3D;
        _headlights = GetNode<SpotLight3D>("Headlights");

        NetworkData = new NetworkManager.NetworkPlayerData() 
        {
            PeerUid = -1,
            Username = "",
            ModelIndex = 0
        };

        _age = 0;

        DebugOverlay.Instance.DebugLines["kart_secheader"] = "\n-- Kart --";
        DebugOverlay.Instance.DebugLines["username"] = "";
        DebugOverlay.Instance.DebugLines["current_model"] = "";
        DebugOverlay.Instance.DebugLines["speed"] = "";
        DebugOverlay.Instance.DebugLines["drift_time"] = "";


        SetupModel();
	}

    public override void _Process(double delta)
    {
        var auth = NetworkManager.Instance.HasControlAuthority(NetworkData);

        if (Local) NetworkData.PeerUid = NetworkManager.Instance.LocalUid;
        if (auth) HandleInput((float)delta);


        _currentMaxSpeed = (Math.Sign(_frameSpeed) > 0 ? MaxSpeed / 4 : MaxSpeed);

        if (auth)
        {
            AngleModel((float)delta);


            GlobalManager.Instance.LocalPlayer_Speed_MetersPerSecond = LinearVelocity.Length();

            DebugOverlay.Instance.DebugLines["username"] = "Username: " + NetworkData.Username;
            DebugOverlay.Instance.DebugLines["speed"] = "Speed: " + Math.Round(LinearVelocity.Length(), 1) + "m/s";
            DebugOverlay.Instance.DebugLines["drift_time"] = "Drift Time: " + Math.Round(_driftTime, 1) + "s";
        }

        _nametag.Text = NetworkData.Username;
        if (NetworkData.Username.Length > 0)
        {
            Name = NetworkData.Username;
        }

        _nametag.Visible = NetworkManager.Instance.ConnectedToServer && !NetworkManager.Instance.HasControlAuthority(NetworkData);


        // update server on position and stuff
        if (_age % 5 == 0) // 12 updates/s @ 60fps
        {
            if (NetworkManager.Instance.HasControlAuthority(NetworkData))
                NetworkManager.Instance.Send(Packets.Packet_UpdateServerOnPosition(this));
        }


        // Angle headlights
        _headlights.GlobalBasis = _model.GlobalBasis.Rotated(_model.GlobalBasis.Y, Mathf.Pi);


        // increase age
        _age += 1;
    }

    public override void _PhysicsProcess(double delta)
	{
        _model.GlobalPosition = GlobalPosition + new Vector3(0, -_sphereShape.Radius, 0);
        if (!NetworkManager.Instance.HasControlAuthority(NetworkData)) return;

        CalculateKartPhysics((float)delta);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (!NetworkManager.Instance.HasControlAuthority(NetworkData)) return;

        GravityScale = 0;

        if (state.LinearVelocity.Length() > _currentMaxSpeed && _raycast.IsColliding())
        {
            state.LinearVelocity = state.LinearVelocity.Normalized() * _currentMaxSpeed;
        }

        if (_offroad)
        {
            state.LinearVelocity = state.LinearVelocity.Normalized() * Mathf.Lerp(LinearVelocity.Length(), Math.Min(LinearVelocity.Length(), OffroadMaxSpeed), 0.3f);
        }

        state.ApplyCentralForce(KartGravity * KartGravityDirection);

        state.AngularVelocity = Vector3.Zero;
    }


    public Transform3D GetModelGlobalTransform() => _model.GlobalTransform;


    private void CalculateKartPhysics(float delta)
    {
        _offroad = false;
        if (_raycast.IsColliding())
        {
            ApplyCentralForce(-_model.GlobalBasis.Z * _frameSpeed);

            var collidedObj = _raycast.GetCollider();
            if (collidedObj is CollisionObject3D c)
            {

                if (Util.IsBitSet(c.CollisionLayer, 3))
                {
                    _offroad = true;
                }
            }
        }
    }

    public void SetupModel()
    {
        var models = GlobalManager.Instance.KartModelScenes;

        if (models != null)
        {
            _model.QueueFree(); // remove default model object

            var newModel = models[NetworkData.ModelIndex].Instantiate<Node3D>();
            DebugOverlay.Instance.DebugLines["current_model"] = "Current Vehicle Model: " + newModel.Name;
            AddChild(newModel);

            _model = newModel; // set new model
        }

        // create raycast
        _raycast = new RayCast3D();
        _raycast.TargetPosition = new Vector3(0, -1f, 0);
        _raycast.AddException(this);
        _raycast.CollisionMask = 2; // collide with track
        _model.AddChild(_raycast);
        _raycast.Position = new Vector3(0, 0.1f, 0);


        // get parts
        _model_body = _model.GetNode<MeshInstance3D>("body");
        _model_wheel_front_left = _model.GetNode<MeshInstance3D>("wheel-front-left");
        _model_wheel_front_right = _model.GetNode<MeshInstance3D>("wheel-front-right");
        _model_wheel_back_left = _model.GetNode<MeshInstance3D>("wheel-back-left");
        _model_wheel_back_right = _model.GetNode<MeshInstance3D>("wheel-back-right");

        // headlights
        _headlights.Visible = NetworkData.HeadlightsState;

        // send update packet
        SendNetVisualsUpdate();
    }

    private void HandleInput(float delta)
    {
        _frameStartDrift = Input.IsActionJustPressed("drift");
        _frameDrift = Input.IsActionPressed("drift");


        float frameTurnAngle = SteeringAngle;


        // increase force based on ground angle
        var sinAngle = _raycast.IsColliding() ? Mathf.Sin(_raycast.GetCollisionNormal().AngleTo(Vector3.Up)) : 1f;
        _frameSpeed = (Input.GetAxis("accelerate", "brake") * EngineForce * (sinAngle + 1));
        _frameTurn = (Input.GetAxis("right", "left") + _driftDirection * DriftModifier) * Mathf.DegToRad(frameTurnAngle);


        if (Input.IsActionJustPressed("lights"))
        {
            NetworkData.HeadlightsState = !NetworkData.HeadlightsState;
            _headlights.Visible = NetworkData.HeadlightsState;
            SendNetVisualsUpdate();
        }

        if (_frameStartDrift)
        {
            _driftDirection = Math.Sign(_frameTurn);
        }

        if (!_frameDrift || LinearVelocity.Length() < MinTurnCarSpeed || _frameSpeed > 0)
        {
            _driftDirection = 0;
        }

        if (_driftDirection != 0)
        {
            _driftTime += delta;
        } else
        {
            _driftTime = 0;
        }


        if (Input.IsActionJustPressed("scroll_model"))
        {
            NetworkData.ModelIndex = Mathf.Wrap(NetworkData.ModelIndex + 1, 0, GlobalManager.Instance.KartModelScenes.Length);
            SetupModel();
        }

        if (_frameSpeed > 0 )
        {
            _frameTurn = -_frameTurn;
        }

        var leftWheelRot = _model_wheel_front_left.Rotation;
        var rightWheelRot = _model_wheel_front_right.Rotation;
        leftWheelRot.Y = _frameTurn * 3 * -Math.Sign(_frameSpeed);
        rightWheelRot.Y = _frameTurn * 3 * -Math.Sign(_frameSpeed);
        _model_wheel_front_left.Rotation = leftWheelRot;
        _model_wheel_front_right.Rotation = rightWheelRot;
    }

    private void AngleModel(float delta)
    {
        if (_raycast.IsColliding() && (LinearVelocity.Length() > MinTurnCarSpeed))
        {
            var newBasis = _model.GlobalBasis.Rotated(_model.GlobalBasis.Y, _frameTurn);
            _model.GlobalBasis = _model.GlobalBasis.Slerp(newBasis.Orthonormalized(), TurnSpeed * delta);
        }

        if (_raycast.IsColliding()) 
        {
            var n = _raycast.GetCollisionNormal();
            var xform = _model.GlobalTransform.AlignedToNormal(n);
            _model.GlobalTransform = xform;

        }

        // orthonormalize; we only want rotation
        _model.GlobalTransform = _model.GlobalTransform.Orthonormalized();
    }


    private void SendNetVisualsUpdate()
    {

        if (NetworkManager.Instance.HasControlAuthority(NetworkData)) 
            NetworkManager.Instance.Send(Packets.Packet_UpdateServerOnPlayerVisuals(this));
    }

}
