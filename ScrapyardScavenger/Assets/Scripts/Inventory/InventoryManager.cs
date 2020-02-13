﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class InventoryManager : MonoBehaviourPun
{
    // Resources and counts are indexed based on Resource's ID
    [SerializeField]
    public Resource[] resources = null;
    [SerializeField]
    public int[] resourceCounts = null;
    private int resourceIndex;

    // Crafts and counts are indexed based on the CraftableObject's ID
    [SerializeField]
    public Item[] items = null;
    [SerializeField]
    public int[] itemCounts = null;
    private int itemIndex;

    // Crafts and counts are indexed based on the CraftableObject's ID
    [SerializeField]
    public Weapon[] weapons = null;
    [SerializeField]
    public int[] weaponCounts = null;
    private int weaponIndex;

    // Crafts and counts are indexed based on the CraftableObject's ID
    [SerializeField]
    public Armor[] armors = null;
    [SerializeField]
    public int[] armorCounts = null;
    private int armorIndex;

    // InvSet corresponds to whether we are going through items (0), weapons (1), or armors (2)
    private int invSet;

    public bool isOpen;

    void Start()
    {
        isOpen = false;

        resourceIndex = 0;
        itemIndex = 0;
        weaponIndex = 0;
        armorIndex = 0;

        invSet = 0;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
            

        // Check if player is opening/closing inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            if (isOpen)
                Debug.Log("Opened Inventory");
            else
                Debug.Log("Closed Inventory");
        }
            


        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                invSet += 1;
                invSet = mod(invSet, 3);

                if (invSet == 0)
                    Debug.Log("Switched to Items Inventory");
                if (invSet == 1)
                    Debug.Log("Switched to Weapons Inventory");
                if (invSet == 2)
                    Debug.Log("Switched to Armor Inventory");
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                invSet -= 1;
                invSet = mod(invSet, 3);

                if (invSet == 0)
                    Debug.Log("Switched to Items Inventory");
                if (invSet == 1)
                    Debug.Log("Switched to Weapons Inventory");
                if (invSet == 2)
                    Debug.Log("Switched to Armor Inventory");
            }

            short changeSlot = 0;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                changeSlot = 1;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                changeSlot = -1;
            }

            if (changeSlot != 0)
            {
                switch (invSet)
                {
                    case 0:
                        itemIndex += changeSlot;
                        itemIndex = mod(itemIndex, items.Length);
                        Debug.Log($"Item switched to {items[itemIndex].name}");
                        break;
                    case 1:
                        weaponIndex += changeSlot;
                        weaponIndex = mod(weaponIndex, weapons.Length);
                        Debug.Log($"Weapon switched to {weapons[weaponIndex].name}");
                        break;
                    case 2:
                        armorIndex += changeSlot;
                        armorIndex = mod(armorIndex, armors.Length);
                        Debug.Log($"Armor switched to {armors[armorIndex].name}");
                        break;
                    default:
                        break;
                }
            }

            // Craft current item
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (invSet == 0)
                    Craft(items[itemIndex]);
                if (invSet == 1)
                    Craft(weapons[weaponIndex]);
                if (invSet == 2)
                    Craft(armors[armorIndex]);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            resourceCounts[(int)ResourceType.Gauze]++;
            Debug.Log("Adding a Gauze");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            resourceCounts[(int)ResourceType.Disinfectant]++;
            Debug.Log("Adding a Disinfectant");
        }
        
    }

    public int ResourceCount(Resource resource)
    {
        int count = 0;
        foreach (var res in resources)
        {
            if (res.name == resource.name)
            {
                count++;
            }
        }
        return count;
    }

    public bool ContainsResource(Resource resource)
    {
        return (resourceCounts[resource.id] > 0);
    }

    public void PrintCrafts()
    {
        Debug.Log("Printing all crafted objects");
        foreach (CraftableObject craft in items)
        {
            Debug.Log(craft.name);
        }
    }

    public void PrintResources()
    {
        Debug.Log("Printing all resources");
        foreach (Resource res in resources)
        {
            Debug.Log(res.name);
        }
    }

    public bool Craft(CraftableObject craft)
    {
        var recipeMaterials = craft.recipe.resources;

        bool doCrafting = true;
        foreach (var resource in recipeMaterials)
        {
            int resourceCount = resource.amount;
            if (resourceCounts[resource.item.id] < resourceCount)
            {
                doCrafting = false;
                break;
            }

        }

        if (!doCrafting)
        {
            Debug.Log($"Can't Craft {craft.name}");
            return false;
        }

        foreach (var resource in recipeMaterials)
        {
            int resourceCount = resource.amount;
            resourceCounts[resource.item.id] -= resourceCount;

            if (craft is Item)
            {
                itemCounts[craft.id]++;
            }
            else if (craft is Weapon)
            {
                weaponCounts[craft.id]++;
            }
            else if (craft is Armor)
            {
                armorCounts[craft.id]++;
            }
        }

        Debug.Log($"Crafted {craft.name}");
        return true;
    }

    private int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
