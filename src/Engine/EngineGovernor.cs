//*************************************************************
//  File: EngineGovernor.cs
//  Date created: 1/15/2015
//  Date edited: 2/28/2016
//  Author: Nathan Martindale
//  Copyright © 2016 Digital Warrior Labs
//  Description: Class that controls the engine, and takes care of all objects, kind of a master class
//*************************************************************

//NOTE that this class follows the observable pattern. It has a list of ILogger interfaces.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
	public class EngineGovernor
	{
		//static variables
		public static int VERSION_MAJOR = 1;
		public static int VERSION_MINOR = 0;
		public static int VERSION_BUILD = 166;
		public static int VERSION_REVISION = 2;
		public static string BUILD_DATE = "5/27/2015";


		//member variables
		private static List<ILogger> m_cRegisteredLogObservers = new List<ILogger>();

		private CodeDocument m_cWorkingDocument = null; // TODO: make this a list!
		// TODO: take in sections, output javascript code and lists of all sections/classes associated with those sections, and then javascript can auto populate the side bar on the left of the page


		private List<CodeDocument> m_cWorkingDocuments = new List<CodeDocument>();

		//static variables
		public static bool ENABLE_LOGGING = true; //if speed is a concern, set to false (will ignore all logging requests, which will save on time if there are a lot of observers)

		//construction
		public EngineGovernor(List<ILogger> logObservers)
		{
			m_cRegisteredLogObservers = logObservers;
			log("********** Engine governor " + VERSION() + " initialized. Revving up documentation engine! **********");
			//print engine stats here (version, copyright, etc)
		}

		//observer functions
		public void registerLogObserver(ILogger observer) { m_cRegisteredLogObservers.Add(observer); }
		public void removeLogObserver(ILogger observer) { m_cRegisteredLogObservers.Remove(observer); }

		//parses the comment syntax in the given file
		public void runAnalysis(string filename)
		{
			log("File analysis requested, starting parser...");
			CodeParser parser = new CodeParser();
			//CodeDocument doc = parser.parseFile(filename);
			CodeDocument doc = (parser.parseFile(filename))[0];
			m_cWorkingDocument = doc;
			log("Engine governor has received parser's document and stored it as the current working document.");
		}

		//creates the html API documentation (assumes m_cWorkingDocument has already been assigned)
		//Returns 3 strings, filename, and class(or interface name), and description
		public List<string> generateAPIDoc(string location, string topHeaderText)
		{
			if (m_cWorkingDocument == null) { log("ERROR - Working Document has not been set. You must analyze a code file before generating API documentation for it.", -1); return null; }
			log("Preparing to create API documentation off of current working document...");
			DocGenerator generator = new DocGenerator(m_cWorkingDocument);
			generator.TopHeaderText = topHeaderText;
			List<string> returnedInfo = generator.createHTMLDocument(location);
			log("API Documentation process complete!");
			return returnedInfo;
		}

		//get the version information
		public static string VERSION()
		{
			return VERSION_MAJOR + "." + VERSION_MINOR + "." + VERSION_BUILD + "." + VERSION_REVISION;
		}

		//global logging function
		public static void log(string msg)
		{
			if (ENABLE_LOGGING) { foreach (ILogger observer in m_cRegisteredLogObservers) { observer.update(msg, 0); } } //0 = everyone sees it
		}
		public static void log(string msg, int level) //use -1 for error/warning messages
		{
			if (ENABLE_LOGGING) { foreach (ILogger observer in m_cRegisteredLogObservers) { observer.update(msg, level); } } //use 1 for debug messages, only ILoggers with a level set at 1 will record these
		}  //use 2 for unit tests (printing out lists)
	}
}
