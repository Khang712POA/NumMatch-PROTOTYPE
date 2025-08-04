using UnityEngine;

public class SaiMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        this.LoadComponents();
        this.loadValue();
    }

    protected virtual void Reset()
    {
        this.LoadComponents();
        this.loadValue();
    }

    protected virtual void Update()
    {
        //For override
    }
    protected virtual void Start()
    {
        //For override
    }
    protected virtual void OnEnable()
    {
        //For override
    }
    protected virtual void OnDisable()
    {
        //For override
    }
    protected virtual void LoadComponents()
    {
        //For override   
    }
    protected virtual void loadValue()
    {
        //for override
    }
}