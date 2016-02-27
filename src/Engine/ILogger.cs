//********************************************************
// File: ILogger.cs
// Author: Nathan Martindale
// Date Created: 1/15/2015
// Date Edited: 1/15/2015
// Copyright © 2015 Digital Warrior Labs
// Description: interface that follows the Observer pattern (will recieve logged updates from the EngineGovernor. This is useful if implementing a graphical UI and you want to display log entries)
//   You can use level to define whether a logger should see a certain message or not (the higher numbers that are allowed the more information will be shown)
//********************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
	public interface ILogger
	{
		void update(string msg, int level); //higher level means a more detailed message, only used for when you want to see LOTS of data
	}
}
