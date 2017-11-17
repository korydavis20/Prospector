using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {
	[Header ("Set in Inspector")]
	public bool startFaceUp = false;

	//Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
	
	// Prefabs
	public GameObject prefabSprite;
	public GameObject prefabCard;

	[Header ("Set Dynamically") ]

	public PT_XMLReader					xmlr;
	// add from p 569
	public List<string>					cardNames;
	public List<Card>					cards;
	public List<Decorator>				decorators;
	public List<CardDefinition>			cardDefs;
	public Transform					deckAnchor;
	public Dictionary<string, Sprite>	dictSuits;


	// called by Prospector when it is ready
	public void InitDeck(string deckXMLText) {
		// from page 576
		if( GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		// init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};
		
		
		
		// -------- end from page 576
		ReadDeck (deckXMLText);
		MakeCards();
	}


	// ReadDeck parses the XML file passed to it into Card Definitions
	public void ReadDeck(string deckXMLText){


		xmlr = new PT_XMLReader ();
		xmlr.Parse (deckXMLText);

		// print a test line
		string s = "xml[0] decorator [0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		print (s);

		//Read decorators for all cards
		// these are the small numbers/suits in the corners
		decorators = new List<Decorator>();
		// grab all decorators from the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i = 0; i < xDecos.Count; i++) {
			// for each decorator in the XML, copy attributes and set up location and flip if needed
			deco = new Decorator ();
			deco.type = xDecos [i].att ("type");
			deco.flip = (xDecos [i].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			deco.scale = float.Parse (xDecos [i].att ("scale"));
			deco.loc.x = float.Parse (xDecos [i].att ("x"));
			deco.loc.y = float.Parse (xDecos [i].att ("y"));
			deco.loc.z = float.Parse (xDecos [i].att ("z"));
			decorators.Add (deco);
		}

			// read pip locations for each card rank
			// read the card definitions, parse attribute values for pips
			cardDefs = new List<CardDefinition>();
			PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

			for (int i=0; i<xCardDefs.Count; i++) {
				// for each carddef in the XML, copy attributes and set up in cDef
				CardDefinition cDef = new CardDefinition();
				cDef.rank = int.Parse(xCardDefs[i].att("rank"));

				PT_XMLHashList xPips = xCardDefs[i]["pip"];
				if (xPips != null) {			
					for (int j = 0; j < xPips.Count; j++) {
						deco = new Decorator();
						deco.type = "pip";
						deco.flip = (xPips[j].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0

						deco.loc.x = float.Parse (xPips[j].att("x"));
						deco.loc.y = float.Parse (xPips[j].att("y"));
						deco.loc.z = float.Parse (xPips[j].att("z"));
						if(xPips[j].HasAtt("scale") ) {
							deco.scale = float.Parse (xPips[j].att("scale"));
						}
						cDef.pips.Add (deco);
					} // for j
				}// if xPips

				// if it's a face card, map the proper sprite
				// foramt is ##A, where ## in 11, 12, 13 and A is letter indicating suit
				if (xCardDefs[i].HasAtt("face")){
					cDef.face = xCardDefs[i].att ("face");
				}
				cardDefs.Add (cDef);
			} // for i < xCardDefs.Count
	} // ReadDeck
	
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank
	
	
	public void MakeCards() {
		// stub Add the code from page 577 here
		cardNames = new List<string>();
		string[] letters = new string[] {"C","D","H","S"};
		foreach (string s in letters) {
			for (int i =0; i<13; i++) {
				cardNames.Add(s+(i+1));
			}
		}
		
		// list of all Cards
		cards = new List<Card>();

		for(int i = 0; i < cardNames.Count; i++){
			cards.Add(MakeCard(i));
		}
		
		// temp variables
		/*Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;  // so tempted to make a D&D ref here...
		
		for (int i=0; i<cardNames.Count; i++) {
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			cgo.transform.parent = deckAnchor;
			Card card = cgo.GetComponent<Card>();
			
			cgo.transform.localPosition = new Vector3(i%13*3, i/13*4, 0);
			
			card.name = cardNames[i];
			card.suit = card.name[0].ToString();
			card.rank = int.Parse (card.name.Substring (1));
			
			if (card.suit =="D" || card.suit == "H") {
				card.colS = "Red";
				card.color = Color.red;
			}
			
			card.def = GetCardDefinitionByRank(card.rank);
			
			// Add Decorators
			foreach (Decorator deco in decorators) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				if (deco.type == "suit") {
					tSR.sprite = dictSuits[card.suit];
				} else { // it is a rank
					tS = rankSprites[card.rank];
					tSR.sprite = tS;
					tSR.color = card.color;
				}
				
				tSR.sortingOrder = 1;                     // make it render above card
				tGO.transform.parent = cgo.transform;     // make deco a child of card GO
				tGO.transform.localPosition = deco.loc;   // set the deco's local position
				
				if (deco.flip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (deco.scale != 1) {
					tGO.transform.localScale = Vector3.one * deco.scale;
				}
				
				tGO.name = deco.type;
				
				card.decoGOs.Add (tGO);
			} // foreach Deco
			
			
			//Add the pips
			foreach(Decorator pip in card.def.pips) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tGO.transform.parent = cgo.transform; 
				tGO.transform.localPosition = pip.loc;
				
				if (pip.flip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (pip.scale != 1) {
					tGO.transform.localScale = Vector3.one * pip.scale;
				}
				
				tGO.name = "pip";
				tSR = tGO.GetComponent<SpriteRenderer>();
				tSR.sprite = dictSuits[card.suit];
				tSR.sortingOrder = 1;
				card.pipGOs.Add (tGO);
			}
			
			//Handle face cards
			if (card.def.face != "") {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				
				tS = GetFace(card.def.face+card.suit);
				tSR.sprite = tS;
				tSR.sortingOrder = 1;
				tGO.transform.parent=card.transform;
				tGO.transform.localPosition = Vector3.zero;  // slap it smack dab in the middle
				tGO.name = "face";
			}
			
			cards.Add (card);
		} // for all the Cardnames	*/

	} // makeCards

	private Card MakeCard(int cNum){
		//create new card object
		GameObject cgo = Instantiate(prefabCard) as GameObject;
		cgo.transform.parent = deckAnchor;
		Card card = cgo.GetComponent<Card>();

		//this stacks the cards so that they are all in rows
		cgo.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 8);

		//assign basic values to the Card
		card.name = cardNames[cNum];
		card.suit = card.name[0].ToString();
		card.rank = int.Parse(card.name.Substring(1));
		if(card.suit == "D" || card.suit == "H"){
			card.colS = "Red";
			card.color = Color.red;
		}

		//pull the CardDefinition
		card.def = GetCardDefinitionByRank(card.rank);

		AddDecorators(card);
		AddPips (card);
		AddFace (card);
		AddBack (card);

		return card;
	}

	private Sprite _tSp = null;
	private GameObject _tGO = null;
	private SpriteRenderer  _tSR = null;

	private void AddDecorators(Card card) { // a
		// Add Decorators
		foreach( Decorator deco in decorators ) {
			if (deco.type == "suit") {
				// Instantiate a Sprite GameObject
				_tGO = Instantiate( prefabSprite ) as GameObject;
				// Get the SpriteRenderer Component
				_tSR = _tGO.GetComponent<SpriteRenderer>();
				// Set the Sprite to the proper suit
				_tSR.sprite = dictSuits[card.suit];
			} else {
				_tGO = Instantiate( prefabSprite ) as GameObject;
				_tSR = _tGO.GetComponent<SpriteRenderer>();
				// Get the proper Sprite to show this rank
				_tSp = rankSprites[ card.rank ];
				// Assign this rank Sprite to the SpriteRenderer
				_tSR.sprite = _tSp;
				// Set the color of the rank to match the suit
				_tSR.color = card.color;
			}
			// Make the deco Sprites render above the Card
			_tSR.sortingOrder = 1;
			// Make the decorator Sprite a child of the Card
			_tGO.transform.SetParent( card.transform );
			// Set the localPosition based on the location from DeckXML
			_tGO.transform.localPosition = deco.loc;
			// Flip the decorator if needed
			if (deco.flip) {
				// An Euler rotation of 180° around the Z-axis will flip it
				_tGO.transform.rotation = Quaternion.Euler(0,0,180);
			}
			// Set the scale to keep decos from being too big
			if (deco.scale != 1) {
				_tGO.transform.localScale = Vector3.one * deco.scale;
			}
			// Name this GameObject so it's easy to see
			_tGO.name = deco.type;
			// Add this deco GameObject to the List card.decoGOs
			card.decoGOs.Add(_tGO);
		}
	}

	private void AddPips(Card card){
		//for each pips in the definition....
		foreach(Decorator pip in card.def.pips){
			//instaniate a sprite gameobject
			_tGO = Instantiate(prefabSprite) as GameObject;
			//set the parent to be the card gameobject
			_tGO.transform.SetParent(card.transform);
			//set the position to that specified in the XML
			_tGO.transform.localPosition = pip.loc;
			//flip it if necessary
			if (pip.flip) {
				_tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
			}
			//scale it if necessary
			if (pip.scale != 1) {
				_tGO.transform.localScale = Vector3.one * pip.scale;
			}
			//Give this gameobject a name
			_tGO.name = "pip";
			//get SpriteRenderer co,ponent
			_tSR = _tGO.GetComponent<SpriteRenderer>();
			//set the sprite to the proper suit
			_tSR.sprite = dictSuits[card.suit];
			//set sortingOrder so the pip is rendered above the Card_Front
			_tSR.sortingOrder = 1;
			//Add this to the Card's list of pips
			card.pipGOs.Add(_tGO);
		}
	}

	private void AddFace(Card card){
		if (card.def.face == "") {
			return; // no need to run if it isn't a face card
		}

		_tGO = Instantiate (prefabSprite) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer> ();
		//generate the right name and pass it on to GetFace()
		_tSp = GetFace(card.def.face+card.suit);
		_tSR.sprite = _tSp; // assign this sprite to _tSR
		_tSR.sortingOrder = 1; //set sort order
		_tGO.transform.SetParent(card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		_tGO.name = "face";
	}
		
	//Find the proper face card
	private Sprite GetFace(string faceS) {
		foreach (Sprite _tSp in faceSprites) {
			//if this sprite has the right name
			if (_tSp.name == faceS) {
				return (_tSp);
			}
		}//foreach	
		return (null);  // couldn't find the sprite (should never reach this line)
	 }// getFace 

	private void AddBack(Card card){
		//Add card back
		//the Card_Back will be able to cover everything else on the card
		_tGO = Instantiate(prefabSprite) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer> ();
		_tSR.sprite = cardBack;
		_tGO.transform.SetParent (card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		//This is a higher sortingOrder than anything else
		_tSR.sortingOrder = 2;
		_tGO.name = "back";
		card.back = _tGO;
		//default to face-up
		card.faceUp = startFaceUp; //use the propety of faceUp of Card
	}

	//shuffle the cards in Deck.cards
	static public void Shuffle(ref List<Card> oCards){
		//create tmporary List to hold the new shuffle order
		List<Card> tCards = new List<Card>();
		int ndx; // this will hold the index of the card to be moved
		tCards = new List<Card>();
		//repeat as long as there are cards in the original list
		while(oCards.Count > 0){
			//pick the index of a random card
			ndx = Random.Range(0, oCards.Count);
			//Add that card to the temporary list
			tCards.Add(oCards[ndx]);
			//and remove that card from the original List
			oCards.RemoveAt(ndx);
		}
			//replace the original List with the temporary List
		oCards = tCards;
		//Because oCards is a reference (ref) parameter, the original argument 
		//that was passed in is changed as well.
	}
	
} // Deck class
