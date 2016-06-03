//*****************************************************************
//  File: BuildTrainingFacilityTask.java
//  Author: Nathan Martindale
//  Date Created: 4/10/2014
//  Last Edited: 5/24/2014
//  Copyright © 2014 CGAP
//  Description: Task to take care of creating a training facility in the desired location
//*****************************************************************

import java.io.*;
import java.util.*;


public class BuildTrainingFacilityTask extends Task
{
	//member variables
	private int m_iNationID;
	private int m_iSectorID;
	
	public static final int MY_CONSTANT = 5; //{@cnst:int MY_CONSTANT @d:A random constant that is always 5}

	//{@c:BuildTrainingFacilityTask @i:@v:String type @d:Task type@v:int timeDif@d:Time until task code executes@v:int nationID@d:ID of the nation that owns the facility@v:int sectorID@d:ID of the sector where it's being built}
	
	//construction
	public BuildTrainingFacilityTask(String type, int timeDif, int nationID, int sectorID) 
	{
		super(type, timeDif);
		m_iNationID = nationID;
		m_iSectorID = sectorID;
	}
	
	public BuildTrainingFacilityTask(){} //{@c:BuildTrainingFacilityTask @d: Blank constructor}

	//properties
	public int getSectorID() { return m_iSectorID; } //{@p:int getSectorID}
	public int getNationID() { return m_iNationID; } //{@p:int getNationID @d:Nation that owns the facility being built.}

	//methods
	
	/*{
		@f:void runTaskCode
		@d:Overriden from @l:Task, contains the code that will execute after the specified amount of time from the constructor
	}*/
	public void runTaskCode()
	{
		int id = Server.game.getNextTrainingFacilityID();
		TrainingFacility tFac = new TrainingFacility(id, m_iNationID, m_iSectorID);
		Server.game.addTrainingFacility(tFac);
		Sector.getSector(m_iSectorID).setTFID(id);
		System.out.println("NEW TFID: " + Sector.getSector(m_iSectorID).getTFID());
		Nation.getNation(m_iNationID).send(new Message("Server", "Display", ">> Construction has been completed on training facility in Sector " + Sector.getSector(m_iSectorID).getLabel() + "\n", WON.STANDARD_TECH));
		completed = true;
	}
	
	/* AWESOME SYNTAX: { @f:static @l:BuildTrainingFacilityTask getTaskBySectorID
	@d:Global function that will return the DB instance of itself based on the passed sector ID and nation ID (if it exists)
	@input:
		@variable:int id @description:Sector ID
		@v:int nationID @d:ID of the nation that owns the desired training facility
	@output:The @l:BuildTrainingFacilityTask if found in the database based on the passed conditions, otherwise returns null } and stuff here*/
	
	public static BuildTrainingFacilityTask getTaskBySectorID(int id, int nationID)
	{
		for (Task task : Server.game.queue.getTasks())
		{
			if (task instanceof BuildTrainingFacilityTask)
			{
				BuildTrainingFacilityTask t = (BuildTrainingFacilityTask) task;
				if (t.getSectorID() == id && t.getNationID() == nationID) { return t; }
			}
		}		
		return null;
	}
}