//*************************************************************
//  File: PhysicsEngine.js
//  Date created: 6/4/2015
//  Date edited: 6/21/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: Javascript class for the engine itself, which takes
//    care of handling the collisions and movements of all objects
//    that have physcial properties
//*************************************************************

/*********************************{
@cl: PhysicsEngine
@d: The engine itself, this class contains all the engine constants and objects, and it takes care of handling all calculations, detecting and resolving collisions, and updating the positions of the objects. This class will also check for global functions as a pseudo "event system." Look in the functions section for more information.
}*********************************/

/*********************************{
@c: PhysicsEngine
}*********************************/
function PhysicsEngine()
{
	//physics engine constants
	var CORRECTION_PERCENT = .3; //{@cnst:CORRECTION_PERCENT @d: During positional correction, this constant will be multiplied to the difference between penetration and allowed correctional slop, and that difference will be applied to the positions of the colliding objects to separate them. (Recommended value is .3)}
	var CORRECTION_SLOP = .001; //{@cnst:CORRECTION_SLOP @d: The distance or "slop" that will be allowed in object overlap before positional correction is applied. This is important so that objects don't rapidly "vibrate" when multiple objects with gravity applied stack on each other. (this can also be affected by CORRECTION_PERCENT.) Recommended value is .001.}
	var MAX_PENETRATION = 10; //{@cnst:MAX_PENETRATION @d: The "opposite" limit of CORRECTION_SLOP. If an object penetrates farther than this constant allows, during the positional correction process, objects will be moved absolutely outside of this range, regardless of the CORRECTION_PERCENT. (Note, this will cause glitching if there are a lot of cluttered objects with gravity or some other force applied.) Recommended value is 10. (This is more dependent on relative object size however.)}
	var VELOCITY_CLAMP = .00001; //{@cnst:VELOCITY_CLAMP @d: If at any point during movement, if an objects velocity is smaller than this constant, it's truncated to zero to reduce on calculations required. Recommended value is .00001.}
	var MAXIMUM_TIME_TIC = 25; //{@cnst:MAXIMUM_TIME_TIC @d: The frame dt limit, or maximum change in time allowed (in milliseconds between frames.) If the passed in dt (for the updatePhysics() function) is ever greater than this, the engine clamps to this value, meaning that the simulation will run slower, but it will retain accuracy. (Otherwise, drastic jumping/glitching would occur.) The recommended value for this is 25 (milliseconds) which is the equivalent of 40 fps. Any time the frame rate dips below this, the engine will slow down to match.}

	this.ticClamped = false; //{@p:ticClamped @d: Boolean value representing whether time clamping is currently active or not. (See MAXIMUM_TIME_TIC constant.) NOTE: THIS PROPERTY IS FOR INFORMATIONAL ACCESS ONLY. There is no need or reason to manually edit its value. Its intended purpose is for whatever interface that uses this engine to check this property and if desired, display information/warnings related to the frame rate. (Also note, in some later version of this engine, this may be converted into the pseudo event system.)

	var entities = []; 
	var lastEntityID = 0;
	
	var collisionSets = []; //collection of CollisionManifolds

	
	this.getEntities = function() { return entities; } //{@p:getEntities() @d: Returns the array of entities evaluated by the engine.}
	
	/*********************************{
	@f:addEntity
	@d:Adds the specified object to the entities list, where it will be analyzed and updated by the engine in the updatePhysics() function.
	@i:
		@v:object @d: The object to be stored in the entities array. NOTE: This object must contain a physics object
	}*********************************/
	this.addEntity = function(object) 
	{ 
		lastEntityID++;
		object.physics.id = lastEntityID;
		entities.push(object); 
	}
	
	/*********************************{
	@f:removeEntity
	@d:Searches the entities array based on the physics id system and removes it when found. NOTE: Keep in mind that if monitoring this list while removing from it, all indicies of objects after the one passed in will CHANGE. This function returns the removed object's old index so you can update any variables as necessary.
	@i:
		@v:object @d: The object to be removed from the entities array. 
	@o: Returns the previous array index of the removed object, or -1 if not found.
	}*********************************/
	this.removeEntity = function(object) //returns prior index of the removed object
	{
		for (var i = 0; i < entities.length; i++)
		{
			if (entities[i].physics.id == object.physics.id) 
			{ 
				entities.splice(i, 1); 
				return i;
			}
		}
		return -1;
	}

	/*********************************{
	@f:updatePhysics
	@d:Analyzes every object in the entities array, checking for collisions, resolving collisions, and updating positions and velocities of all entities. This function should be called once every animation frame.
	@i:
		@v:dt @d: The amount of time in milliseconds since the last updatePhysics() call. (This engine is TIME based, rather than FRAME based.)
	}*********************************/
	this.updatePhysics = function(dt)
	{
		//frame tic time clamping
		if (dt > MAXIMUM_TIME_TIC) { dt = 25; this.ticClamped = true; }
		else { this.ticClamped = false; }
		
		//check for collisions
		var collisionIndex = 0;
		for (var i = 0; i < entities.length; i++)
		{
			for (var j = i + 1; j < entities.length; j++)
			{
				var obj1 = entities[i];
				var obj2 = entities[j];

				var collision = this.detectCollision(obj1, obj2);
				if (collision) { collisionSets.push(new CollisionManifold(obj1, obj2)); }
			}
		}

		//resolve any collisions
		for (var i = 0; i < collisionSets.length; i++)
		{
			resolveCollision(collisionSets[i]);
		}

		//clear out collisionSets
		collisionSets = [];

		//update positions
		for (var i = 0; i < entities.length; i++)
		{
			//CLAMP VELOCITIES
			var velX = Math.abs(entities[i].physics.vx);
			var velY = Math.abs(entities[i].physics.vy);
			if (velX < VELOCITY_CLAMP && velX > 0) { entities[i].physics.vx = 0;  }
			if (velY < VELOCITY_CLAMP && velY > 0) { entities[i].physics.vy = 0; }

			//update
			entities[i].physics.updatePosition(dt);
		}
	}

	/*********************************{
	@f:detectCollison
	@d:Checks whether the two objects have a common layer and if they are colliding. NOTE: The two objects MUST have a @l:Physics instance named 'physics'.
	@i:
		@v:objA @d: The first object involved in the collision. (Must have Physics instance.)
		@v:objB @d: The second object to check against the first object for a collison. (Must have Physics instance.)
	@o: Returns true if they overlap and have a common layer, and false if they don't collide.
	}*********************************/
	this.detectCollision = function(objA, objB)
	{
		var obj1 = objA.physics; //TODO: ERROR CHECKING
		var obj2 = objB.physics;
		
		if ((obj1.px + obj1.width) < obj2.px || obj1.px > (obj2.px + obj2.width)) { return false; }
		if ((obj1.py + obj1.height) < obj2.py || obj1.py > (obj2.py + obj2.height)) { return false; }

		//check to make sure in same layers
		for (var i = 0; i < obj1.layers.length; i++)
		{
			if (obj2.layers.indexOf(obj1.layers[i]) > -1) { return true; }
		}

		//return true;
		//reaches this point if it didn't find any matching layers
		return false;
	}

	//Apply impulse velocities and positional correction to both objects in a collision
	function resolveCollision(manifold)
	{
		manifold.generateBoxBoxManifold();
					
		//CHECK if engine container has the event function and if so call it.
		if (typeof physEvent_collision == "function")
		{
			var vel1 = 0;
			var vel2 = 0;
			var totalCollisionVelocity = 0;
			if (manifold.normal[0] == 0) //collision normal is along y axis
			{ 
				vel1 = Math.abs(manifold.A.vy); 
				vel2 = Math.abs(manifold.B.vy);
				
				//if velocities have opposite signs, going against each other, so add velocities
				if ((manifold.A.vy < 0 && manifold.B.vy > 0) || (manifold.A.vy > 0 && manifold.B.vy < 0)) { totalCollisionVelocity = vel1 + vel2; }
				//otherwise use the difference
				else { totalCollisionVelocity = Math.abs(vel1 - vel2); }
			}
			else //collision normal is along x axis
			{
				vel1 = Math.abs(manifold.A.vx);
				vel2 = Math.abs(manifold.B.vx);
				
				//if velocities have opposite signs, going against each other, so add velocities
				if ((manifold.A.vx < 0 && manifold.B.vx > 0) || (manifold.A.vx > 0 && manifold.B.vx < 0)) { totalCollisionVelocity = vel1 + vel2; }
				//otherwise use the difference
				else { totalCollisionVelocity = Math.abs(vel1 - vel2); }
			}
			
			var keOn1 = (manifold.B.getMass() * totalCollisionVelocity * totalCollisionVelocity) / 2;
			var keOn2 = (manifold.A.getMass() * totalCollisionVelocity * totalCollisionVelocity) / 2;

			//call the event function
			physEvent_collision(manifold.objA, manifold.objB, keOn1, keOn2);
		}
		
		//get the velocity vector of the objects in relation to eachother
		var relativeVelocity = [];
		relativeVelocity[0] = manifold.A.vx - manifold.B.vx;
		relativeVelocity[1] = manifold.A.vy - manifold.B.vy;
		
		//how fast the objects are moving towards eachother in relation to their normal
		//(this should end up giving you velocity ONLY IN ONE DIRECTION (along x, or along y)
		//NOTE: normalvelocity can be thought of to represent the CHANGE IN VELOCITY. Delta V
		var normalVelocity = dotProduct(relativeVelocity, manifold.normal);

		//if objects already moving away from eachother, ignore
		if (normalVelocity < 0) { return; }

		//get average restitution
		var e = (manifold.A.restitution + manifold.B.restitution) / 2;

		//calculate impulse scalar (impulse = Mass * Delta V)
		var j = (-(1 + e) * normalVelocity) / (manifold.A.getInvMass() + manifold.B.getInvMass());

		//apply that impulse based on normal unit vector
		var impulse = [];
		impulse[0] = j * manifold.normal[0];
		impulse[1] = j * manifold.normal[1];

		//change velocities based on normal
		manifold.A.vx += manifold.A.getInvMass() * impulse[0];
		manifold.A.vy += manifold.A.getInvMass() * impulse[1];
		manifold.B.vx -= manifold.B.getInvMass() * impulse[0];
		manifold.B.vy -= manifold.B.getInvMass() * impulse[1];

		//positional correction
		if (manifold.penetration > CORRECTION_SLOP) 
		{
			var correction = [];
			correction[0] = CORRECTION_PERCENT * manifold.normal[0] * (manifold.penetration - CORRECTION_SLOP); 
			correction[1] = CORRECTION_PERCENT * manifold.normal[1]  * (manifold.penetration - CORRECTION_SLOP);
			
			if (manifold.penetration > MAX_PENETRATION)
			{
				correction[0] = manifold.normal[0] * (manifold.penetration - MAX_PENETRATION + 1);
				correction[1] = manifold.normal[1] * (manifold.penetration - MAX_PENETRATION + 1); 
			}

			if (manifold.A.movable) 
			{
				manifold.A.px -= correction[0];
				manifold.A.py -= correction[1];
			}
			if (manifold.B.movable) 
			{
				manifold.B.px += correction[0];
				manifold.B.py += correction[1];
			}
		}

		//FRICTION STUFFS
		
		//recalculate relative velocity
		relativeVelocity = [];
		relativeVelocity[0] = manifold.A.vx - manifold.B.vx;
		relativeVelocity[1] = manifold.A.vy - manifold.B.vy;

		//get the tangent vector
		var tangent = [];
		tangent[0] = relativeVelocity[0] - dotProduct(relativeVelocity, manifold.normal) * manifold.normal[0];
		tangent[1] = relativeVelocity[1] - dotProduct(relativeVelocity, manifold.normal) * manifold.normal[1];
		
		//get magnitude to apply along friction vector
		var jt = -dotProduct(relativeVelocity, manifold.normal);
		jt = jt / (manifold.A.getInvMass() + manifold.B.getInvMass());
		
		var frictionImpulse = [];
		dynFric = (manifold.A.friction + manifold.B.friction) / 2;
		frictionImpulse[0] = (-jt * dynFric * 10) * tangent[0] + (-dynFric) * tangent[0];
		frictionImpulse[1] = (-jt * dynFric * 10) * tangent[1] + (-dynFric) * tangent[1];
		
		//apply friction impulse
		manifold.A.vx += manifold.A.getInvMass() * frictionImpulse[0];
		manifold.A.vy += manifold.A.getInvMass() * frictionImpulse[1];
		manifold.B.vx -= manifold.B.getInvMass() * frictionImpulse[0];
		manifold.B.vy -= manifold.B.getInvMass() * frictionImpulse[1];
	}

	//only handles 2x1 matricies
	function dotProduct(matrix1, matrix2)
	{
		var result = (matrix1[0] * matrix2[0]) + (matrix1[1] * matrix2[1]);
		return result;
	}

	/*********************************{
	@f:physEvent_collision
	@d:EVENT - THIS FUNCTION IS NOT IMPLEMENTED. As a basic event system, the physics engine will check to see if a global function physEvent_Collision() exists, and if so it is called whenever a collision occurs. If you want to run a certain piece of code on every collision, you can implement this function yourself on a global scope.
	@i:
		@v:objA @d: The first object involved in the collision. 
		@v:objB @d: The second object involved in the collision. 
		@v:keOn1 @d: Amount of kinetic energy transferred to objA. This can be used to implement a damage system.
		@v:keOn2 @d: Amount of kinetic energy transferred to objB.
	}*********************************/
}
