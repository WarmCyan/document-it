//*************************************************************
//  File: TextChar.js
//  Date created: 6/22/2015
//  Date edited: 6/30/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Class for a single character INSIDE OF A TEXT. These objects hold positioning and rendering information and take care of updating any animations
//*************************************************************

/*********************************{
@cl: TextChar
@d: Class for a single character inside of a text object. An instance of this class holds positioning, rendering, and animation information for an individual character.
}*********************************/

/*********************************{
@c: TextChar
@i:
	@v:text @d: The Text object that contains this textchar. (needed to access the constants from Text)
}*********************************/
function TextChar(text)
{
	this.textContainer = text; //{@p:textContainer @d: The Text object that contains the current textchar.}
	this.positionMatrix = []; //{@p:positionMatrix[] @d: The matrix containing the x and y coordinates for each block of the letter.}
	this.assocSymbol; //{@p:assocSymbol @d: The symbol that this textchar is based on.}
	this.color = "#000000"; //{@p:color @d: The hex-color code for the letter. (Note that this value is a string that must start with the hashtag "#".) Default is "#000000".}
	this.opacity = 1; //{@p:opacity @d: The opacity of each rendered block. Default is 1.}
	this.blockSize = []; //{@p:blockSize[] @d: The array containing the x and y dimensions of each rendered block. ([0] = width [1] = height.)}
	
	this.animations = []; //{@p:animations[] @d: The array of current applied @l:Animation instances. To start an animation on a textchar, just push it into this array.}
	
	//DO NOT TOUCH THESE. HANDLED WITHIN THE CODE, NO NEED TO EDIT MANUALLY.
	this.relativeAnimationMatrixX = []; //only for positional animation
	this.relativeAnimationMatrixY = []; //only for positional animation
	this.useRelativeMatrix = false; //set to true when doing positional animation (remember to set to false when animation is complete)

	/*********************************{
	@f: render
	@d: This function should be called by the Text that contains the textchar. It handles rendering out each individual block to the canvas.
	@i:
		@v:dt @d: Change in time, or amount of time in milliseconds since the last time this function was called (necessary for any animations that are running.)
		@v:context @d: The canvas 2d context you wish to render the text to.
	}*********************************/
	this.render = function(dt, context)
	{
		//update current variables based on animations
		if (this.animations.length > 0)
		{
			for (var i = 0; i < this.animations.length; i++)
			{
				//TODO: add in block spacing as well (and char margins))
				var result = this.animations[i].updateAnimation(dt);
				if (this.animations[i].userType == "blockWidth")
				{
					this.blockSize[0] = result;
				}
				else if (this.animations[i].userType == "blockHeight")
				{
					this.blockSize[1] = result;
				}
				else if (this.animations[i].userType == "positionRelX")
				{
					this.useRelativeMatrix = true;
					this.relativeAnimationMatrixX = result;
				}
				else if (this.animations[i].userType == "positionRelY")
				{
					this.useRelativeMatrix = true;
					this.relativeAnimationMatrixY = result;
				}
				else if (this.animations[i].userType == "opacity")
				{
					this.opacity = result;
				}
				else if (this.animations[i].userType == "blockColor")
				{
					this.color = "#" + result;
				}
			}

			//check all animations and remove if done
			for (var i = this.animations.length - 1; i >= 0; i--)
			{
				if(this.animations[i].done) 
				{ 
					if (this.animations[i].userType == "positionRelX" || this.animations[i].userType == "positionRelY") { this.useRelativeMatrix = false; }
					this.animations.splice(i, 1); 
				}
			}
		}

		//render based on current variables
		context.fillStyle = this.color;
		context.globalAlpha = this.opacity;
		
		for (var y = 0; y < this.textContainer.CHAR_MATRIX_HEIGHT; y++)
		{
			for (var x = 0; x < this.assocSymbol.width; x++)
			{
				if (this.assocSymbol.matrix[y][x] == 1)
				{
					var posDX = 0;
					var posDY = 0;
					
					if (this.useRelativeMatrix) 
					{ 
						if (this.relativeAnimationMatrixX[y] != undefined) 
						{ 
							if (this.relativeAnimationMatrixX[y][x] != undefined) { posDX = this.relativeAnimationMatrixX[y][x]; }
						}
						if (this.relativeAnimationMatrixY[y] != undefined) 
						{ 
							if (this.relativeAnimationMatrixY[y][x] != undefined) { posDY = this.relativeAnimationMatrixY[y][x]; }
						}
					}

					//console.log("posx: " + posDX + " posy: " + posDY);
		
					context.fillRect(this.positionMatrix[y][x][0] + posDX, this.positionMatrix[y][x][1] + posDY, this.blockSize[0], this.blockSize[1]);
				}
			}
		}
	}
}
