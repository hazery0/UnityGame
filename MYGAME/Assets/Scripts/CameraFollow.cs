using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target; // 需要拖拽玩家对象到这个字段

    [Header("相机参数")]
    public Vector3 offset = new Vector3(0f, 10f, 5f); // 固定在玩家斜后方上方

    [Header("滚轮缩放")]
    public float zoomSpeed = 10f;
    public float minDistance = 5f;
    public float maxDistance = 25f;

    // 固定视角角度
    private Vector3 fixedRotation = new Vector3(45f, 0f, 0f);
    private Vector3 currentOffset;

    void Start()
    {
        // 初始化时设置固定视角和当前offset
        transform.rotation = Quaternion.Euler(fixedRotation);
        currentOffset = offset;

        if (target == null)
        {
            Debug.LogError("请设置相机跟随的目标对象！");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Zoom();
        FollowTarget();
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 以 offset 方向为缩放方向
            Vector3 direction = currentOffset.normalized;
            float distance = currentOffset.magnitude;

            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            // 更新currentOffset
            currentOffset = direction * distance;
        }
    }

    void FollowTarget()
    {
        // 直接设置相机位置，保持与target的相对offset
        transform.position = target.position + currentOffset;

        // 始终保持固定旋转
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}