//*************************************************************
//  File: Text.js
//  Date created: 6/21/2015
//  Date edited: 6/30/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Class that contains a group of symbols, and all
//    the necessary functions for rendering and animating the symbols
//*************************************************************


/*********************************{
@cl: Text
@d: This class is a collection of @l:TextChar objects and rendering properties. It can be thought of as representing a string, when you want text to be rendered in your canvas, most of the information needs to go into an instance of this class. (NOTE: This engine requires a global "alphabet" array that contains symbols for any characters you want the ability to render.) Keep in mind that fonts are displayed by rendering out small blocks in the shapes of letters (based on matricies that you define.)
}*********************************/

/*********************************{
@c: Text
@i:
	@v:canvasContainer @d: The canvas object that you're rendering to. This is used for calculating relative positions, as well as for getting rendering context.
	@v:blockSizeArray @d: Every block that is rendered in a character has a height and width. You can pass in both simultaneously in an array here. ([0] = width, [1] = height.) Each dimension is in pixels.
	@v:letterSpacing @d: The space (in pixels) between each character in the string.
	@v:useRelPos @d: Boolean value representing whether relative fractional positions or absolute pixel positions should be used. (True for relative, false for absolute.)
}*********************************/
function Text(canvasContainer, blockSizeArray, letterSpacing, useRelPos)
{
	//TODO: alphabet may be stored in here? and then matrix height calculated from that?
	this.CHAR_MATRIX_HEIGHT = 14; //{@cnst:CHAR_MATRIX_HEIGHT @d: The height of every character matrix. NOTE that this needs to be the same for EVERY character in your alphabet. When designing custom fonts, keep this in mind. The width of each matrix can be variable, but height MUST be consistent.} 

	this.container = canvasContainer; //{@p:container @d: the canvas object the text is rendered onto.}
	this.chars = []; //{@p:chars[] @d: The list of @l:TextChar objects. Using this array, you can manually affect settings of individual characters in the string, such as animations, colors, etc.}
	var textString = "";

	this.blockMargin = 0; //{@p:blockMargin @d: The margin in pixels given to each rendered block. (Pads the blocks so there's space between them.) Default is 0.}
	this.blockSize = blockSizeArray; //{@p:blockSize[] @d: Array holding the size specifications for each rendered block. [0] = width [1] = height}
	this.letterSpacing = letterSpacing; //{@p:letterSpacing @d: The space (in pixels) put between each rendered letter.}
	this.lineSpacing = 0; //{@p:lineSpacing @d: The space (in pixels) put between each line of text. (Only taken into account when multiple lines required.) Default is 0.}
	this.textMargin = [0,0]; //{@p:textMargin[] @d: Array representing the amount of space (in pixels) given on each side of the entire text object. [0] = x-margin [1] = y-margin. Default is [0,0].}
	
	this.useRelPos = useRelPos; //{@p:useRelPos @d: Boolean representing whether to use relative fractional positioning (true) or absolute pixel postitioning (false.)}
	this.relPosStartX = [0,1]; //{@p:relPosStartX[] @d: Relative fractional positioning array for left text boundary. ([0] = numerator, [1] = denominator) Fractional positioning works by taking the amount of world space (based on the canvas container), dividing by the denominator, and multiplying by the numerator. In other words, [1,2] would mean that the left limit of the text would be exactly in the center of the canvas. (Use [0,1] if you want to start at the extreme left edge.)}
	this.relPosStartY = [0,1]; //{@p:relPosStartY[] @d: Relative fractional positioning array for upper text boundary. ([0] = numerator, [1] = denominator) Fractional positioning works by taking the amount of world space (based on the canvas container), dividing by the denominator, and multiplying by the numerator. In other words, [1,2] would mean that the upper limit of the text would be exactly in the middle of the canvas. (Use [0,1] if you want to start at the extreme top edge.)}
	this.relPosEndX = [0,1]; //{@p:relPosEndX[] @d: Relative fractional positioning array for right text boundary. ([0] = numerator, [1] = denominator) Fractional positioning works by taking the amount of world space (based on the canvas container), dividing by the denominator, and multiplying by the numerator. In other words, [1,2] would mean that the right limit of the text would be exactly in the center of the canvas. (Use [1,1] if you want to end at the extreme right edge.) NOTE: If the rendered text surpasses this limit, it will be wrapped into multiple lines until it fits x-wise.}
	this.relPosEndY = [0,1]; //{@p:relPosEndY[] @d: Relative fractional positioning array for bottom text boundary. ([0] = numerator, [1] = denominator) Fractional positioning works by taking the amount of world space (based on the canvas container), dividing by the denominator, and multiplying by the numerator. In other words, [1,2] would mean that the bottom limit of the text would be exactly in the middle of the canvas. (Use [1,1] if you want to end at the extreme bottom edge.) NOTE: If text is wrapped into multiple lines, it may surpass this limit.}

	this.alignX = "left"; //{@p:alignX @d: Horizontal alignment of the text within given limits. Set as either "left", "center", or "right". Default is "left".}
	this.alignY = "top"; //{@p:alignY @d: Vertical alignment of the text within given limits. Set as either "top", "center", or "bottom". Default is "top".}
	
	//the relative amounts ARE TRANSLATED INTO the absolute. But you have the option to manually put in the amounts yourself
	this.absPosStartX = null; //{@p:absPosStartX @d: Absolute pixel position of left text boundary. NOTE: When useRelPos is set to TRUE, all passed relative fractional positions are calculated and the results are stored in the absolute positioning variables. Any manually set absolute positions WILL NOT AFFECT the text while useRelPos is true.}
	this.absPosStartY = null; //{@p:absPosStartY @d: Absolute pixel position of top text boundary. NOTE: When useRelPos is set to TRUE, all passed relative fractional positions are calculated and the results are stored in the absolute positioning variables. Any manually set absolute positions WILL NOT AFFECT the text while useRelPos is true.}
	this.absPosEndX = null; //{@p:absPosEndX @d: Absolute pixel position of right text boundary. NOTE: When useRelPos is set to TRUE, all passed relative fractional positions are calculated and the results are stored in the absolute positioning variables. Any manually set absolute positions WILL NOT AFFECT the text while useRelPos is true. (Also note that if the rendered text surpasses this limit, it will be wrapped into multiple lines until it fits x-wise.)}
	this.absPosEndY = null; //{@p:absPosEndY @d: Absolute pixel position of bottom text boundary. NOTE: When useRelPos is set to TRUE, all passed relative fractional positions are calculated and the results are stored in the absolute positioning variables. Any manually set absolute positions WILL NOT AFFECT the text while useRelPos is true. (Also note that the text is not required to fit within this limit, and if wrapped enough, it can pass the boundary.)}

	/*********************************{
	@f: setRelativePositions
	@d: A fast way to quickly set all relative fractional positions.
	@i:
		@v:startX @d: Fractional left boundary array ([0] = numerator, [1] = denominator.)
		@v:endX @d: Fractional right boundary array ([0] = numerator, [1] = denominator.)
		@v:startY @d: Fractional top boundary array ([0] = numerator, [1] = denominator.)
		@v:endY @d: Fractional bottom boundary array ([0] = numerator, [1] = denominator.)
	}*********************************/
	this.setRelativePositions = function(startX, endX, startY, endY)
	{
		this.relPosStartX = startX;
		this.relPosEndX = endX;
		this.relPosStartY = startY;
		this.relPosEndY = endY;
	}
	
	/*********************************{
	@f: setTextString
	@d: Change the string that the text should render. NOTE: This function calls calculateTextChars(), so you do not need to manually call that function every time you change the text.
	@i:
		@v:text @d: The new text for the string.
	}*********************************/
	this.setTextString = function(text)
	{
		textString = String(text);
		this.chars = [];
		this.calculateTextChars();
	}
	
	/*********************************{
	@f: getTextString
	@d: returns what string the text is currently rendering
	@o: returns what string the text is currently rendering
	}*********************************/
	this.getTextString = function() { return textString; }

	//shouldn't be public function, not including in documentation...
	this.findTextHeight = function(numOfLines)
	{
		var height = numOfLines * (this.blockSize[1]*this.CHAR_MATRIX_HEIGHT + this.blockMargin*(this.CHAR_MATRIX_HEIGHT - 1)) + (numOfLines-1)*this.lineSpacing;
		height += this.textMargin[1] * 2;
		return height;
	}
	
	//shouldn't be public function, not including in documentation...
	this.findLineWidth = function(text)
	{
		var width = 0;
		for (var i = 0; i < text.length; i++)
		{
			var letter = alphabet[text[i]];
			if (letter == undefined) { letter = alphabet["unknown"]; }
			var numBlocks = letter.width;
			width += (this.blockSize[0] * numBlocks + this.blockMargin * (numBlocks - 1));
		}
		width += (this.textMargin[0] * 2) + (this.letterSpacing * (text.length - 1));
		return width;
	}
	
	/*********************************{
	@f: calculateTextChars
	@d: Calculates all necessary positions, widths, etc, and creates @l:TextChar instances accordingly. This should only ever be called manually if you manually adjust a property without changing the text string.
	}*********************************/
	this.calculateTextChars = function()
	{
		//calculate absPos values
		if (this.useRelPos)
		{
			var totalWidth = this.container.width;	
			var totalHeight = this.container.height;

			this.absPosStartX = (totalWidth / this.relPosStartX[1]) * this.relPosStartX[0];
			this.absPosStartY = (totalHeight / this.relPosStartY[1]) * this.relPosStartY[0];
			this.absPosEndX = (totalWidth / this.relPosEndX[1]) * this.relPosEndX[0];
			this.absPosEndY = (totalHeight / this.relPosEndY[1]) * this.relPosEndY[0];
		}
		
		//multiline stuff
		var lines = [];
		var lineWidths = [];

		lines[0] = textString;

		//find allowed dimensions
		var allowedWidth = this.absPosEndX - this.absPosStartX;
		var allowedHeight = this.absPosEndY - this.absPosStartY;
		

		var newLineLN = -1;
		for (var i = 0; i < lines.length; i++)
		{
			if (lines[i].indexOf("\n") > -1)
			{
				var index = lines[i].indexOf("\n");
				if (lines[i+1] == undefined) { lines[i+1] = ""; }
				lines[i+1] = lines[i].substring((index + 1), lines[i].length) + lines[i+1];
				lines[i] = lines[i].substring(0, index);
				newLineLN = i+1;
			}
			while (this.findLineWidth(lines[i]) > allowedWidth)
			{
				if (i+1 == newLineLN)
				{
					if (lines[i+2] == undefined) { lines[i+2] = ""; }
					lines[i+2] = lines[i+1];
					lines[i+1] = "";
					newLineLN = i+2;
				}

				//get the last character of the current line and add it to the beginning of the next
				var lastChar = lines[i].substring(lines[i].length - 1, lines[i].length);
				lines[i] = lines[i].substring(0, lines[i].length - 1);
				if (lines[i+1] == undefined) { lines[i+1] = ""; }
				lines[i+1] = lastChar + lines[i+1];
			}
			lineWidths[i] = this.findLineWidth(lines[i]);
		}

		
		if (this.findTextHeight(lines.length) > allowedHeight) { /*log("ERROR - NOT ENOUGH SPACE FOR TEXT");*/ }
			
		//evaluating positional variables
		var evalX = 0;
		var evalY = 0;

		//find starting positions
		if (this.alignX == "center") { evalX = this.absPosStartX  + (allowedWidth - lineWidths[0]) / 2; } 
		else if (this.alignX == "left") { evalX = this.absPosStartX + this.textMargin[0]; }
		else if (this.alignX == "right") { evalX = this.absPosEndX - lineWidths[0]; }
		else { log("NOTHING FOUND FOR evalX"); } //DEBUGGING ONLY
		
		if (this.alignY == "center") { evalY = this.absPosStartY  + (allowedHeight - this.findTextHeight(lines.length)) / 2; }
		else if (this.alignY == "top") { evalY = this.absPosStartY + this.textMargin[1]; }
		else if (this.alignY == "bottom") { evalY = this.absPosEndY - this.findTextHeight(lines.length); }

		for (var line = 0; line < lines.length; line++)
		{
			//advance evalY forward for previous line
			if (line > 0)
			{
				var lineForwardAmount = this.blockSize[1]*this.CHAR_MATRIX_HEIGHT + this.blockMargin*(this.CHAR_MATRIX_HEIGHT - 1);
				evalY += lineForwardAmount + this.lineSpacing;
				
				if (this.alignX == "center") { evalX = this.absPosStartX + this.textMargin[0] + (allowedWidth - lineWidths[line]) / 2; }
				else if (this.alignX == "left") { evalX = this.absPosStartX + this.textMargin[0]; }
				else if (this.alignX == "right") { evalX = this.absPosEndX - lineWidths[line]; }
				else { log("NOTHING FOUND FOR evalX"); } //DEBUGGING ONLY
			}
			for (var l = 0; l < lines[line].length; l++)
			{

				//start instance of textChar
				var textCharacter = new TextChar(this);

				var letter = alphabet[lines[line][l]];
				if (letter == undefined) { letter = alphabet["unknown"]; }
				var mat = letter.matrix;
				textCharacter.assocSymbol = letter;
				textCharacter.blockSize = this.blockSize;

				var maxX = letter.width;
				for (var y = 0; y < this.CHAR_MATRIX_HEIGHT; y++)
				{
					textCharacter.positionMatrix[y] = [];
					for (var x = 0; x < maxX; x++)
					{
						textCharacter.positionMatrix[y][x] = [];
						posX = evalX + (this.blockSize[0]*x) + (this.blockMargin*x);
						posY = evalY + (this.blockSize[1]*y) + (this.blockMargin*y);
						textCharacter.positionMatrix[y][x][0] = parseInt(posX);
						textCharacter.positionMatrix[y][x][1] = parseInt(posY);
					}
				}
				this.chars.push(textCharacter);
				
				//update evalx
				var letter = alphabet[lines[line][l]];
				if (letter == undefined) { letter = alphabet["unknown"]; }
				var numBlocks = letter.width;
				var letterForwardAmount = this.blockSize[0] * numBlocks + this.blockMargin*(numBlocks-1);
				evalX += letterForwardAmount + this.letterSpacing;
			}
		}
	}

	//dt is for any animations
	/*********************************{
	@f: renderFont
	@d: Call this function to actually have the engine render out all of the characters.
	@i:
		@v:dt @d:Change in time, or amount of time in milliseconds since the last time this function was called.Note that this is only necessary to pass through to individual character animations. If you aren't using any animations, it doesn't matter what you pass in here.
	}*********************************/
	this.renderFont = function(dt)
	{
		var context = this.container.getContext('2d');
		for (var c = 0; c < this.chars.length; c++) { this.chars[c].render(dt, context); }
	}
}
