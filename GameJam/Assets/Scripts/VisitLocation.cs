﻿using Sirenix.OdinInspector;
using System.Collections;

public class VisitLocation : PlayerTurn
{
    [Required]
    public LocationPicker Picker;

    [Required]
    public LocationVisitor Visitor;

    public override IEnumerator RunTurn(PlayerInfo player)
    {
        this.Visitor.gameObject.SetActive(false);
        this.Picker.gameObject.SetActive(true);

        yield return this.StartCoroutine(this.Picker.PickLocation(player));

        this.Picker.gameObject.SetActive(false);
        this.Visitor.gameObject.SetActive(true);

        yield return this.StartCoroutine(this.Visitor.VisitLocation(player, (Location)this.Picker.PickedLocation));
    }
}