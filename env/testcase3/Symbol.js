//*************************************************************
//  File: Symbol.js
//  Date created: 6/21/2015
//  Date edited: 7/2/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Class that holds information for a single character
//*************************************************************

/*********************************{
@cl: Symbol
@d: Small class used for creating font alphabets, associating a single character with a block binary matrix. For the font engine to work, you need to create an array of these symbols named "alphabet".
}*********************************/

/*********************************{
@c: Symbol
@i:
	@v:assocChar @d: The single character (passed as a string) that states what letter the matrix represents.
	@v:matrix @d: The array of arrays (aka two dimensional array) with 1's representing the positions where a block should be rendered, and 0's where there shouldn't.
}*********************************/
function Symbol(assocChar, matrix)
{
	this.assocChar = assocChar; //{@p:assocChar @d:The character that states what letter the matrix represents.}
	this.matrix = matrix; //{@p:matrix[][] @d: The array of arrays with 1's representing where a block should be rendered, and 0's where it shouldn't.}
	this.width = matrix[0].length; //@{p:width @d: Represents how wide the character's matrix is.}
}
