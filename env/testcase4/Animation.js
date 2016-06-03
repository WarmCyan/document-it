//*************************************************************
//  File: Animation.js
//  Date created: 6/27/2015
//  Date edited: 6/29/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Class that takes care of keeping track of animation
//    time, key frames, etc.
//*************************************************************

//animation types: (which return type)
//	color (hexidecimal, but do NOT include # sign)
//	number
//	matrix (array of arrays, two dimensional array)


/*********************************{
@cl: Animation
@d: Generic animation class that can handle linear interpolation of keyframes. It works by taking in a certain type of data, (hex-color, number, or a matrix [array of arrays]) and any number of keyframes for that data. Then, whenever the update function is called, it calculates the current set of data based on linear interpolation for the current time between surrounding keyframes. Although this comes packaged with the engine specifically for text animations, THIS CLASS CAN BE USED GENERICALLY TO LINEARLY ANIMATE ANY PROPERTY that can be defined as a hex-color, number, or matrix.
}*********************************/

/*********************************{
@c: Animation
@i:
	@v:type @d:Type of data that will be input. (Pass in a string, it will currently take "color", "number", or "matrix")
	@v:userType @d:This is meant purely to be a user-defined variable. You can pass in any string, it's intended for the user if an extra piece of information is needed so that whatever class handles animation will know to which property it needs to apply the returned values from this class.
}*********************************/
function Animation(type, userType)
{
	//TODO: ERROR CHECKING
	var type = type; 
	this.userType = userType; //{@p:userType @d: User-defined informational variable meant for when an extra piece of information is needed by the class handling the animation, if it needs further specification as to what property to apply the returned animation value to.}
	this.currentTime = 0; //{@p:currentTime @d: The amount of time (in milliseconds) that has transpired in the animation. (Assuming passed in dt's are accurate.) Keyframe time intervals are compared to this variable, so this variable can be used to control where in an animation's timeline you are.}

	this.done = false; //{@p:done @d: Variable representing whether the animation has completed or not (if currentTime has passed the last keyframe's time. This variable is primarily meant for checking animations for removal.}
	
	var keyframes = [];
	var keyframeIntervals = []; //assoc array that contains time in ms since beginning of animation before it applies (each one should therefore be consecutively larger than the former.)

	/*********************************{
	@f: addKeyFrame
	@d: Adds the desired keyframe data and time to the animation's arrays.
	@i:
		@v:keyFrameData @d: The data/value(s) you want to be applied to the property. NOTE: The type of this data MUST match the data type passed in when you initially create the Animation object
		@v:time @d: The time in milliseconds at which you want the specified data/value(s) to be applied in the animation. NOTE: this is a "global animation" time, it represents the time from the START of the animation, NOT the last keyframe.
	}*********************************/
	this.addKeyFrame = function(keyFrameData, time)
	{
		keyframes.push(keyFrameData);
		keyframeIntervals.push(time);
		this.keyFrames = keyframes;
	}

	//THIS is the function that will also return "current frame"
	/*********************************{
	@f: updateAnimation
	@d: Calculates the data/value(s) of the animation for the current time through linear interpolation of the surrounding keyframes. Note that once the time for the final keyframe is passed by the currentTime variable, the done variable will be set to true.
	@i:
		@v:dt @d: Change in time, or amount of time in milliseconds since the last time this function was called. NOTE that this means the animation is TIME BASED, not frame based. (Meaning animations will run at the exact same speed no matter the frame rate.)
	@o: This function returns the data/value(s) for the current point in time of the animation. NOTE: For "color" type animations, it returns the hex code WITHOUT a starting hash "#".
	}*********************************/
	this.updateAnimation = function(dt)
	{
		if (this.done) { return null; }
		
		this.currentTime += dt;
		var frameOne = null; //INDEX 
		var frameTwo = null; //INDEX
		
		//check for end of animation reached
		if (this.currentTime > keyframeIntervals[keyframeIntervals.length - 1])
		{
			this.done = true;
			return keyframes[keyframes.length - 1];
		}
		
		//for loop to find which frames time is currently between
		for (var i = 0; i < keyframeIntervals.length; i++)
		{
			if (this.currentTime > keyframeIntervals[i] && this.currentTime < keyframeIntervals[i+1])
			{
				frameOne = i;
				frameTwo = i+1;
				break;
			}
			//if exactly on a keyframe
			else if (this.currentTime == keyframeIntervals[i]) { return keyframes[i]; log("EXACT FRAME");}
			else 
			{
				continue;
			}
		}

		//find ratio of times
		var ratio = (keyframeIntervals[frameTwo] - keyframeIntervals[frameOne]) / (this.currentTime - keyframeIntervals[frameOne]);

		//based on type, apply ratio differently
		if (type == "number")
		{
			return ((keyframes[frameTwo] - keyframes[frameOne]) / ratio) + keyframes[frameOne];
		}
		else if (type == "matrix")
		{
			var matrix = [];
			
			//populate basic arrays
			for (var i = 0; i < keyframes[frameOne].length; i++)
			{
				matrix[i] = [];

				for (var j = 0; j < keyframes[frameOne][i].length; j++)
				{
					matrix[i][j] = ((keyframes[frameTwo][i][j] - keyframes[frameOne][i][j]) / ratio) + keyframes[frameOne][i][j];
				}
			}
			return matrix;
		}
		else if (type == "color")
		{
			var colorValues1 = getHexValues(keyframes[frameOne]);
			var colorValues2 = getHexValues(keyframes[frameTwo]);

			var r = 0;
			var g = 0;
			var b = 0;
			
			//perform math on each individual color value
			r = parseInt(((colorValues2[0] - colorValues1[0]) / ratio) + colorValues1[0]);
			g = parseInt(((colorValues2[1] - colorValues1[1]) / ratio) + colorValues1[1]);
			b = parseInt(((colorValues2[2] - colorValues1[2]) / ratio) + colorValues1[2]);
			
			return getColorHex(r, g, b);
		}
	}

	//hex conversion dictionary
	var dictionary = [];
	dictionary[0] = "0";
	dictionary[1] = "1";
	dictionary[2] = "2";
	dictionary[3] = "3";
	dictionary[4] = "4";
	dictionary[5] = "5";
	dictionary[6] = "6";
	dictionary[7] = "7";
	dictionary[8] = "8";
	dictionary[9] = "9";
	dictionary[10] = "A";
	dictionary[11] = "B";
	dictionary[12] = "C";
	dictionary[13] = "D";
	dictionary[14] = "E";
	dictionary[15] = "F";

	//returns the complete 6 character hex code from the 3 base 10 color values
	function getColorHex(r, g, b) { return getHexPairFromValue(r) + getHexPairFromValue(g) + getHexPairFromValue(b);	}

	//converts base 10 to hex code
	function getHexPairFromValue(value)
	{
		var sixteenth = parseInt(value / 16);
		var remainder = value - (sixteenth*16);

		var hex1 = dictionary[sixteenth];
		var hex2 = dictionary[remainder];
		return hex1 + hex2;
	}
	
	//returns base 10 values in array of a 6 character hex code
	function getHexValues(hex)
	{
		hex = hex.toUpperCase();
		var hr = hex.substring(0, 2);
		var hg = hex.substring(2, 4);
		var hb = hex.substring(4, 6);

		var r = convertHexValue(hr);
		var g = convertHexValue(hg);
		var b = convertHexValue(hb);

		return [r, g, b];
	}

	//digit/letter pair (0-255) (returns in base 10)
	function convertHexValue(hex)
	{
		var hex1 = hex.substring(0,1);
		var hex2 = hex.substring(1,2);

		var hex1Value = convertSingleHex(hex1);
		var hex2Value = convertSingleHex(hex2);

		hex1Value *= 16;
		var total = parseInt(parseInt(hex1Value) + parseInt(hex2Value));

		return total;
	}
	
	//single digit/letter (returns in base 10)
	function convertSingleHex(hex)
	{
		for (var i = 0; i < dictionary.length; i++)
		{
			if (dictionary[i] == hex) { return i; }
		}
		return -1;
	}
}
