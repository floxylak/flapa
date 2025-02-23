using Player.Interact;
using UnityEngine;

public class PipeLeak : MonoBehaviour
{
    [Header("Leak Settings")]
    [SerializeField] private Transform leakPos;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private LayerMask bucketLayer;

    [Header("Debug")]
    [SerializeField] private bool showRaycast = true;

    private BucketFill _currentBucketFill;
    private float detectionGracePeriod = 0.2f;
    private float timeSinceLastDetection = 0f;

    private void Update()
    {
        CheckForBucket();
    }

    private void CheckForBucket()
    {
        if (leakPos == null)
        {
            Debug.LogWarning("PipeLeak: leakPos is null, cannot raycast!", this);
            return;
        }

        Ray ray = new Ray(leakPos.position, Vector3.down);
        bool hitBucket = Physics.Raycast(ray, out RaycastHit hit, raycastDistance, bucketLayer);

        if (hitBucket)
        {
            BucketFill bucketFill = hit.collider.GetComponent<BucketFill>();
            if (bucketFill != null)
            {
                bucketFill.SetUnderPipe(true); // NEW: Mark bucket as under the pipe

                if (_currentBucketFill != bucketFill)
                {
                    if (_currentBucketFill != null)
                    {
                        _currentBucketFill.StopFilling();
                        _currentBucketFill.SetUnderPipe(false); // Mark old bucket as NOT under pipe
                    }

                    _currentBucketFill = bucketFill;
                    _currentBucketFill.StartFilling();
                }

                timeSinceLastDetection = 0f;
            }
        }
        else
        {
            timeSinceLastDetection += Time.deltaTime;
            if (timeSinceLastDetection >= detectionGracePeriod)
            {
                StopFillingCurrentBucket();
            }
        }

        if (showRaycast)
        {
            Debug.DrawRay(leakPos.position, Vector3.down * raycastDistance, hitBucket ? Color.green : Color.red);
        }
    }

    private void StopFillingCurrentBucket()
    {
        if (_currentBucketFill != null)
        {
            _currentBucketFill.StopFilling();
            _currentBucketFill.SetUnderPipe(false); 
            _currentBucketFill = null;
        }
    }

    private void OnValidate()
    {
        if (leakPos == null)
        {
            Debug.LogWarning("PipeLeak: LeakPos not assigned! Please assign in Inspector.", this);
        }
    }
}
