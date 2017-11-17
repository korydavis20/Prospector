using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header ("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset 			layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;

	[Header ("Set Dynamically")]
	public Deck					deck;
	public Layout 				layout;
	public List<CardProspector> drawPile;
	public Transform layoutAnchor;
	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;

	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards); // shuffles the deck by reference

		/*Card c;
		for (int cNum = 0; cNum < deck.cards.Count; cNum++) {
			c = deck.cards [cNum];
			c.transform.localPosition = new Vector3 ((cNum % 13) * 3, cNum / 13 * 4, 0);
		}*/

		layout = GetComponent<Layout> (); //Get the Layout Component
		layout.ReadLayout(layoutXML.text);
		drawPile = ConvertListCardsToListCardProspectors (deck.cards);
		LayoutGame ();
	}

	CardProspector Draw(){
		CardProspector cd = drawPile [0]; //pull the 0th CardProspector
		drawPile.RemoveAt(0); // then remove it from list<> drawPile
		return(cd); //And return it
	}

	//LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"
	void LayoutGame(){
		//create an empty GameObject to serve as an anchor for the tableau
		if(layoutAnchor == null){
			GameObject tGO = new GameObject ("_LayoutAnchor");
			// create an empty GameObject named _LayoutAnchor in the heirarchy
			layoutAnchor = tGO.transform; //grab its transform
			layoutAnchor.transform.position = layoutCenter; //position it
		}

		CardProspector cp;
		//follow the layout
		foreach (SlotDef tSD in layout.slotDefs) {
			//iterate through all the slotdefs in the layout.slotDefs as tSD
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3 (layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y, -tSD.layerID);
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = eCardState.tableau;

			tableau.Add (cp);
		}
	}

	List<CardProspector> ConvertListCardsToListCardProspectors (List<Card> lCD){
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach(Card tCD in lCD){
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}	
		return (lCP);
	}
}
