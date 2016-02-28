//*************************************************************
//  File: Physics.js
//  Date created: 6/4/2015
//  Date edited: 6/18/2015
//  Author: Nathan Martindale
//  Copyright © 2015 Digital Warrior Labs
//  Description: File for javascript class that relates to all 
//    physical properties of an object. EVERY OBJECT THAT GETS
//    EVALUATED BY THE PHSYICS ENGINE MUST CONTAIN AN INSTANCE
//    OF THIS CLASS
//*************************************************************

/*********************************{
@cl: Physics
@d: Representation/collection of all physical properties that an object can have. NOTE: EVERY OBJECT THAT GETS EVALUATED BY THE PHYSICS ENGINE MUST CONTAIN AN INSTANCE OF THIS CLASS. Also note an important feature of this physics engine: It is entirely layer based, which means if you want an object to collide with something else, they both have to exist on a common layer. (See the layers property below for more information.)
}*********************************/

/*********************************{
@c: Physics
}*********************************/
function Physics()
{
	this.id = 0; //{@p:id @d: An internal identification number that the engine uses to primarily when removing an entity from its list. THIS PROPERTY IS ASSIGNED AND USED INTERNALLY, there's no need or reason to manually edit its value.}
	this.layers = []; //{@p:layers[] @d: Array of layer numbers. Any object that has a 1 in this array will collide with any other object that also has a 1 in its layers array. NOTE: A single object can exist in mulitple layers. For example, if you need 'walls' that overlap, but don't want the physics engine attempting to resolve collisions between them every single frame, you can put each one on a separate layer, (they will no longer be considered to collide with each other) and then all objects can have all of those layer numbers in this array, so that they collide with all of the walls.}
	
	this.height = 0 //{@p:height @d: Height of bounding box. This can be thought of as a vector going down from py.}
	this.width = 0; //{@p:width @d: Width of bounding box. This can be thought of as a horizontal vector going to the right from px.}
	
	this.px = 0; //{@p:px @d: X-coordinate of the upper left hand corner of a bounding box.}
	this.py = 0; //{@p:py @d: Y-coordinate of the upper left hand corner of a bounding box.}
	
	this.vx = 0; //{@p:vx @d: Velocity in pixels per millisecond in the x direction.}
	this.vy = 0; //{@p:vy @d: Veolicty in pixels per millisecond in the y direction.}
	
	this.ax = 0; //{@p:ax @d: Acceleration in pixels per millisecond^2 in the x direction.}
	this.ay = 0; //{@p:ay @d: Acceleration in pixels per millisecond^2 in the y direction. NOTE: If attempting to model gravity, I've found that setting this property to somewhere between .0003-.0005 works quite well.}
	
	this.movable = true; //{@p:movable @d: Boolean representing if any movement forces will affect this object or not. If set to false, the updatePosition function will be ignored. Velocities can still be affected by collisions, but as long as this property is false, the position won't be updated. (px and py can still be manually changed if desired however.)}
	
	//"bounciness." between 0-1, closer to 1, bouncier it is
	this.restitution = .5; //{@p:restitution @d: DEFAULT: ".5". The "bouciness" of the object. (Can be thought of as the amount of dampening applied on every collision.) This should be a number between 0-1. The closer to 1, the bouncier it is. Set to 1 exactly means that the exact amount of force going INTO a collision is the exact amount of force coming out of it. (Anything higher and crazy things will ensue!) NOTE: In a collision between the two objects, final restitution of the collision is the AVERAGE of the respective resitutions of the two objects.}
	this.friction = .04; //{@p:friction @d: DEFAULT: ".04". The amount of friction an object has. To model realistic textures, a smaller friction would represent a smoother surface, (less friction, things slide on it easier) and a higher friction would represent a rougher texture (more fricton, less sliding.) NOTE: In a collision between two objects (or when one object is resting on another) final friction calculated is the AVERAGE of the respective frictions of the objects.}
	
	var mass = 1;
	var inv_mass = 1;
	
	this.setMass = function(newMass) //{@p:setMass(newMass) @d: DEFAULT MASS: "1". The mass of an object. This affects relative velocity changes (impulse ratio) between objects in a collision. (Higher mass object will be affected less than a lower mass object.) NOTE: If you need something with an infinite mass, (something that moves and participates in collisions but is never affected itself), pass in zero. If you're modelling a wall or something similar (something that SHOULDN'T MOVE) DON'T just give it an infinite mass, you must ALSO set movable to 'false'. (Velocity and acceleration can still affect objects with infinite mass.)}
	{ 
		inv_mass = 0;
		mass = newMass; 
		if (mass == 0) { inv_mass = 0; } //"infinite" mass"
		else { inv_mass = 1 / mass; }
	}
	this.getMass = function() { return mass; } //{@p:getMass() @d: Returns mass of the object.}
	this.getInvMass = function() { return inv_mass; } //{@p:getInvMass() @d: Returns the inverted mass of the object. This is calculated whenever setMass() is called. Thus, in any formulas that require the inverted mass, it is significantly faster to call this function than calculating inverted mass yourself in the actual equation. (Division is expensive resource-wise)}
	
	/*********************************{
	@f:updatePosition
	@d:Updates px and py based on the current velocities, and updates vx and vy based on the current acceleration. Note that the physics engine is time based rather than frame based. It controls positions based on length of time since the last update, not the fps. NOTE: You should not normally need to call this function manually, it should be handled by the @l:PhysicsEngine.
	@i:
		@v:dt @d: The change in time. (Time difference/amount of time in milliseconds since the last frame.)
	}*********************************/
	this.updatePosition = function(dt)
	{
		if (!this.movable) { return; }
		
		//update velocity based on acceleration
		this.vx = this.vx + this.ax*dt;
		this.vy = this.vy + this.ay*dt;

		//update position based on velocity
		this.px = this.px + this.vx*dt;
		this.py = this.py + this.vy*dt;
	}
}
