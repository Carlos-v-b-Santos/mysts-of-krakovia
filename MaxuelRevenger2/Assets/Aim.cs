using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Aim : MonoBehaviour
{
    private void Update()
    {

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // garante que fique no plano 2D

        // Move suavemente até a posição do mouse
        transform.position = mousePos;
    }
}
