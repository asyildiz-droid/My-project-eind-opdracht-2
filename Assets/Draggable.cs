using System;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public Transform trans;

    private bool isDragging = false;
    private AuthManager authManager;

    void Start()
    {
        // ⭐ Zorg dat trans altijd goed staat
        if (trans == null)
            trans = transform;

        // ⭐ Nieuwe Unity manier (geen warning meer)
        authManager = FindFirstObjectByType<AuthManager>();
    }

    void Update()
    {
        if (isDragging)
            trans.position = GetMousePosition();
    }

    private void OnMouseUpAsButton()
    {
        // Klik = toggle drag aan/uit
        isDragging = !isDragging;

        // ⭐ Wanneer je stopt met slepen → opslaan
        if (!isDragging)
        {
            if (authManager != null)
            {
                authManager.SaveWorldObjects();
            }
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector3 positionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        positionInWorld.z = 0;
        return positionInWorld;
    }
}
