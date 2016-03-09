//*************************************************************
//  File: EngineGovernor.cs
//  Date created: 1/15/2015
//  Date edited: 3/8/2016
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
		public static int VERSION_MAJOR = 2;
		public static int VERSION_MINOR = 0;
		public static int VERSION_BUILD = 38161;
		public static int VERSION_REVISION = 0;
		public static string BUILD_DATE = "3/8/2016";


		//member variables
		private static List<ILogger> m_cRegisteredLogObservers = new List<ILogger>();

		private CodeDocument m_cWorkingDocument = null; // TODO: make this a list!
		// TODO: take in sections, output javascript code and lists of all sections/classes associated with those sections, and then javascript can auto populate the side bar on the left of the page


		private List<CodeDocument> m_cWorkingDocuments = new List<CodeDocument>();
		private List<int> m_cWorkingDocumentsAssoc = new List<int>();

		private List<string> m_cSections = new List<string>();

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
			List<CodeDocument> docList = parser.parseFile(filename);
			foreach (CodeDocument doc in docList) 
			{ 
				m_cWorkingDocuments.Add(doc); 
				m_cWorkingDocumentsAssoc.Add(section);
				if (doc.CodeObjects[0].CodeType == "class") 
				{ 
					m_cClasses.Add(doc.CodeObjects[0].Name); 
					m_cClassesAssoc.Add(section);
				} 
				else if (doc.CodeObjects[0].CodeType == "interface") 
				{ 
					m_cInterfaces.Add(doc.CodeObjects[0].Name); 
					m_cInterfacesAssoc.Add(section);
				}
				else 
				{ 
					m_cFiles.Add(doc.CodeObjects[0].Name); 
					m_cFilesAssoc.Add(section);
				} 
			}
			log("Engine governor has received parser's documents and stored in list of current working documents.");
		}

		// creates the html API documentation (assumes m_cWorkingDocument has already been assigned)
		// NOTE: generates ALL api documentation of ALL code that has been analyzed up until this point!
		public void generateAPIDoc(string location, string topHeaderText)
		{
			List<List<string>> info = new List<List<string>>();
			for (int i = 0; i < m_cWorkingDocuments.Count; i++)
			{
				DocGenerator generator = new DocGenerator(m_cWorkingDocuments[i], m_cWorkingDocumentsAssoc[i]);
				generator.TopHeaderText = topHeaderText;
				info.Add(generator.createHTMLDocument(location));
			}
			
			// sections
			string sectionsJS = "var SECTION_LIST = [";
			for (int i = 0; i < m_cSections.Count; i++)
			{
				sectionsJS += "'" + m_cSections[i] + "'";
				if (i < m_cSections.Count - 1) { sectionsJS += ","; }
			}
			sectionsJS += "];";
			writeToFile(location, "sections.js", sectionsJS);
			
			// classes
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
			string filesJS = "var FILE_LIST = [";
			for (int i = 0; i < m_cFiles.Count; i++)
			{
				string link = DocGenerator.makeSafeLink(m_cFiles[i]) + ".html";
				filesJS += "'" + m_cFiles[i] + "," + m_cFilesAssoc[i] + "," + link + "'";
				if (i < m_cFiles.Count - 1) { filesJS += ","; }
			}
			filesJS += "];";
			writeToFile(location, "files.js", filesJS);

			createIndex(info, topHeaderText, location);
		}
		
		static void createIndex(List<List<string>> information, string projectTitle, string projectFolder)
		{
			string html = "";

			html += "<html>\n\t<head>";
			html += "\n\t\t<title>" + projectTitle + " Project</title>";
			html += "\n\t\t<link rel='stylesheet' type='text/css' href='api_style.css'>";
			html += "\n\t\t<script src='SidebarDriver.js'></script>";
			html += "\n\t\t<script src='sections.js'></script>";
			html += "\n\t\t<script src='classes.js'></script>";
			html += "\n\t\t<script src='interfaces.js'></script>";
			html += "\n\t\t<script src='files.js'></script>";
			html += "\n\t\t<meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>";
			html += "\n\t</head>\n\t<body>";
			html += "\n\n<div id='top_band'></div>";
			html += "\n<div id='logo_header_area'><h1>" + projectTitle + "</h1></div>";
			html += "\n<div id='side_bar' onclick='sidebarClick()'>";
			html += "\n\t<div id='side_sections'></div>";
			html += "\n\t<div id='side_section_contents'></div>";
			html += "\n</div>";
			html += "\n<div id='content'>";
			html += "\n<h2>" + projectTitle + " Class Index</h2>";

			html += "\n\t<table class='dataTable'>\n\t\t<tr><th>Class/Interface</th><th>Description</th></tr>";

			int row = 1; //for banding

			for (int i = 0; i < information.Count; i++)
			{
				//get all the specific information for current class/interface
				string type = information[i][1].Substring(0, information[i][1].IndexOf(" "));
				string name = information[i][1].Substring(information[i][1].IndexOf(" "));
				string file = information[i][0];
				string descrip = information[i][2];

				html += "\n\t\t<tr class='row" + row + "'>";
				html += "\n\t\t\t<td class='return_modifier_box'><code><span class='keyword'>" + type + "</span> <a href='" + file + "'>" + name + "</a></code></td>";
				html += "\n\t\t\t<td>" + descrip + "</td>";
				html += "\n\t\t</tr>";

				//for banding
				if (row == 1) { row = 2; }
				else { row = 1; }
			}
			html += "</table>\n\t</div>\n\t<div id='bottom_band_container'><div id='bottom_band'>\n\t\t<div id='footer_center'>";
			html += "<p><i>Index page generated by DICI " + VERSION() + "</i></br>Copyright © 2016 Digital Warrior Labs</p>";
			html += "</div></div></body></html>";

			if (!projectFolder.EndsWith("/"))
			{
				projectFolder += "/";
			}

			StreamWriter fileStream = new StreamWriter(projectFolder + "index.html", false);
			fileStream.WriteLine(html);
			fileStream.Close();
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
			return VERSION_MAJOR + "." + VERSION_MINOR + "." + VERSION_REVISION;
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
