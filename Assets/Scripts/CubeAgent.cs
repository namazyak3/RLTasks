using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class CubeAgent : Agent
{
    public float maxSpeed = 10f;
    public float rotationSpeed = 180f;
    public float jumpForce = 7f;

    public Transform sw;
    public Transform wall;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // スイッチに触れたらエピソード終了
        if (other.gameObject.name == "Switch")
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        // エピソードのセットアップ
        this.transform.localPosition = new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(2f, 6f));
        this.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        this.rb.velocity = Vector3.zero;
        this.rb.angularVelocity = Vector3.zero;
        sw.localPosition = new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-2f, -6f));

        // 現在の壁のスケール・位置を設定
        float wallHeight = Academy.Instance.EnvironmentParameters.GetWithDefault("wall_height", 0.0f);
        wall.localScale = new Vector3(wall.localScale.x, wallHeight, wall.localScale.z);

        float wallPositionY = wallHeight == 0 ? -1f : wallHeight/2;
        wall.localPosition = new Vector3(wall.localPosition.x, wallPositionY, wall.localPosition.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = maxSpeed * (-1 * actions.ContinuousActions[0]);
        Vector3 forwardDirection = this.transform.forward * moveForward;
        rb.AddForce(forwardDirection);

        float rotation = rotationSpeed * actions.ContinuousActions[1];
        this.transform.Rotate(0f, rotation * Time.fixedDeltaTime, 0f);

        if (actions.DiscreteActions[0] == 1 && IsGrounded())
        {
            rb.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
        }

        // 落下したらエピソード終了
        if (this.transform.localPosition.y < -1f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(this.transform.position + Vector3.up * 0.1f, Vector3.down, 0.11f);
    }
}
