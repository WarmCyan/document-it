//********************************************************
// File: DocGenerator.cs
// Author: Nathan Martindale
// Date Created: 1/19/2015
// Date Edited: 3/8/2016
// Copyright © 2016 Digital Warrior Labs
// Description: Class that handles creating the actual html
//********************************************************


//FOR NOW THIS GENERATOR IS GOING TO ASSUME THAT THERE IS A SINGLE ROOT CODE OBJECT, EITHER A CLASS OR AN INTERFACE.

//TODO: have this organize all sections alphabetically

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
	class DocGenerator
	{
		//member variables
		private CodeDocument m_cDocument = null;

		private CodeObject m_cRoot = null;
		private string m_sHTML = ""; //save all the html text to this string

		private string m_sTopHeaderText = "@Document It!";
		private int m_iSectionNumber = 0;

		//construction
		public DocGenerator() { }
		public DocGenerator(CodeDocument doc, int sectionNumber)
		{
			EngineGovernor.log("DocGenerator initialized.");
			m_cDocument = doc;
			m_iSectionNumber = sectionNumber;
		}

		//properties
		public string TopHeaderText { get { return m_sTopHeaderText; } set { m_sTopHeaderText = value; } }

		//eventually allow custom stylesheet file to be input into this function, as well as other options
		public List<string> createHTMLDocument(string destFolder)
		{
			//first assign root object
			m_cRoot = m_cDocument.CodeObjects[0];
			//EngineGovernor.log("DEBUG - Saved a root object.", 1);

			if (m_cRoot.CodeType != "class" && m_cRoot.CodeType != "interface") { EngineGovernor.log("ERROR - Could not find a root code object of type class or interface. Generator currently has no implementation for this type of file.", -1); return null; }

			string fileName = "";
			string rootName = "";
			string description = "";

			alphabetizeDoc();

			List<string> nameDescrip = new List<string>();

			EngineGovernor.log("Generating HTML...");
			htmlHead();
			nameDescrip = topInfo();
			tableOfContents();
			constructorIndex();
			functionIndex();
			constantList();
			propertyList();
			constructorList();
			functionList();
			footerStuff();

			EngineGovernor.log("HTML Generation complete.");

			fileName = writeHTML(destFolder);
			checkCSS(destFolder);
			checkJS(destFolder);

			rootName = nameDescrip[0];
			description = nameDescrip[1];

			List<string> info = new List<string>();
			info.Add(fileName);
			info.Add(rootName);
			info.Add(description);
			return info;
		}
	
		//alphabetize all code objects (so all lists are in alphabetical order)
		//NOTE, ONLY ALPHABETIZING CHILDREN OF ROOT. 
		private void alphabetizeDoc()
		{
			EngineGovernor.log("Alphabetizing document objects...");

			//get current list of code objects
			List<CodeObject> docObjects = m_cRoot.Children;
			int objectCount = docObjects.Count;

			//EngineGovernor.log("DEBUG - Preparing to alphabetize " + objectCount + " objects", 1);

			//foreach list (continually decreasing) starting at 1 to avoid root (0)
			for (int startIndex = 1; startIndex < objectCount - 1; startIndex++)
			{
				//EngineGovernor.log("DEBUG - : Starting from index " + startIndex, 1);
				int indexOfLowest = startIndex; //by default, say the first one in list is lowest
				//EngineGovernor.log("DEBUG - : : Default lowest is " + docObjects[indexOfLowest].Name, 1);
				for (int i = startIndex + 1; i < objectCount; i++)
				{
					//EngineGovernor.log("DEBUG - : : Comparing " + docObjects[indexOfLowest].Name + " with " + docObjects[i].Name, 1);

					//change if 1
					int result = compareAlpha(docObjects[indexOfLowest].Name, docObjects[i].Name);
					if (result == 1) { indexOfLowest = i; /*EngineGovernor.log("DEBUG - : : : " + docObjects[i].Name + " was lower, replacing index of lowest.", 1);*/ }
				}
				
				//checked all of them, now swap
				docObjects = swapIndicies(indexOfLowest, startIndex, docObjects);
				//EngineGovernor.log("DEBUG - : : Swapped " + indexOfLowest + " with " + startIndex, 1);
			}

			m_cRoot.Children = docObjects;
			EngineGovernor.log("Finished alphabetizing");
		}
		
		//private int findIndexOfLowest(List<CodeObject>
		private List<CodeObject> swapIndicies(int index1, int index2, List<CodeObject> list)
		{
			CodeObject temp = list[index1];
			list[index1] = list[index2];
			list[index2] = temp;

			return list;
		}

		//returns -1 if string1 is alphabetically less than string2 (a, b), returns 1 if string 2 is alphabetically 
		//	less than string1 (b, a), and 0 if exact same word (b, b)
		//	NOTE: If two strings are the exact same but one is longer (getSomething, getSomethingElse), returns SHORTER one as 
		//		alphabetically SOONER. (in that ex, returns -1)
		private int compareAlpha(string string1, string string2)
		{
			//put all in lowercase (all needs to be in same case for alphabetizing to work)
			string1 = string1.ToLower();
			string2 = string2.ToLower();

			int charIndex = 0;
			char character1 = string1[charIndex];
			char character2 = string2[charIndex];

			if (character1 < character2) { return -1; }
			if (character2 < character1) { return 1; }

			//if first letter is same, continue comparing the rest of the letters
			while(character1 == character2)
			{
				charIndex++;

				//check if strings are same length and if we've reached end
				//	(means they're the same word)
				if (string1.Length == string2.Length && charIndex >= string1.Length) { return 0; }

				//check if one is shorter than the other and we've reached end
				if (charIndex >= string1.Length) { return -1; }
				if (charIndex >= string2.Length) { return 1; }

				//otherwise, continue comparing
				character1 = string1[charIndex];
				character2 = string2[charIndex];

				if (character1 < character2) { return -1; }
				if (character2 < character1) { return 1; }
			}

			return 2; //SOMETHING SCREWED UP IF YOU GET HERE.
		}

		//returns a version of the link text WITHOUT punctuation, and lowercase, etc. etc....
		// (static so that it can be accessed ffrom outside)
		public static string makeSafeLink(string originalLink)
		{
			string link = originalLink.ToLower();

			link = DocGenerator.takeOutString(link, ",");
			link = DocGenerator.takeOutString(link, ".");
			link = DocGenerator.takeOutString(link, "'");
			link = DocGenerator.takeOutString(link, "\"");
			link = DocGenerator.takeOutString(link, ";");
			link = DocGenerator.takeOutString(link, "?");
			link = DocGenerator.takeOutString(link, "!");
			link = DocGenerator.takeOutString(link, "(");
			link = DocGenerator.takeOutString(link, ")");
			link = DocGenerator.takeOutString(link, "[");
			link = DocGenerator.takeOutString(link, "]");
			link = DocGenerator.takeOutString(link, "{");
			link = DocGenerator.takeOutString(link, "}");
			link = DocGenerator.takeOutString(link, "|");
			link = DocGenerator.takeOutString(link, "/");
			link = DocGenerator.takeOutString(link, "\\");

			return link;
		}

		//removes the specified string from the given string, IF IT HAS IT (this function performs the check. Returns original string if character(s) aren't found)
		private static string takeOutString(string source, string removeMe)
		{
			//EngineGovernor.log("DEBUG - : Trying to remove '" + removeMe + "' from '" + source + "'", 1);
			while (source.Contains(removeMe))
			{
				//EngineGovernor.log("DEBUG - : : Found string to remove", 1);
				source = source.Remove(source.IndexOf(removeMe), 1); 
				//EngineGovernor.log("DEBUG - : : Removed it! Source now reads '" + source + "'", 1);
			}
			//EngineGovernor.log("DEBUG - : Removed all instances of given string.", 1);

			return source;
		}

		//will insert <a> tags for any links it finds in the source string
		private string convertLinkTags(string source)
		{
			//EngineGovernor.log("DEBUG - : Checking '" + source + "' for links...", 1);
			if (source.Contains("@l:") || source.Contains("@link:"))
			{
				string[] words = source.Split(' ');
				string result = "";
				for (int i = 0; i < words.Length; i++)
				{
					if (words[i].Contains("@l:") || words[i].Contains("@link:"))
					{
						//EngineGovernor.log("DEBUG - : : Found a link!", 1);
						string linkText = "";
						if (words[i].IndexOf("@l:") != -1 && words[i].Length > words[i].IndexOf("@l:") + 3)
						{
							linkText = words[i].Substring(words[i].IndexOf("@l:") + 3);
							//EngineGovernor.log("DEBUG - : : : Found shortened version of link tag, taking link text: '" + linkText + "'", 1);
						}
						else if (words[i].IndexOf("@link:") != -1 && words[i].Length > words[i].IndexOf("@link:") + 6)
						{
							linkText = words[i].Substring(words[i].IndexOf("@link:") + 6);
							//EngineGovernor.log("DEBUG - : : : Found long version of link tag, taking link text: '" + linkText + "'", 1);
						}
						else { EngineGovernor.log("WARNING - Found an empty link tag: '" + source + "'", -1); }

						//string endpunctuation = ""; 

						words[i] = "<a href='" + DocGenerator.makeSafeLink(linkText) + ".html'>" + linkText + "</a>";
						//EngineGovernor.log("DEBUG - : : : Link now reads: '" + words[i] + "'", 1);
					}

					//recombine into the result string
					if (i == 0) { result = words[i]; }
					else { result += " " + words[i]; }
				}
				//EngineGovernor.log("DEBUG - : Finished substituting links. '" + result + "'", 1);
				return result;
			}
			else
			{
				//EngineGovernor.log("DEBUG - : No links found in string.", 1);
				return source;
			}
		}

		//gets the stuff within the parenethesis for parameters for input of the given object (either constructor or function) 
		private string getFunctionParameters(CodeObject obj)
		{
			//EngineGovernor.log("DEBUG - Searching code object for input parameters...", 1);
			if (obj.CodeType != "function" && obj.CodeType != "constructor") { EngineGovernor.log("ERROR - Tried to get the function parameters of an object that wasn't a class or function", -1); return ""; }
			if (obj.Inputs.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found an input!", 1);

				string returnString = "";		
				CodeObject input = obj.Inputs[0];
				for (int i = 0; i < input.Variables.Count; i++)
				{
					if (input.Variables[i].Type == "") { returnString += input.Variables[i].Name; } //added in so that if in language without strong types (javascript) it doesn't put random space before variable)
					else { returnString += "<span class='keyword'>" + convertLinkTags(input.Variables[i].Type) + "</span> " + input.Variables[i].Name; }
					if (i != input.Variables.Count - 1) { returnString += ", "; }
				}
				EngineGovernor.log("DEUBG - : Returning '" + returnString + "'", 1);
				return returnString;
			}
			else
			{
				//EngineGovernor.log("DEBUG - Found no inputs in the object, returning an empty string.", 1);
				return "";
			}
		}

		//will print out the detailed version of the input parameters of the passed input object
		private string getInputDetails(CodeObject input)
		{
			//EngineGovernor.log("DEBUG - Preparing to gather the details of the input...", 1);
			string returnString = "";
			returnString += "\n<p><b>Input Parameters</b></p>";

			foreach (CodeObject variable in input.Variables)
			{
				//EngineGovernor.log("DEBUG - : Printing out variable '" + variable.Name + "'", 1);
				returnString += "\n\t<p class='tabbedText'><code class='mediumCode'><span class='keyword'>" + convertLinkTags(variable.Type) + "</span> " + variable.Name + "</code>";

				if (variable.Description != "") {  /*EngineGovernor.log("DEBUG - : : Found description for variable.", 1);*/ returnString += "<span class='descriptionText'> - " + variable.Description + "</span>"; }
				returnString += "</p>";
			}
			//EngineGovernor.log("DEBUG - Gathered all input information.", 1);
			return returnString;
		}

		//-------------------------
		//---   HTML DOCUMENT   ---
		//-------------------------

		//starts the html, head, and body
		//top phrase is the little text in the top bar (default is @Document It!)
		private void htmlHead()
		{
			//EngineGovernor.log("DEBUG - Adding header stuff to HTML string", 1);
			html("<html>\n\t<head>");
			html("\n\t\t<title>" + m_cRoot.Name + "</title>");
			html("\n\t\t<link rel='stylesheet' type='text/css' href='api_style.css'>");
			html("\n\t\t<script src='SidebarDriver.js'></script>");
			html("\n\t\t<script src='sections.js'></script>");
			html("\n\t\t<script src='classes.js'></script>");
			html("\n\t\t<script src='interfaces.js'></script>");
			html("\n\t\t<script src='files.js'></script>");
			html("\n\t\t<script>selectedSectionIndex = " + m_iSectionNumber + ";</script>");
			html("\n\t\t<meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>");
			html("\n\t</head>\n\t<body>");
			html("\n\n<div id='top_band'></div>");
			html("\n<div id='logo_header_area'><h1>" + m_sTopHeaderText + "</h1></div>");
			html("\n<div id='side_bar' onclick='sidebarClick()'>");
			html("\n\t<div id='side_sections'></div>");
			html("\n\t<div id='side_section_contents'></div>");
			html("\n</div>");
			html("\n<div id='content'>");
			//EngineGovernor.log("DEBUG - Finished header stuff", 1);
		}
		
		//class name and description (returns the name for info purposes)
		private List<string> topInfo()
		{
			string rootName = "";

			//EngineGovernor.log("DEBUG - Inserting root information.", 1);
			html("\n\t<h1>");
			if (m_cRoot.CodeType == "interface") 
			{ 
				html("Interface"); 
				rootName = "interface"; 
				//EngineGovernor.log("DEBUG - : Root is an interface.", 1); 
			}
			else if (m_cRoot.CodeType == "class") 
			{
				html("Class");
				rootName = "class";
				//EngineGovernor.log("DEBUG - : Root is a class.", 1); 
			}
			html(" <span class='keyword'>" + m_cRoot.Name + "</span></h1>");

			rootName += " " + m_cRoot.Name;

			//description
			string description = convertLinkTags(m_cRoot.Description);
			html("\n\t<p>" + description + "</p>\n\n\t<hr/>");
			//EngineGovernor.log("DEBUG - Finished root information.", 1);

			List<string> info = new List<string>();
			info.Add(rootName);
			info.Add(description);
			return info;
		}

		//links to anchors within documents as needed
		private void tableOfContents()
		{
			//EngineGovernor.log("DEBUG - Preparing table of contents.", 1);
			if (m_cRoot.Constructors.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found constructors, creating a link to the constructor index.", 1);
				html("\n\t<p class='tabbedText'><a href='#constructorindex'>Constructor Index</a></p>");
			}
			if (m_cRoot.Functions.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found functions, creating a link to the function index.", 1);
				html("\n\t<p class='tabbedText'><a href='#functionindex'>Function Index</a></p>");
			}
			if (m_cRoot.Constants.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found constants, creating a link to the constant list.", 1);
				html("\n\t<p class='tabbedText'><a href='#constantlist'>Constant List</a></p>");
			}
			if (m_cRoot.Properties.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found properties, creating a link to the property list.", 1);
				html("\n\t<p class='tabbedText'><a href='#propertylist'>Property List</a></p>");
			}
			if (m_cRoot.Constructors.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found constructors, creating a link to the constructor list.", 1);
				html("\n\t<p class='tabbedText'><a href='#constructorlist'>Constructor List</a></p>");
			}
			if (m_cRoot.Functions.Count > 0)
			{
				//EngineGovernor.log("DEBUG - : Found functions, creating a link to the functions list.", 1);
				html("\n\t<p class='tabbedText'><a href='#functionlist'>Function List</a></p>");
			}
			html("\n\t<hr/>");
			//EngineGovernor.log("DEBUG - Finished table of contents.", 1);
		}

		//prints out the basic list of constructors
		private void constructorIndex()
		{
			if (m_cRoot.Constructors.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found constructors, creating constructor index...", 1);
				html("\n\n\t<a name='constructorindex'></a>\n\t<h2>Constructor Index</h2>\n\t<p>Click on any of the following constructor names to see more detail about them below.</p>");
				html("\n\t<table class='dataTable'>");

				int row = 1; //for banded row styles

				for (int i = 0; i < m_cRoot.Constructors.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing constructor...", 1);
					CodeObject current = m_cRoot.Constructors[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td><code><a href='#constructor" + i + "'>" + current.Name + "</a>(" + getFunctionParameters(current) + ")</code></td>");
					html("\n\t\t</tr>");

					//change row number to add banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating constructor index.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No constructors, ignoring constructor index...", 1); }
		}

		//prints out the basic list of functions
		private void functionIndex()
		{
			if (m_cRoot.Functions.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found functions, creating function index...", 1);
				html("\n\n\t<a name='functionindex'></a>\n\t<h2>Function Index</h2>\n\t<p>Click on any of the following function names to see more detail about them below.</p>");
				html("\n\t<table class='dataTable'>\n\t\t<tr><th>Modifier/Return Type</th><th>Function Name</th></tr>");

				int row = 1; //for banding

				for (int i = 0; i < m_cRoot.Functions.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing function...", 1);
					CodeObject current = m_cRoot.Functions[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td class='return_modifier_box'><code><span class='keyword'>" + convertLinkTags(current.Type) + "</span></code></td>");
					html("\n\t\t\t<td><code><a href='#function" + i + "'>" + current.Name + "</a>(" + getFunctionParameters(current) + ")</code></td>");
					html("\n\t\t</tr>");

					//for banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating function index.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No functions, ignoring function index...", 1); }
		}

		//prints out each constructor with its details
		private void constantList()
		{
			if (m_cRoot.Constants.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found constants, creating constant list...", 1);
				html("\n\n</br>\n<hr/>\n\n\t<a name='constantlist'></a>\n\t<h3>Constant List</h3>");
				html("\n\t<table class='dataTable'>");

				int row = 1; //for banding

				for (int i = 0; i < m_cRoot.Constants.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing Constant...", 1);
					CodeObject current = m_cRoot.Constants[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td><code class='biggerCode'><span class='keyword'>" + convertLinkTags(current.Type) + "</span> " + current.Name + "</code>");
					if (current.Description != "") { html(" - " + convertLinkTags(current.Description)); }
					html("</td>\n\t\t</tr>");

					//for banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating constant list.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No constants, ignoring constant list...", 1); }
		}

		//prints out each property with its details
		private void propertyList()
		{
			if (m_cRoot.Properties.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found properties, creating property list...", 1);
				html("\n\n</br>\n<hr/>\n\n\t<a name='propertylist'></a>\n\t<h3>Property List</h3>");
				html("\n\t<table class='dataTable'>");

				int row = 1; //for banding

				for (int i = 0; i < m_cRoot.Properties.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing property...", 1);
					CodeObject current = m_cRoot.Properties[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td><code class='biggerCode'><span class='keyword'>" + convertLinkTags(current.Type) + "</span> " + current.Name + "</code>");
					if (current.Description != "") { html(" - " + convertLinkTags(current.Description)); }
					html("</td>\n\t\t</tr>");

					//for banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating properties list.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No constants, ignoring property list...", 1); }
		}

		//prints out each constructor with its details
		private void constructorList()
		{
			if (m_cRoot.Constructors.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found constructors, creating constructor list...", 1);
				html("\n\n</br>\n<hr/>\n\n\t<a name='constructorlist'></a>\n\t<h3>Constructor List</h3>");
				html("\n\t<table class='dataTable'>");

				int row = 1; //for banding

				for (int i = 0; i < m_cRoot.Constructors.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing constructor...", 1);
					CodeObject current = m_cRoot.Constructors[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td>\n\t\t\t\t<a name='constructor" + i + "'></a><code class='biggerCode'>" + current.Name + "(" + getFunctionParameters(current) + ")</code></br></br>");

					//insert description
					//if (current.Description == "") { EngineGovernor.log("DEBUG - : : Didn't find a description. Substituting 'No description.'", 1); current.Description = "No description."; }
					if (current.Description != "") { html("\n\t\t\t\t<p>" + convertLinkTags(current.Description) + "</p>"); }

					//check for inputs
					if (current.Inputs.Count > 0)
					{
						//EngineGovernor.log("DEBUG - : An input was found!", 1);
						html(getInputDetails(current.Inputs[0]));
					}

					html("\n\t\t\t</td>\n\t\t</tr>");

					//for banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating constructor list.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No constants, ignoring constructor list...", 1); }
		}

		//prints out each constructor with its details
		private void functionList()
		{
			if (m_cRoot.Functions.Count > 0)
			{
				//EngineGovernor.log("DEBUG - Found functions, creating function list...", 1);
				html("\n\n</br>\n<hr/>\n\n\t<a name='functionlist'></a>\n\t<h3>Function List</h3>");
				html("\n\t<table class='dataTable'>");

				int row = 1; //for banding

				for (int i = 0; i < m_cRoot.Functions.Count; i++)
				{
					//EngineGovernor.log("DEBUG - : Printing function...", 1);
					CodeObject current = m_cRoot.Functions[i];
					html("\n\t\t<tr class='row" + row + "'>");
					html("\n\t\t\t<td>\n\t\t\t\t<a name='function" + i + "'></a><span class='function_title'>" + current.Name + "</span></br><code class='biggerCode'><span class='keyword'>" + convertLinkTags(current.Type) + "</span> " + current.Name + "(" + getFunctionParameters(current) + ")</code></br></br>");

					//insert description
					//if (current.Description == "") { EngineGovernor.log("DEBUG - : : Didn't find a description. Substituting 'No description.'", 1); current.Description = "No description."; }
					if (current.Description != "") { html("\n\t\t\t\t<p>" + convertLinkTags(current.Description) + "</p>"); }

					//check for inputs
					if (current.Inputs.Count > 0)
					{
						//EngineGovernor.log("DEBUG - : An input was found!", 1);
						html(getInputDetails(current.Inputs[0]));
					}

					//check for output
					if (current.Outputs.Count > 0)
					{
						//EngineGovernor.log("DEBUG - : An output was found!", 1);
						html("\n\t\t\t\t<p><b>Output</b></p>");
						html("\n\t\t\t\t<p class='tabbedText'>" + convertLinkTags(current.Outputs[0].Description) + "</p>");
					}

					html("\n\t\t\t</td>\n\t\t</tr>");

					//for banding
					if (row == 1) { row = 2; }
					else { row = 1; }
				}
				html("\n\t</table>");
				//EngineGovernor.log("DEBUG - Finished creating function list.", 1);
			}
			//else { EngineGovernor.log("DEBUG - No constants, ignoring function list...", 1); }
		}

		//prints out the final HTML footer stuffs
		private void footerStuff()
		{
			//EngineGovernor.log("DEBUG - Writing out footer stuff.", 1);
			html("\n\t</div>\n\t<div id='bottom_band_container'><div id='bottom_band'>\n\t\t<div id='footer_center'>");
			html("<p><i>Generated by Document It engine " + EngineGovernor.VERSION() + "</i></br>Copyright © 2015 Digital Warrior Labs</p>");
			html("\n\t</div></div>\n</body>\n</html>");
			//EngineGovernor.log("DEBUG - Footer complete.", 1);
		}

		//save the HTML string to a file (return filename)
		private string writeHTML(string destFolder)
		{
			EngineGovernor.log("Writing HTML string to file...");

			//make sure destFolder has proper ending
			//EngineGovernor.log("DEBUG - Checking destination folder string format...", 1);
			if (!destFolder.EndsWith("/")) 
			{
				//EngineGovernor.log("DEBUG - : Did NOT end with a slash, adding one now...", 1);
				destFolder += "/";
			}

			//EngineGovernor.log("DEBUG - Deciding HTML file name...", 1);
			string filename = DocGenerator.makeSafeLink(m_cRoot.Name) + ".html";
			//EngineGovernor.log("DEBUG - Decided on '" + filename + "'", 1);
			//EngineGovernor.log("DEBUG - Final file path is '" + destFolder + filename + "'", 1);

			//write out the HTML

			StreamWriter fileStream = null;
			try { fileStream = new StreamWriter(destFolder + filename, false); }
			catch (IOException e) { EngineGovernor.log("ERROR - Could not open file stream. Did you input the correct destination folder path? (" + destFolder + " was given.)", -1); Environment.Exit(2); }
			fileStream.WriteLine(m_sHTML);
			fileStream.Close();
			EngineGovernor.log("Finished file creation.");
			return filename;
		}

		private void checkJS(string destFolder)
		{
			EngineGovernor.log("Checking destination '" + destFolder + "' for SidebarDriver.js");

			if (!destFolder.EndsWith("/")) { destFolder += "/"; }

			// check if it already exists or not
			if (!File.Exists(destFolder + "SidebarDriver.js"))
			{
				EngineGovernor.log("Didn't find JavaScrit files in destination folder, attempting to copy one from local folder...");

				string localJSCopy = Uri.UnescapeDataString(new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "/SidebarDriver.js").AbsolutePath);

				// make sure local copy exists
				if (!File.Exists(localJSCopy))
				{
					EngineGovernor.log("WARNING - No local copy of SidebarDriver.js was found. Engine will continue, but result files will not have the proper stylsheet applied. Please reacquire your copy of SidebarDriver.js.", -1);

					return;
				}

				// copy local version
				File.Copy(localJSCopy, destFolder + "SidebarDriver.js");
				EngineGovernor.log("Copied local version of SidebarDriver.js to destination folder!");
			}
			else { EngineGovernor.log("JavaScript files found in destination folder, no further action necessary"); }
		}

		//make sure that an api_style.css file exists in the output folder, if not try to copy local one there
		private void checkCSS(string destFolder)
		{
			EngineGovernor.log("Checking destination '" + destFolder + "' for api_style.css");

			//first correct folder if necessary
			//EngineGovernor.log("DEBUG - Checking destination folder string format...", 1);
			if (!destFolder.EndsWith("/"))
			{
				//EngineGovernor.log("DEBUG - : Did NOT end with a slash, adding one now...", 1);
				destFolder += "/";
			}

			//check to see if it exists
			if (!File.Exists(destFolder + "api_style.css"))
			{
				EngineGovernor.log("Didn't find the stylesheet in destination folder, attempting to copy one from local folder...");

				//SURELY THERE'S AN EASIER WAY TO FREAKING DO THIS??????
				string localCSSCopy = Uri.UnescapeDataString(new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "/api_style.css").AbsolutePath);

				//make sure local copy exists
				if (!File.Exists(localCSSCopy))
				{
					EngineGovernor.log("WARNING - No local copy of api_style.css was found. Engine will continue, but resultant files will not have the proper stylesheet applied. Please reacquire your copy of api_style.css.", -1);
					return; 
				}

				//copy local version
				File.Copy(localCSSCopy, destFolder + "api_style.css");
				EngineGovernor.log("Copied local version of api_style.css to destination folder!");
			}
			else
			{
				EngineGovernor.log("Stylesheet found in destination folder, no further action necessary");
			}
		}

		private void unitTestHTMl() { EngineGovernor.log(m_sHTML, 2); }

		//shortcut to save html text
		private void html(string HTML) { m_sHTML += HTML; }
	}
}
