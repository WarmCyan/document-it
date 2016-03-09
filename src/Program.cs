﻿//*************************************************************
//  File: Program.cs
//  Date created: 1/23/2015
//  Date edited: 2/28/2016
//  Author: Nathan Martindale
//  Copyright © 2016 Digital Warrior Labs
//  Description: Document It Console Interface. Driver for documentation engine
//*************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Engine;

namespace DICI
{
	class Program
	{
		//static variables
		public static int VERSION_MAJOR = 2;
		public static int VERSION_MINOR = 0;
		public static int VERSION_BUILD = 38161;
		public static int VERSION_REVISION = 0;

		public static string g_sProjectTitle = ""; //I KNOW IT'S BAD TO MAKE THIS A GLOBAL, BUT I DON'T WANT TO HAVE MESSY RETURNS
		public static string g_sProjectFolder = ""; //BAD BAD BAD, BAD ME.

		//users can either pass in args 1. the filename (code) 2.the FOLDER PATH (result), and 3. (optionally) the header text. OR run the executable and manually put it all in
		static void Main(string[] args)
		{
			List<List<string>> indexInformation = null;

			// set up the engine
			List<ILogger> logSystem = new List<ILogger>();
			logSystem.Add(new ConsoleLogger(0));
			//logSystem.Add(new TextFileLogger(0));
			//logSystem.Add(new TextFileLogger("debug.txt", 1));
			//logSystem.Add(new TextFileLogger("errors.txt", -1, true));

			EngineGovernor engine = new EngineGovernor(logSystem);
			
			//running from executuable, instead of command line
			if (args.Length == 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine("\n===================== DICI " + VERSION() + " =====================");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("\nCopyright © 2015 Digital Warrior Labs");
				Console.Write("Engine version: ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(EngineGovernor.VERSION());
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("\nBuild Date: " + EngineGovernor.BUILD_DATE);
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine("\n\nWelcome to the Document It Console Interface!");

				string l_file = "";
				string l_resultFolder = "";
				string l_headerText = "";
				bool exitFlag = false;

				while (!exitFlag)
				{
					//get the file name
					Console.Write("\n\n\nEnter the filename of the code file you wish to document, or provide a .apiproj file. (This can be an absolute or relative file path.)\nAlternately, enter 'exit' to quit the program.\nfile: ");
					l_file = Console.ReadLine();

					//check for exit
					if (l_file == "exit") { exitFlag = true; continue; }

					//check to see if the file is a .apiproj
					if (l_file.EndsWith(".apiproj")) 
					{ 
						indexInformation = runProject(l_file, null); // TODO: TAKE OUT ENTIRE SINGLE FILE THING AT SOME POINT
						/*if (indexInformation != null) { createIndex(indexInformation); }*/
						continue; 
					}

					//get destination folder
					Console.Write("\n\nEnter the folder path where you would like the HTML document to be stored. (This can be an absolute or relative file path. If left blank, the document will be saved in the same folder as this executable.)\nDestination folder: ");
					l_resultFolder = Console.ReadLine();

					//header text
					Console.Write("\n\nEnter the header text you would like to appear in the upper left-hand corner of the document. (This can be used for project names, company names, etc.) If you don't care what it says, just hit enter, and the default program name will be substituted.\nHeader text: ");
					l_headerText = Console.ReadLine();

					//run(l_file, l_resultFolder, l_headerText);
				}

				//Console.WriteLine("Press any key to continue...");
				//Console.Read();
			}
			else
			{
				//COMMAND LINE HANDLING HERE
				string projectFile = args[0];
				if (!projectFile.EndsWith(".apiproj")) { Console.WriteLine("If running this program with command line arguments, you MUST supply an .apiproj file as the first argument."); }

				indexInformation = runProject(projectFile, engine);
				//createIndex(indexInformation);
			}
			
		}

		//function that takes care of reading the special .apiproj files 
		//returns list of all information from files created
		static List<List<string>> runProject(string file, EngineGovernor engine)
		{
			List<List<string>> compiledInfo = new List<List<string>>();

			int currentSectionID = -1;
			List<string> sections = new List<string>();
			//Dictionary<int, string> fileList = new Dictionary<int, string>();  // int is index of section
			List<string> fileList = new List<string>();
			List<int> fileListAssoc = new List<int>();
			
			string headerText = "";
			string destFolder = "";

			Console.WriteLine("Recieved api project file.");
			Console.WriteLine("Attempting to read file....");

			//try to open the file
			StreamReader fileStream = null;
			try { fileStream = new StreamReader(file); }
			catch (IOException e) 
			{ 
				Console.ForegroundColor = ConsoleColor.Red; 
				Console.WriteLine("ERROR - Failed to open file. Incorrect path?"); 
				Console.ForegroundColor = ConsoleColor.Gray;  
				return null; 
			}
			Console.WriteLine("Got file!");

			//read lines in file
			List<string> fileLines = new List<string>();
			string currentLine = "";
			int lineIndex = 0;
			while((currentLine = fileStream.ReadLine()) != null)
			{
				fileLines.Add(currentLine);
				lineIndex++;
			}
			Console.WriteLine("Read in " + lineIndex + " lines");

			//iterate through all lines and analyze them appropriately
			Console.WriteLine("Analyzing project file...");
			for (int i = 0; i < fileLines.Count; i++)
			{
				//if line starts with # sign or if it's blank, ignore it!
				if (fileLines[i].StartsWith("#") || fileLines[i] == "" || fileLines[i] == " ") { continue; }

				// check for section statement
				if (fileLines[i].StartsWith(":section"))
				{
					string sectionName = fileLines[i].Substring(fileLines[i].IndexOf(" ")); // TODO: error checking!
					sections.Add(sectionName);
					currentSectionID = sections.Count - 1;
					continue;
				}

				//first check for properties
				if (fileLines[i].StartsWith("destination=") && fileLines[i].Length > 12)
				{
					destFolder = fileLines[i].Substring(12);
				}
				else if (fileLines[i].StartsWith("header=") && fileLines[i].Length > 7)
				{
					headerText = fileLines[i].Substring(7);
				}
				//if line isn't a property, must be file name
				else
				{
					// first check to see if no sections involved
					if (currentSectionID == -1) { sections.Add("All"); currentSectionID = 0; }
					fileList.Add(fileLines[i]);
					fileListAssoc.Add(currentSectionID);
				}
			}
			Console.WriteLine("Analyzed successfully!");

			g_sProjectTitle = headerText;
			g_sProjectFolder = destFolder;

			engine.Sections = sections;

			//now go through the file list and generate documentation for all of them
			Console.WriteLine("Running documentation engine on file list...");
			for (int i = 0; i < fileList.Count; i++)
			{
				engine.runAnalysis(fileList[i], fileListAssoc[i]);
			}
			engine.generateAPIDoc(destFolder, headerText);
			
			
			Console.WriteLine("Finished running engine!");

			return compiledInfo;
			

			//CREATE INDEX FILE (maybe a file name thing should be returned by the generateDoc thing 
			//DOC GENERATOR SHOULD RETURN LIST OF STRINGS. FIRST ONE IS NAME OF CLASS, SECOND IS DESCRIPTION
		}

		//get the version information
		public static string VERSION()
		{
			return VERSION_MAJOR + "." + VERSION_MINOR + "." + VERSION_REVISION;
		}
	}
}
