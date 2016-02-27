//********************************************************
// File: ConsoleLogger.cs
// Author: Nathan Martindale
// Date Created: 1/15/2015
// Date Edited: 5/19/2015
// Copyright © 2015 Digital Warrior Labs
// Description: A console implementation of the ILogger interface (shows all log updates in the console window
//********************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
	public class ConsoleLogger : ILogger
	{
		private int m_iLevelAllowed = 0; //only gets default messages //1 means gets a higher level (more) messages)

		public ConsoleLogger(int level) { m_iLevelAllowed = level; }

		public void update(string msg, int level)
		{
			if (level <= m_iLevelAllowed)
			{
				if (msg.Contains("ERROR")) { Console.ForegroundColor = ConsoleColor.Red; }
				else if (msg.Contains("WARNING")) { Console.ForegroundColor = ConsoleColor.Yellow; }
				Console.WriteLine(msg);
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}
	}
}
