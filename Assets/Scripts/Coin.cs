using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int _value = 1;
    [SerializeField] private float _rotationSpeed = 180f;

    private bool _collected;

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        if (other.GetComponent<PlayerController>() != null)
        {
            _collected = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(_value);
            }
            gameObject.SetActive(false);
        }
    }

    public void ResetCoin()
    {
        _collected = false;
        gameObject.SetActive(true);
    }
}
