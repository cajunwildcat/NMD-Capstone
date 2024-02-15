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
    public bool artSection1 = false;
    public bool artSection2 = false;
    public bool artSection3 = false;
    public bool artSection4 = false;
    public bool artSection5 = false;
    public bool artSection6 = false;

    // Assuming divisionSections is 6 for now.
    // This number is not really very changeable. Since we need to do the splitting manually.
    public int divisionSections = 6;

    // Reference to the sprites or game objects that represent each art section
    public GameObject[] artSections;

    void Start()
    {
        //Spawn and turn off display for sprites here.
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

        // Toggle visibility
        bool isVisible = !artSections[section - 1].activeSelf;
        artSections[section - 1].SetActive(isVisible);

        // Update the corresponding Boolean variable
        switch (section)
        {
            case 1: artSection1 = isVisible; break;
            case 2: artSection2 = isVisible; break;
            case 3: artSection3 = isVisible; break;
            case 4: artSection4 = isVisible; break;
            case 5: artSection5 = isVisible; break;
            case 6: artSection6 = isVisible; break;
        }
    }
}
