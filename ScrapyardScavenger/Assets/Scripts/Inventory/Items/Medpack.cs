﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Used by the player to recover health
 */
[CreateAssetMenu(fileName = "New Item", menuName = "Items/Medpack")]
public class Medpack : Item
{
    public Medpack(int id)
    {
        this.id = id;
        name = "Medpack";
        description = "Medpack description here";
        icon = null;
    }

    
    public override void Use(InventoryManager manager)
    {
        // use medpack
        manager.GetComponent<Health>().Heal();
        // remove this from the manager?
        manager.RemoveCraft(this);
    }
}
