﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState{
	drawpile,
	tableau,
	target,
	discard
}

public class CardProspector : Card {

	[Header ("Set Dynamically: CardProspector")]
	//this is how you use enum eCardState
	public eCardState state = eCardState.drawpile;
	//the hiddenBy list stores which other cards will keep this one face down
	public List<CardProspector> hiddenBy = new List<CardProspector>();
	//The layoutID matches this card to the tableau XML if it's a tableau card
	public int layoutID;
	//the SlotDef class stores information pulled in from the LayoutXML <slot>
	public SlotDef slotDef;

}