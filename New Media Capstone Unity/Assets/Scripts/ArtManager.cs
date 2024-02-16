using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script will handle all of the art for the levels.
//Essentially, we will split up the art before importing it into our game as separate sprites
//Afterwards, we will individually toggle their display on. 
//We will keep the sprites on a highed layer within Unity so they automatically overlap the previous Art.
//Depending on how we are handling the dark area, we might also need a separate function to turn that off.
public class ArtManager : MonoBehaviour
{
    // These variables will keep track of which sections are currently visible 
    public bool artSection1visible = false;
    public bool artSection2visible = false;
    public bool artSection3visible = false;
    public bool artSection4visible = false;
    public bool artSection5visible = false;
    public bool artSection6visible = false;

    // Assuming divisionSections is 6 for now.
    // This number is not really very changeable. Since we need to do the splitting manually.
    public int divisionSections = 6;

    // Reference to the sprites or game objects that represent each art section
    public GameObject[] artSections;

    void Start()
    {
        //Spawn and turn off display for sprites here.
    }

    void Update()
    {
        // Check if the mouse button was clicked
        if (Input.GetMouseButtonDown(0)) // 0 is for left button, 1 for right button, 2 for middle button
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        // Convert mouse position to world position
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Assuming the camera is centered and the sprites cover the entire view equally
        float screenWidth = Camera.main.orthographicSize * 2.0f * Camera.main.aspect;
        float screenHeight = Camera.main.orthographicSize * 2.0f;

        float spriteWidth = 1365f; // Sprite width
        float spriteHeight = 1080f; // Sprite height

        // Calculate single section dimensions based on your setup
        float sectionWidth = screenWidth / 3; // 3 columns
        float sectionHeight = screenHeight / 2; // 2 rows

        // Calculate which section the click is in
        int column = (int)((mousePos.x + screenWidth / 2) / sectionWidth) + 1;
        int row = (int)((mousePos.y + screenHeight / 2) / sectionHeight) + 1;

        int section = (row - 1) * 3 + column; // Calculate section based on row and column

        // Call ShowArtSection for the calculated section
        ShowArtSection(section);
    }

    // I have a plan for making this section check all of the boolean variables and then set the correct ones to display.
    // Although I'm trying to find a more efficient way that would reduce the amount of things this function checks.

    public void ShowArtSection(int section)
    {
        //Safety check
        if (section < 1 || section > divisionSections)
        {
            Debug.LogError("ShowArtSection called with invalid section number.");
            return;
        }

        //Testing Section Console Logs
        Debug.Log($"Section {section} was Called");

        // Toggle visibility
        bool isVisible = !artSections[section - 1].GetComponent<SpriteRenderer>().enabled;
        artSections[section - 1].GetComponent<SpriteRenderer>().enabled = isVisible;

        // Update the corresponding Boolean variable
        switch (section)
        {
            case 1: artSection1visible = isVisible; break;
            case 2: artSection2visible = isVisible; break;
            case 3: artSection3visible = isVisible; break;
            case 4: artSection4visible = isVisible; break;
            case 5: artSection5visible = isVisible; break;
            case 6: artSection6visible = isVisible; break;
        }
    }



}
