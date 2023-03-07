using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[Serializable]
public struct FootCastPair
{
    [SerializeField]
    private FootMovementSphere foot;

    [SerializeField]
    private Transform raycastPoint;

    [SerializeField]
    private Transform legRoot;

    // [SerializeField]
    // private Transform footMesh;

    [SerializeField]
    public FootMovementSphere Foot => foot;

    public Transform RaycastPoint => raycastPoint;

    public Vector3 Position => foot.position;

    public Transform LegRoot => legRoot;


    // public Transform FootMesh => footMesh;


    public FootCastPair(FootMovementSphere foot, Transform raycastPoint, Transform legRoot)
    {
        this.foot = foot;
        this.raycastPoint = raycastPoint;
        this.legRoot = legRoot;
    }
}


public class QuadrupedMovementController : MonoBehaviour
{
    enum MovementPattern
    {
        OneAtATime,
        EveryOtherOne,
        TwoInARow,
        OppositeCorners,
        FrontThenBack,
    }

    enum QuadState
    {
        Upright,
        Ragdoll
    }

    [SerializeField]
    private QuadState quadState = QuadState.Upright;

    [Header("Feet")]
    [SerializeField]
    private FootCastPair[] feet;

    [SerializeField]
    private FootCastPair[] frontFeet;

    [SerializeField]
    private FootCastPair[] backFeet;

    [SerializeField]
    private int footIndex = 0;

    [SerializeField]
    private float anchorRange = 3f;


    [Header("Cast Points")]
    [SerializeField]
    private float castDistance = 7f;

    [SerializeField]
    private LayerMask castLayer;

    [Header("Body")]
    [SerializeField]
    private float bodyHeight = 2f;

    [SerializeField]
    [Range(0f, 1f)]
    private float bodyToFeetLerp = .2f;

    [Space(10)]
    [Header("Movement Control")]
    [SerializeField]
    private MovementPattern movementPattern;


    [SerializeField]
    private Transform castParent;

    [SerializeField]
    private Vector2 feetMoveAngle = new Vector2(30, 15);

    [SerializeField]
    private float footMoveTime = .1f;

    [SerializeField]
    private float gravityMultiplier = 1f;

    [SerializeField]
    private float timeBetweenFoot = .5f;

    [SerializeField]
    private float footUpdateDistance = 10f;


    private Vector2 inputDir_Local;
    private Vector3 inputDir_World => InputDir_World();
    private Vector3 inputDir_LastForward = Vector3.forward;
    private Vector3 lastInputDir = new Vector3();


    [Header("Ground Check")]
    [SerializeField]
    private float groundCheckTime = 1f;

    private float groundCheckTime_Now = 1f;
    private bool isGrounded = false;

    [Space(10)]
    [Header("Force")]
    [SerializeField]
    private Rigidbody catRigidbody;

    private Transform catTransform => catRigidbody.transform;

    [SerializeField]
    private float move_Y = 10f;

    [SerializeField]
    private float move_XZ = 20f;

    [SerializeField]
    private float maxVelocity = 5f;


    [SerializeField]
    private Vector3 moveTorque = new Vector3(.4f, 5f, .2f);

    [SerializeField]
    private Vector2 maxRotation = new Vector2(30f, 30f);

    [SerializeField]
    private Vector2 maxAngular = new Vector2(3f, 3f);

    private float lastMoveTime = 0f;


    [Header("Jump")]
    [SerializeField]
    private float jumpForce_Y = 100f;

    [SerializeField]
    private Vector2 jumpForce_XZ = new Vector2(100f, 100f);

    [SerializeField]
    private Vector2 jumpForceRandom_XZ = new Vector2(100f, 100f);

    [SerializeField]
    private Vector2 jumpBodyMultiplier = new Vector2(.85f, 1.15f);

    [Space(10)]
    [Header("Ragdoll")]
    [SerializeField]
    private Rigidbody[] rigidbodies;

    [SerializeField]
    private float ragdollGravityMultiplier = 4f;

    private Vector3 floorPoint;
    private float gravity;
    private Vector3 catAngles;
    private Vector3 bodyTarget;

    [Space(5)]
    [Header("Swipe")]
    [SerializeField]
    [Tooltip("X: initial, Y: Side")]
    private Vector2 swipe_Force = new Vector2(200f, 100f);

    [SerializeField]
    float swipe_LaunchAngle = 30;

    [SerializeField]
    private float swipe_LaunchOffset = 10f;

    [SerializeField]
    private float swipe_Cooldown = 1f;

    private float swipe_Cooldown_Now = 0f;
    private float explosiveMultiplier = 5f;


    private void Awake()
    {
    }

    [ContextMenu("Initialise Feet")]
    public void InitialiseAllFeet()
    {
        foreach (FootCastPair footCastPair in feet)
        {
            FootMovementSphere footMovementSphere = footCastPair.Foot;
            footMovementSphere.Initialize(this, gravityMultiplier);
        }
    }

    [ContextMenu("Get RB and Joins from Model")]
    public void GetRBFromModel()
    {
        if (catTransform)
        {
            rigidbodies = catTransform.GetComponentsInChildren<Rigidbody>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (inputDir_Local.magnitude > 0.1f)
        {
            Vector2 dir = inputDir_Local * 10f;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir.x, 0, dir.y));
        }

        if (floorPoint.magnitude > 0.01f)
        {
            Gizmos.DrawSphere(floorPoint, 2f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeQuadState(quadState);
        InitialiseAllFeet();

        UpdateFeetToCastPosition();

        lastInputDir = catTransform.forward;
        inputDir_LastForward = catTransform.forward;
        gravity = -Physics.gravity.magnitude * catRigidbody.mass * ragdollGravityMultiplier;
    }

    void Update()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                // MoveModel_ClampRotation();
                break;
            case QuadState.Ragdoll:

                break;
        }

        if (swipe_Cooldown_Now > 0)
        {
            swipe_Cooldown_Now -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                FixedUpdate_Upright();
                break;
            case QuadState.Ragdoll:
                FixedUpdate_Ragdoll();

                break;
        }

        lastInputDir = inputDir_Local;
    }

    private void FixedUpdate_Upright()
    {
        isGrounded = GroundCheck();
        MoveBody();

        MoveCatFeet();
        // if (isGrounded && inputDir_Local.magnitude < 0.1f)
        // {
        //     UpdateCatFeet();
        // }

        UpdateCastParent();
    }


    private void FixedUpdate_Ragdoll()
    {
        // UpdateBodyToFeet();

        MoveCatFeet();


        MoveBody();
    }


    public void OnMove(Vector2 moveDir)
    {
        if (!moveDir.Equals(inputDir_Local))
        {
            inputDir_Local = moveDir;
        }

        if (moveDir.magnitude > 0.1f)
        {
            inputDir_LastForward = inputDir_World;
        }

        bool startToMove = inputDir_Local.magnitude > 0.1f;
        if (startToMove && isGrounded && quadState == QuadState.Ragdoll)
        {
            ChangeQuadState(QuadState.Upright);
        }
    }

    public void OnMove_World(Vector3 world)
    {
        world = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * world;
        OnMove(new Vector2(world.x, world.z));
    }

    [ContextMenu("Upright")]
    public void SwitchQuad_Upright()
    {
        ChangeQuadState(QuadState.Upright);
    }

    [ContextMenu("Ragdoll")]
    public void SwitchQuad_Ragdoll()
    {
        ChangeQuadState(QuadState.Ragdoll);
    }

    void ChangeQuadState(QuadState qs)
    {
        switch (qs)
        {
            case QuadState.Upright:

                // catTransform.transform.localPosition = new Vector3();
                // catTransform.transform.rotation = mainTransform.rotation;

                foreach (FootCastPair footCastPair in feet)
                {
                    footCastPair.Foot.SetFootIdle();
                }

                GetTargetBodyPosition();
                UpdateCastParent();
                UpdateFeetToCastPosition();

                break;
            case QuadState.Ragdoll:
                // foreach (Rigidbody rb in rigidbodies)
                // {
                //     rb.isKinematic = false;
                //     rb.useGravity = true;
                // }
                foreach (FootCastPair footCastPair in feet)
                {
                    footCastPair.Foot.SetFootFree();
                }

                break;
        }

        quadState = qs;
    }


    //****************CAT FEET MOVEMENT

    void UpdateCatFeet()
    {
        Vector3 targetPos = FindLegTarget(feet[footIndex].RaycastPoint);

        if (Vector3.Distance(feet[footIndex].Foot.position, targetPos) > footUpdateDistance)
        {
            MoveCatFeet(true);
        }
    }

    private void MoveCatFeet(bool force = false)
    {
        if (force || inputDir_Local.magnitude > 0f ||
            Mathf.Abs(inputDir_Local.magnitude - lastInputDir.magnitude) > 0.1f)
        {
            if (force || Time.time - lastMoveTime > timeBetweenFoot)
            {
                if (!force)
                {
                    lastMoveTime = Time.time;
                }

                int oddIndex = footIndex % 2;

                switch (movementPattern)
                {
                    case MovementPattern.OneAtATime:
                        LaunchFoot();
                        footIndex = (footIndex + 1) % feet.Length;
                        break;
                    case MovementPattern.EveryOtherOne:
                        for (int i = oddIndex; i < feet.Length; i += 2)
                        {
                            footIndex = i;
                            LaunchFoot();
                        }

                        footIndex = (footIndex + 1) % feet.Length;

                        break;
                    case MovementPattern.TwoInARow:
                        for (int i = 0; i < feet.Length; i += 4)
                        {
                            LaunchFoot((footIndex + i) % feet.Length);
                            LaunchFoot((footIndex + i + 1) % feet.Length);
                        }

                        footIndex = (footIndex + 2) % feet.Length;

                        break;


                    case MovementPattern.OppositeCorners:
                        MoveCatFeet_OppositeCorners(oddIndex);
                        footIndex = (footIndex + 1) % feet.Length;

                        break;
                    case MovementPattern.FrontThenBack:
                        if (oddIndex == 0)
                        {
                            foreach (FootCastPair footCastPair in frontFeet)
                            {
                                LaunchFoot(footCastPair);
                            }
                        }
                        else
                        {
                            foreach (FootCastPair footCastPair in backFeet)
                            {
                                LaunchFoot(footCastPair);
                            }
                        }

                        footIndex = (footIndex + 1) % feet.Length;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void MoveCatFeet_OppositeCorners(int oddIndex)
    {
        LaunchFoot(frontFeet[oddIndex]);
        LaunchFoot(backFeet[1 - oddIndex]);
    }

    void LaunchFoot(int index = -1, bool flipYAngles = false)
    {
        if (index < 0)
        {
            index = footIndex;
        }

        LaunchFoot(feet[index], flipYAngles);
    }

    void LaunchFoot(FootCastPair footCastPair, bool flipYAngles = false)
    {
        Vector3 trajectory = FindTrajectoryToTarget(footCastPair.Foot, footCastPair.RaycastPoint, flipYAngles);
        Vector3 velocity = inputDir_World * move_XZ;


        trajectory += velocity;

        trajectory += catRigidbody.velocity;

        footCastPair.Foot.Move_SetVelocity(trajectory);
    }

    [ContextMenu("Force Update Feet")]
    private void UpdateFeetToCastPosition()
    {
        Transform currentCast;
        FootMovementSphere currentFoot;
        RaycastHit hit;
        for (int i = 0; i < feet.Length; i++)
        {
            currentCast = feet[i].RaycastPoint;
            currentFoot = feet[i].Foot;
            if (Physics.Raycast(currentCast.position, currentCast.forward, out hit, castDistance, castLayer))
            {
                currentFoot.position = hit.point + new Vector3(0, 1f, 0);
            }
        }
    }


    Vector3 FindTrajectoryToTarget(FootMovementSphere currentFoot, Transform legCastPoint, bool flipYAngle = false)
    {
        Vector3 targetPos = FindLegTarget(legCastPoint, flipYAngle);
        if (targetPos.Equals(new Vector3()))
        {
            return targetPos;
        }

        Vector3 moveAmount = targetPos - currentFoot.position;
        float t = footMoveTime;
        float horizontal = (moveAmount.magnitude / t);
        float gravityMagnitude = currentFoot.GravityExtra;
        float y = gravityMagnitude * t / 2f + moveAmount.y;
        Vector3 velocity = new Vector3(horizontal * moveAmount.normalized.x, y, horizontal * moveAmount.normalized.z);

        // Debug.Log($"Calculated Velocity: {velocity}, {velocity.magnitude}");


        return velocity;
    }


    Vector3 FindLegTarget(Transform legCastPoint, bool flipYAngle = false)
    {
        RaycastHit hit;
        Vector3 dir = legCastPoint.forward;
        float yAngle = feetMoveAngle.y;
        if (flipYAngle)
        {
            yAngle *= -1f;
        }

        Quaternion angleAxis = Quaternion.AngleAxis(yAngle * inputDir_Local.x, transform.forward) *
                               Quaternion.AngleAxis(-feetMoveAngle.x * inputDir_Local.y, transform.right);
        dir = angleAxis * dir;
        // Debug.Log($"{legCastPoint} angle: {angleAxis.eulerAngles}");
        Vector3 offset = (move_XZ * footMoveTime) * inputDir_World;

        if (Physics.Raycast(legCastPoint.position + offset, dir, out hit, castDistance, castLayer))
        {
            Debug.DrawLine(legCastPoint.position + offset, hit.point, Color.blue, .5f);
            return hit.point;
        }
        else
        {
            return new Vector3();
        }
    }

    void UpdateCastParent()
    {
        if (!castParent)
        {
            return;
        }

        // castParent.up = Vector3.up;
        // castParent.position = floorPoint + new Vector3(0f, bodyHeight * 1.1f, 0);
        castParent.position = bodyTarget + new Vector3(0f, bodyHeight * .1f, 0);

        castParent.forward = inputDir_LastForward;
    }


    //****************CAT BODY MOVE

    Vector3 AverageFeetPosition()
    {
        Vector3 avg = new Vector3();
        int i = 0;
        foreach (FootCastPair footCastPair in feet)
        {
            avg += footCastPair.Position;
        }

        return avg / feet.Length;
    }

    Vector3 TargetLocalRotation()
    {
        Vector3 rotation = new Vector3();
        Vector3 up = Vector3.up;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, castDistance, castLayer))
        {
            up = hit.normal;
        }
        else
        {
            // Vector3 forward = ((frontFeet[0].Position - backFeet[0].Position) +
            //                    (frontFeet[1].Position - backFeet[1].Position)) / 2f;
            // Vector3 right = ((frontFeet[1].Position - frontFeet[0].Position) +
            //                  (backFeet[1].Position - backFeet[0].Position)) / 2f;
            up = Vector3.Cross(frontFeet[0].Position - backFeet[1].Position,
                frontFeet[1].Position - backFeet[0].Position);
            //DEBUG
            Vector3 pos = AverageFeetPosition();
            // Debug.DrawRay(pos, forward.normalized * 10f, Color.blue);
            // Debug.DrawRay(pos, right.normalized * 10f, Color.red);


            // print($"rotation: {rotation}");
        }

        rotation.x = Vector3.SignedAngle(catTransform.up, up, catTransform.right);
        rotation.z = Vector3.SignedAngle(catTransform.up, up, catTransform.forward);

        rotation.y = Vector3.SignedAngle(catTransform.forward, inputDir_LastForward,
            catTransform.up);
        return rotation;
    }

    Vector3 GetTargetBodyPosition()
    {
        Vector3 heightOffset = new Vector3(0, bodyHeight, 0);

        Vector3 avg = AverageFeetPosition();
        Vector3 position = catRigidbody.position;
        if (Physics.Raycast(avg, -Vector3.up, out RaycastHit hit,
                bodyHeight * 2f, castLayer))
        {
            position.y = hit.point.y;
        }
        else
        {
            position.y = avg.y;
        }

        floorPoint = position;
        position.y += bodyHeight;
        bodyTarget = position;

        return position;
    }

    Vector3 UpdateBodyToFeet()
    {
        Vector3 position = catRigidbody.position;
        position.y = AverageFeetPosition().y;
        return position;
    }

    void MoveBody()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                // MoveModel_Velocity();
                // MoveModel_Velocity_Y();

                if (isGrounded)
                {
                    MoveModel_Accel_Y();
                    // MoveModel_ClampRotation();
                    MoveModel_Torque();
                }
                else
                {
                    MoveModel_Gravity();
                }


                MoveModel_Accel_XY();
                // MoveModel_ClampAngular();
                break;
            case QuadState.Ragdoll:
                MoveModel_Gravity();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MoveModel_Gravity()
    {
        catRigidbody.AddForce(new Vector3(0, gravity,
            0));
    }

    void MoveModel_Velocity()
    {
        float yDif = GetTargetBodyPosition().y - catRigidbody.position.y;

        Vector3 dir = catRigidbody.rotation * new Vector3(inputDir_Local.x, 0, inputDir_Local.y);
        Vector3 velocity = dir.normalized * move_Y;

        velocity += new Vector3(0, yDif * 2f, 0);

        Debug.DrawRay(catRigidbody.position, velocity, Color.blue);
        catRigidbody.velocity = velocity;
        // print($"Move velocity: {velocity}, {velocity.magnitude}  Compared to {catRigidbody.velocity.magnitude}");
    }

    void MoveModel_Velocity_Y()
    {
        float yDif = GetTargetBodyPosition().y - catRigidbody.position.y;


        Vector3 velocity = new Vector3();
        // Vector3 dir = catRigidbody.rotation * new Vector3(inputDir.x, 0, inputDir.y);
        // velocity = dir.normalized * moveVelocity;

        velocity += new Vector3(0, yDif * 2f, 0);

        Debug.DrawRay(catRigidbody.position, velocity, Color.cyan);

        Vector3 temp = catRigidbody.velocity;
        temp.y = velocity.y;
        catRigidbody.velocity = velocity;
        // print($"Move velocity: {velocity}, {velocity.magnitude}  Compared to {catRigidbody.velocity.magnitude}");
    }

    private void MoveModel_Accel_Y()
    {
        Vector3 targetBodyPosition = GetTargetBodyPosition();
        Vector3 dif = targetBodyPosition - catRigidbody.position;
        float yDif = dif.y;

        float dot = 1;
        float yCat = catRigidbody.velocity.y;
        if (Mathf.Abs(yCat) > Mathf.Abs(yDif))
        {
            // dot =Vector3.Dot(dif.normalized, catRigidbody.velocity.normalized);
            if (yCat * yDif > 0)
            {
                dot = -1;
            }
        }

        Vector3 velocity = new Vector3(0, (yDif * move_Y * dot), 0f);
        // Vector3 dir = catRigidbody.rotation * new Vector3(inputDir.x, 0, inputDir.y);
        // velocity = dir.normalized * moveVelocity;


        Debug.DrawRay(catRigidbody.position, velocity, Color.yellow);
        catRigidbody.AddForce(velocity, ForceMode.Acceleration);

        // print($"Move velocity: {velocity}, {velocity.magnitude}  Compared to {catRigidbody.velocity.magnitude}");
    }

    void MoveModel_Accel_XY()
    {
        // float yDif = GetTargetBodyPosition().y - catRigidbody.position.y;

        Vector3 velocity = inputDir_World.normalized * move_XZ;

        // velocity += new Vector3(0, yDif*2f, 0);

        Debug.DrawRay(catRigidbody.position, velocity, Color.green);
        catRigidbody.AddForce(velocity, ForceMode.VelocityChange);
        catRigidbody.velocity = Vector3.ClampMagnitude(catRigidbody.velocity, maxVelocity);
        // print($"Move velocity: {velocity}, {velocity.magnitude}  Compared to {catRigidbody.velocity.magnitude}");
    }

    void MoveModel_Torque()
    {
        Vector3 targetRotation = TargetLocalRotation();

        Vector3 localTorque = Quaternion.Inverse(catRigidbody.rotation) * catRigidbody.angularVelocity;
        Vector3 localVelocityDif = targetRotation - localTorque;

        targetRotation = new Vector3(localVelocityDif.x * moveTorque.x, localVelocityDif.y * moveTorque.y,
            localVelocityDif.z * moveTorque.z);
        targetRotation = catRigidbody.rotation * targetRotation;

        catRigidbody.AddTorque(targetRotation, ForceMode.VelocityChange);
    }

    void MoveModel_ClampRotation()
    {
        Vector3 catRigidbodyAngularVelocity = catRigidbody.angularVelocity;

        // catAngles = catRigidbody.rotation.eulerAngles;
        float signX = Vector3.SignedAngle(Vector3.up, catTransform.up, Vector3.right);
        if (Mathf.Abs(signX) > maxRotation.x)
        {
            catRigidbodyAngularVelocity.x = 0f;
            print($"x reached max {signX}");
            // catAngles.x = Mathf.Clamp(signX, -maxRotation.x, maxRotation.x);
        }

        float signZ = Vector3.SignedAngle(Vector3.up, catTransform.up, Vector3.forward);

        if (Mathf.Abs(signZ) > maxRotation.y)
        {
            catRigidbodyAngularVelocity.z = 0f;
            print($"z reached max {signZ}");

            // catAngles.z = Mathf.Clamp(catAngles.z, -maxRotation.y, maxRotation.y);
        }

        catRigidbody.rotation = Quaternion.Euler(catAngles);
    }

    void MoveModel_ClampAngular()
    {
        Vector3 v = catRigidbody.angularVelocity;
        v.x = Mathf.Clamp(v.x, maxAngular.x, maxAngular.y);
        v.z = Mathf.Clamp(v.z, maxAngular.x, maxAngular.y);
    }

    void ApplyTorque(float cat, float move, Vector3 torque)
    {
        if (Mathf.Abs(cat) > Mathf.Abs(move))
        {
            if (cat * move > 0)
            {
                catRigidbody.AddTorque(-torque, ForceMode.Acceleration);
                return;
            }
        }

        catRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    float ClampRotation(float r)
    {
        float temp = r;
        r = r % 360;
        if (r > 180f)
        {
            r = r - 360;
        }

        return r;
    }

    //********************JUMP
    public void Jump()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                if (!GroundCheck())
                {
                    return;
                }

                ChangeQuadState(QuadState.Ragdoll);
                Vector3 baseVelocity =
                    (new Vector3(inputDir_World.x * jumpForce_XZ.x, jumpForce_Y, inputDir_World.z * jumpForce_XZ.y));
                Vector3 velocity;
                foreach (FootCastPair footCastPair in feet)
                {
                    velocity = baseVelocity + RandomJumpVelocity();
                    // velocity = transform.rotation * velocity;
                    footCastPair.Foot.SetJump(velocity);
                }

                catRigidbody.AddForce(
                    (baseVelocity + RandomJumpVelocity()) * Random.Range(jumpBodyMultiplier.x, jumpBodyMultiplier.y),
                    ForceMode.VelocityChange);

                break;
            case QuadState.Ragdoll:
                ChangeQuadState(QuadState.Upright);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Vector3 RandomJumpVelocity()
    {
        return new Vector3(Random.Range(-jumpForceRandom_XZ.x, jumpForceRandom_XZ.x), 0,
            Random.Range(-jumpForceRandom_XZ.y, jumpForceRandom_XZ.y));
    }


    Vector3 InputDir_World()
    {
        Vector3 world = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up), 0) *
                        new Vector3(inputDir_Local.x, 0, inputDir_Local.y);
        return world.normalized;
    }

    bool GroundCheck()
    {
        foreach (FootCastPair footCastPair in feet)
        {
            if (footCastPair.Foot.IsIdle)
            {
                return true;
            }
        }

        return false;
    }


    //******************SWIPE
    public void OnSwipe(Vector3 dir, Vector3 cam)
    {
        FootMovementSphere foot = GetClosestFoot(dir, cam);
        if (!foot)
        {
            return;
        }

        if (swipe_Cooldown_Now > 0)
        {
            return;
        }

        if (inputDir_World.magnitude > 0.1f)
        {
            dir = inputDir_World;
        }

        swipe_Cooldown_Now = swipe_Cooldown;
        Quaternion angle = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, dir, Vector3.up), Vector3.up);
        dir = angle * Quaternion.AngleAxis(swipe_LaunchOffset, Vector3.up) *
              Quaternion.AngleAxis(-swipe_LaunchAngle, Vector3.right) * Vector3.forward;
        Vector3 sideForce = Quaternion.AngleAxis(-105, Vector3.up) * dir;
        foot.Swipe(dir * swipe_Force.x, sideForce * swipe_Force.y);
    }

    public FootMovementSphere GetClosestFoot(Vector3 dir, Vector3 camPos)
    {
        float distance = 0f;
        FootMovementSphere current = null;
        if (inputDir_World.magnitude > 0.1f)
        {
            dir = inputDir_World;
        }

        foreach (FootCastPair footCastPair in feet)
        {
            footCastPair.Foot.SetHighlight(false);
            if (!footCastPair.Foot.IsSwiping)
            {
                float d = Vector3.Distance(footCastPair.Position, camPos);
                d *= Vector3.Dot(dir, (footCastPair.LegRoot.position - catTransform.position).normalized);
                if (d > distance)
                {
                    distance = d;
                    current = footCastPair.Foot;
                }
            }
        }

        current?.SetHighlight(true);

        return current;
    }


    //*****************RESET
    public void HardReset()
    {
        Vector3 position = GetTargetBodyPosition() + new Vector3(0, bodyHeight / 2f, 0);
        catTransform.position = position;
        catTransform.rotation = Quaternion.identity;
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.velocity = new Vector3();
            rb.angularVelocity = new Vector3();
        }
    }
    
    //***********EXPlODE
    public void Explode(Vector3 force,Vector3 velocity)
    {
        ChangeQuadState(QuadState.Ragdoll);
        catRigidbody.AddForce(force*explosiveMultiplier);
        catRigidbody.AddForce(velocity*explosiveMultiplier, ForceMode.VelocityChange);
    }
}