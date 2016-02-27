//********************************************************
// File: TextFileLogger.cs
// Author: Nathan Martindale
// Date Created: 1/15/2015
// Date Edited: 5/25/2015
// Copyright © 2015 Digital Warrior Labs
// Description: A text file implementation of the ILogger interface (saves all log entries in a text file)
//********************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
	public class TextFileLogger : ILogger
	{
		//member variables
		private string m_sLogFileName = "log.txt"; //default location
		private int m_iLevelAllowed = 0; //default level (only basic messages get through) make 1 to see higher level of detail
		private bool m_bListenOnlyToGivenLevel = false; //means that it will ignore any messages that don't have the level = m_iLevelAllowed

		//construction
		public TextFileLogger(int level) 
		{ 
			m_sLogFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "/log.txt";
			m_iLevelAllowed = level; 
		}

		//changed specifically so that the debug file doesn't get enormous
		public TextFileLogger(string file, int level) 
		{
			m_sLogFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "/" + file; 
			m_iLevelAllowed = level;
			StreamWriter fileStream = new StreamWriter(Uri.UnescapeDataString(new Uri(m_sLogFileName).AbsolutePath), false);
			fileStream.WriteLine("");
			fileStream.Close();
		} 
		public TextFileLogger(string file, int level, bool onlyGivenLevel) 
		{
			m_sLogFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "/" + file; 
			m_iLevelAllowed = level; 
			m_bListenOnlyToGivenLevel = onlyGivenLevel; 
		}

		//properties
		public string LogFile { get { return m_sLogFileName; } set { m_sLogFileName = value; } }

		public void update(string msg, int level)
		{
			if (m_bListenOnlyToGivenLevel)
			{
				if (level == m_iLevelAllowed) { printMsg(msg); }
				else { return; }
			}
			else
			{
				if (level <= m_iLevelAllowed)
				{
					printMsg(msg);
				}
			}
		}

		private void printMsg(string msg)
		{
			StreamWriter logFileStream = new StreamWriter(Uri.UnescapeDataString(new Uri(m_sLogFileName).AbsolutePath), true);
			logFileStream.WriteLine(DateTime.Now.ToString() + " - " + msg);
			logFileStream.Close();
		}
	}
}
