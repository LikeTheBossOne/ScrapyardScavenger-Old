﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PlayerHUD : MonoBehaviourPunCallbacks
{
	#region Private Fields


	[Tooltip("UI Slider to display Player's Health")]
	[SerializeField]
	private Slider playerHealthSlider;

    [Tooltip("UI Text to display Player's Ammo Count")]
    [SerializeField]
	private Text playerAmmoCount;

    #endregion

	void Start() {
		playerHealthSlider = GameObject.FindWithTag("Health").GetComponent<Slider>();
        playerAmmoCount = GameObject.FindWithTag("AmmoCount").GetComponent<Text>();

		// The photon view is mine check is necessary here, otherwise everyone's health bar will be reset
		if (!photonView.IsMine) return;
		playerHealthSlider.value = 100;

        Gun startGun = GetComponent<PlayerControllerLoader>().equipmentManager.getCurrentEquipment() as Gun;
        if (startGun != null)
        {
            AmmoChanged(startGun.baseClipSize, startGun.baseClipSize);
		}
    }

	void Update()
	{
		// If not me, don't update!
		if (!photonView.IsMine) return;
    }

	#region Public Methods

	public Slider getHealthSlider() {
		return this.playerHealthSlider;
	}

	public void takeDamage(float dmg) {
		if (getHealthSlider().value > 0)
			playerHealthSlider.value -= dmg;
	}

	public void heal(float healAmt) {
		if (photonView.IsMine)
		{
			playerHealthSlider.value += healAmt;
		}
	}

    public void AmmoChanged(int ammoCount, int maxAmmo)
    {
        if (photonView.IsMine)
        {
            playerAmmoCount.text = $"Ammo: {ammoCount}/{maxAmmo}";
		}
    }


	#endregion
}
