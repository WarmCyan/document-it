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
		private List<int> m_cWorkingDocumentsAssoc = new List<int>();

		private List<string> m_cSections = new List<string>();

		// in each of the dictionaries, the int represents index of section in m_cSections above that its associated with (for output javascript file)
		/*private Dictionary<int, string> m_cClasses = new Dictionary<int, string>();
		private Dictionary<int, string> m_cInterfaces = new Dictionary<int, string>();
		private Dictionary<int, string> m_cFiles = new Dictionary<int, string>();*/

		private List<string> m_cClasses = new List<string>();
		private List<string> m_cInterfaces = new List<string>();
		private List<string> m_cFiles = new List<string>();
		
		private List<int> m_cClassesAssoc = new List<int>();
		private List<int> m_cInterfacesAssoc = new List<int>();
		private List<int> m_cFilesAssoc = new List<int>();

		//static variables
		public static bool ENABLE_LOGGING = true; //if speed is a concern, set to false (will ignore all logging requests, which will save on time if there are a lot of observers)

		//construction
		public EngineGovernor(List<ILogger> logObservers)
		{
			m_cRegisteredLogObservers = logObservers;
			log("********** Engine governor " + VERSION() + " initialized. Revving up documentation engine! **********");
			//print engine stats here (version, copyright, etc)
		}

		// properties
		public List<string> Sections { get { return m_cSections; } set { m_cSections = value; } }

		//observer functions
		public void registerLogObserver(ILogger observer) { m_cRegisteredLogObservers.Add(observer); }
		public void removeLogObserver(ILogger observer) { m_cRegisteredLogObservers.Remove(observer); }

		//parses the comment syntax in the given file (pass in the section index it's associated with)
		public void runAnalysis(string filename, int section)
		{
			log("File analysis requested, starting parser...");
			CodeParser parser = new CodeParser();
			//CodeDocument doc = parser.parseFile(filename);
			List<CodeDocument> docList = parser.parseFile(filename);
			//m_cWorkingDocument = doc;
			//m_cWorkingDocuments = docList;
			foreach (CodeDocument doc in docList) 
			{ 
				m_cWorkingDocuments.Add(doc); 
				m_cWorkingDocumentsAssoc.Add(section);
				if (doc.CodeObjects[0].CodeType == "class") 
				{ 
					//m_cClasses.Add(section, doc.CodeObjects[0].Name); 
					m_cClasses.Add(doc.CodeObjects[0].Name); 
					m_cClassesAssoc.Add(section);
				} 
				else if (doc.CodeObjects[0].CodeType == "interface") 
				{ 
					//m_cInterfaces.Add(section, doc.CodeObjects[0].Name); 
					m_cInterfaces.Add(doc.CodeObjects[0].Name); 
					m_cInterfacesAssoc.Add(section);
				}
				else 
				{ 
					//m_cFiles.Add(section, doc.CodeObjects[0].Name); 
					m_cFiles.Add(doc.CodeObjects[0].Name); 
					m_cFilesAssoc.Add(section);
				} 
			}
			log("Engine governor has received parser's documents and stored in list of current working documents.");
		}

		//creates the html API documentation (assumes m_cWorkingDocument has already been assigned)
		//Returns 3 strings, filename, and class(or interface name), and description
		// NEW: generates ALL api documentation of ALL code that has been analyzed up until this point!
		public List<string> generateAPIDoc(string location, string topHeaderText)
		{
			/*if (m_cWorkingDocument == null) { log("ERROR - Working Document has not been set. You must analyze a code file before generating API documentation for it.", -1); return null; }
			log("Preparing to create API documentation off of current working document...");
			DocGenerator generator = new DocGenerator(m_cWorkingDocument);
			generator.TopHeaderText = topHeaderText;
			List<string> returnedInfo = generator.createHTMLDocument(location);
			log("API Documentation process complete!");
			return returnedInfo;*/

			/*foreach (CodeDocument doc in m_cWorkingDocuments)
			{
				log("Preparing to create API documentation off of current working documents...");
				DocGenerator generator = new DocGenerator(doc);
				generator.TopHeaderText = topHeaderText;
				generator.createHTMLDocument(location);
			}*/
			for (int i = 0; i < m_cWorkingDocuments.Count; i++)
			{
				DocGenerator generator = new DocGenerator(m_cWorkingDocuments[i], m_cWorkingDocumentsAssoc[i]);
				generator.TopHeaderText = topHeaderText;
				generator.createHTMLDocument(location);
			}
			
			// TODO TODO: add in section/list saving function in here
			// TODO TODO TODO TODO: (in other words, each html document needs to know which section to start in (use a global variable in a script put in the html itself?)
			
			// write sections
			/*string sections = "";
			foreach (string sec in m_cSections) { sections += sec + "\n"; }*/

			string sectionsJS = "var SECTION_LIST = [";
			for (int i = 0; i < m_cSections.Count; i++)
			{
				sectionsJS += "'" + m_cSections[i] + "'";
				if (i < m_cSections.Count - 1) { sectionsJS += ","; }
			}
			sectionsJS += "];";
			writeToFile(location, "sections.js", sectionsJS);
			
			// write classes
			/*string classes = "";
			//foreach (KeyValuePair<int, string> classEntry in m_cClasses) { classes += classEntry.Value + "," + classEntry.Key + "\n"; }
			for (int i = 0; i < m_cClasses.Count; i++) { classes += m_cClasses[i] + "," + m_cClassesAssoc[i] + "\n"; }*/
			
			string classesJS = "var CLASS_LIST = [";
			for (int i = 0; i < m_cClasses.Count; i++) 
			{ 
				string link = DocGenerator.makeSafeLink(m_cClasses[i]) + ".html";
				classesJS += "'" + m_cClasses[i] + "," + m_cClassesAssoc[i] + "," + link + "'";
				if (i < m_cClasses.Count - 1) { classesJS += ","; }
			}
			classesJS += "];";
			writeToFile(location, "classes.js", classesJS);

			// write interfaces
			/*string interfaces = "";
			//foreach (KeyValuePair<int, string> interfaceEntry in m_cInterfaces) { interfaces += interfaceEntry.Value + "," + interfaceEntry.Key + "\n"; }
			for (int i = 0; i < m_cInterfaces.Count; i++) { interfaces += m_cInterfaces[i] + "," + m_cInterfacesAssoc[i] + "\n"; }*/
			
			string interfacesJS = "var INTERFACE_LIST = [";
			for (int i = 0; i < m_cInterfaces.Count; i++)
			{
				string link = DocGenerator.makeSafeLink(m_cInterfaces[i]) + ".html";
				interfacesJS += "'" + m_cInterfaces[i] + "," + m_cInterfacesAssoc[i] + "," + link + "'";
				if (i < m_cInterfaces.Count - 1) { interfacesJS += ","; }
			}
			interfacesJS += "];";
			writeToFile(location, "interfaces.js", interfacesJS);
			
			// write "files"
			/*string files = "";
			//foreach (KeyValuePair<int, string> fileEntry in m_cFiles) { interfaces += fileEntry.Value + "," + fileEntry.Key + "\n"; }
			for (int i = 0; i < m_cFiles.Count; i++) { files += m_cFiles[i] + "," + m_cFilesAssoc[i] + "\n"; }*/

			string filesJS = "var FILE_LIST = [";
			for (int i = 0; i < m_cFiles.Count; i++)
			{
				string link = DocGenerator.makeSafeLink(m_cFiles[i]) + ".html";
				filesJS += "'" + m_cFiles[i] + "," + m_cFilesAssoc[i] + "," + link + "'";
				if (i < m_cFiles.Count - 1) { filesJS += ","; }
			}
			filesJS += "];";
			writeToFile(location, "files.js", filesJS);
			
			// TODO TODO: add in index generation function here as well, not in the program
			// TODO: also add project description/notes thing for .apiproj files

			return null;
		}

		private bool writeToFile(string location, string fileName, string content)
		{
			StreamWriter fileStream = null;
			try { fileStream = new StreamWriter(location + "/" + fileName, false); }
			catch (IOException e) { return false; } 

			fileStream.WriteLine(content); 
			fileStream.Close();

			return true;
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
