//*************************************************************
//  File: CollisionManifold.js
//  Date created: 6/5/2015
//  Date edited: 6/17/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Class that stores and computes information about
//    a collision between two objects
//*************************************************************

/*********************************{
@cl: CollisionManifold
@d: This is a class that stores and computes information about a collision between two objects. Every time the @l:PhysicsEngine detects a collision, it creates an instance of this class and stores it in an array of collisions. When it resolves collisions, it goes through that list and uses the information stored in these instances.
}*********************************/

/*********************************{
@c: CollisionManifold
@i:
	@v:obj1 @d: First object involved in collision. (NOTE: This can be any class or object, but it MUST CONTAIN A PHYSICS INSTANCE named 'physics'.) This variable is stored directly into objA, and its physics instance is stored in A.
	@v:obj2 @d: Second object involved in collision. (NOTE: This can be any class or object, but it MUST CONTAIN A PHYSICS INSTANCE named 'physics'.) This variable is stored directly into objB and its physics instance is stored in B.
}*********************************/
function CollisionManifold(obj1, obj2) 
{
	//TODO: ERROR CHECKING!!! (if doesn't find a 'physics' variable)
	this.A = obj1.physics; //{@p:A @d: The @l:physics instance of the first object.}
	this.B = obj2.physics; //{@p:B @d: The @l:physics instance of the second object.}
	this.objA = obj1; //{@p:objA @d: The parent object of @l:physics A. (direct copy of obj1 from constructor)}
	this.objB = obj2; //{@p:objB @d: The parent object of @l:physics B. (direct copy of obj2 from constructor)}
	this.penetration = 0; //{@p:penetration @d: The overlay of one object on another or 'depth of penetration' along the axis of LEAST penetration.}
	this.normal = []; //{@p:normal[] @d: The <b>unit</b> normal vector that points from obj1 (objA) to obj2 (objB). Note that this is an array, where [0] represents X and [1] represents Y. (Since this is a UNIT vector, the magnitude should always be 1.)}

	/*********************************{
	@f: generateBoxBoxManifold
	@d: This function carries out all computations to calculate the normal vector and penetration amount. (Specifically between two box objects. While currently boxes are the only thing the engine can handle, this may be expanded in the future, and each type of collision will require its own manifold generation function.) NOTE: Since there is no rotational logic anywhere in the engine, the normal calculated by this function should just be one of object A's face normals.
	}*********************************/
	this.generateBoxBoxManifold = function()
	{
		//Find the distances from the respective midpoints to left/right sides
		var aMidX = this.A.width / 2;
		var bMidX = this.B.width / 2;
		
		//Find the distances from the respective midpoints to top/bottom sides
		var aMidY = this.A.height / 2;
		var bMidY = this.B.height / 2;
		
		//get the vector from A's midpoint to B's midpoint
		var abVec = [];
		abVec[0] = (this.A.px + aMidX) - (this.B.px + bMidX);
		abVec[1] = (this.A.py + aMidY) - (this.B.py + bMidY);

		//calculate x-axis overlap: take the IDEAL DISTANCE between midpoints (if the perimeter of one was on the perimeter of the other) and subtract the ACTUAL DISTANCE between midpoints (found from the vector immediately above)
		var xIdealDistance = aMidX + bMidX; //ideal distance, if perimeter of one was exactly on perimeter of other, is just sum of distances from midpoints to sides.
		var xActualDistance = Math.abs(abVec[0]); //absolute value because we're dealing with a physical distance and want a positive result
		var xOverlap = xIdealDistance - xActualDistance;

		//calculate y-axis overlap: same as above
		var yIdealDistance = aMidY + bMidY;
		var yActualDistance = Math.abs(abVec[1]);
		var yOverlap = yIdealDistance - yActualDistance;

		//create the normal vector (this should just be one of the face normals of object A)
		if (xOverlap < yOverlap) //since going for the smallest overlap, go with xOverlap
		{
			if (abVec[0] < 0) //negative difference: B is on the RIGHT, point to the RIGHT
			{
				this.normal[0] = 1; //positive x direction (right)
				this.normal[1] = 0; 
			}
			else //positive difference: B is on the LEFT, point to the LEFT
			{
				this.normal[0] = -1 //negative x direction (left)
				this.normal[1] = 0;
			}
			this.penetration = xOverlap;
		}
		else //yOverlap is smaller, so use it
		{
			if (abVec[1] < 0) //negative difference: B is ABOVE, so point UP
			{
				this.normal[0] = 0;
				this.normal[1] = 1; //positive y direction (up)
			}
			else //positive difference: B is BELOW, so point DOWN
			{
				this.normal[0] = 0;
				this.normal[1] = -1; //negative y direction (down)
			}
			this.penetration = yOverlap;
		}
	}
}
