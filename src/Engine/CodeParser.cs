//*************************************************************
//  File: CodeParser.cs
//  Date created: 1/16/2015
//  Date edited: 2/28/2016
//  Author: Nathan Martindale
//  Copyright © 2016 Digital Warrior Labs
//  Description: Takes a file, parses it into codeobjects and then returns a codedocument
//*************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
	class CodeParser
	{
		//member variables
		private string m_sFileName = "";
		private StreamReader m_cFileStream = null;

		//line collections
		private List<string> m_cFileLines = new List<string>();
		private List<int> m_cFileLinesTypes = new List<int>(); //every returned result from isCommentLine()
		private List<List<string>> m_cCommentBlocks = new List<List<string>>();
		private List<List<int>> m_cCommentBlocksLineIndex = new List<List<int>>(); 
		private List<List<string>> m_cSyntaxBlocks = new List<List<string>>();

		//functions
		//public CodeDocument parseFile(string filename)
		public List<CodeDocument> parseFile(string filename)
		{
			EngineGovernor.log("Parser initialized.");
			EngineGovernor.log("File passed for parsing: " + filename);
			m_sFileName = filename;

			getFileLines();
			EngineGovernor.log("Preparing to analyze lines...");
			getCommentBlocks();
			//unitTestPrintCommentBlockLists();
			getSyntaxBlocks();
			//unitTestSyntaxBlocks();

			EngineGovernor.log("Analyzing syntax blocks...");

			//one of these two (or possibly neither) will be the super container for all the rest of the objects that are returned
			//CodeObject potentialClass = null;
			//CodeObject potentialInterface = null;
			List<CodeObject> currentCodeObjects = new List<CodeObject>(); //after all are collected, this will be set as the children for either the potential class or interface (or the document itself if no class/interface)

			CodeObject rootObject = null; //this will end up being either the potentialClass or the potentialInterface This will be the object added to the CodeDocument

			List<CodeDocument> documents = new List<CodeDocument>();

			// get all code objects into currentCodeObjects
			foreach (List<string> syntaxBlock in m_cSyntaxBlocks)
			{
				List<CodeObject> blockObjects = analyzeSyntaxBlock(syntaxBlock);
				foreach (CodeObject obj in blockObjects) { currentCodeObjects.Add(obj); }
			}

			// Compile classes/interfaces/files based on found codeobjects
			foreach (CodeObject obj in currentCodeObjects)
			{
				if (obj.CodeType == "class" || obj.CodeType == "interface")
				{
					// if we already had a root object, make a document out of everything that came before
					if (rootObject != null)
					{
						CodeDocument doc = new CodeDocument();
						doc.CodeObjects.Add(rootObject);
						documents.Add(doc);
					}
					rootObject = obj;
					continue;
				}
				
				// if by this point we don't have an actual root, this must be just a file of functions, so make file
				if (rootObject == null) 
				{ 
					rootObject = new CodeObject(filename, "", "", "FILE", new List<CodeObject>()); 
					continue;
				}
				
				// otherwise go ahead and add to whatever is current
				rootObject.Children.Add(obj);
			}

			// at this point, all code objects have been analyzed, so take whatever last root object we had was and turn into documnet
			CodeDocument document = new CodeDocument();
			document.CodeObjects.Add(rootObject);
			documents.Add(document);
			

			//compile all the code objects from the syntax blocks
			/*foreach (List<string> syntaxBlock in m_cSyntaxBlocks)
			{
				List<CodeObject> blockObjects = analyzeSyntaxBlock(syntaxBlock);
				foreach (CodeObject obj in blockObjects)
				{
					if (obj.CodeType == "class") // NOTE: This actually means we've finished LAST class, so reassign current root object?
					{
						EngineGovernor.log("DEBUG - SUPER - We have a class object!", 1);
						if (potentialClass != null) //we already had a class...
						{
							EngineGovernor.log("WARNING - Found another class in the same file. Ignoring new class.", -1);
						}
						else { potentialClass = obj; }
					}
					else if (obj.CodeType == "interface")
					{
						EngineGovernor.log("DEBUG - SUPER - We have an interface object!", 1);
						if (potentialInterface != null) //we already had an interface...
						{
							EngineGovernor.log("WARNING - Found another interface in the same file. Ignoring new interface", -1);
						}
						else { potentialInterface = obj; }
					}
					else { currentCodeObjects.Add(obj); EngineGovernor.log("DEBUG - Adding an object of type " + obj.CodeType + " to the currentCodeObjects list", 1); }
				}
			}

			//check whether this is an interface or class, then add the list of codeobjects to it.
			if (potentialInterface != null && potentialClass != null)
			{
				EngineGovernor.log("ERROR - Found both an interface and a class in the same file. Parse engine is stopping...", -1);
				return null;
			}
			else if (potentialClass == null)
			{
				EngineGovernor.log("DEBUG - Setting interface as root code object", 1);
				potentialInterface.Children = currentCodeObjects;
				rootObject = potentialInterface;
			}
			else if (potentialInterface == null)
			{
				EngineGovernor.log("DEBUG - Setting class as root code object", 1);
				potentialClass.Children = currentCodeObjects;
				rootObject = potentialClass;
			}*/

			//List<CodeObject> testList = new List<CodeObject>();
			//testList.Add(rootObject);
			//unitTestDumpCodeObjects(testList);

			EngineGovernor.log("Analysis complete.");

			//EngineGovernor.log("DEBUG - SUPER - Storing everything in a codeDocument and returning it to the engine", 1);
			//CodeDocument doc = new CodeDocument();
			//doc.CodeObjects.Add(rootObject);

			EngineGovernor.log("Parser has completed job successfully! Returning the completed CodeDocument analysis to the engine governor...");
			//return doc; 
			return documents; 
		}

		//try to assign the file stream based on name, and then read all lines into the List m_cFileLines
		private void getFileLines()
		{
			EngineGovernor.log("Attempting to open file at " + m_sFileName + "...");
			try { m_cFileStream = new StreamReader(m_sFileName); }
			catch (IOException e) { EngineGovernor.log("ERROR - Could not open file stream. Did you input the correct file path?", -1); Environment.Exit(2); }
			EngineGovernor.log("\tOpened file stream successfully!");

			//read the lines
			string currentLine = "";
			int lineIndex = 0;
			while ((currentLine = m_cFileStream.ReadLine()) != null)
			{
				m_cFileLines.Add(currentLine);
				lineIndex++;
			}
			EngineGovernor.log("\tRead in " + lineIndex + " lines.");

			//fill the line type with entries
			m_cFileLinesTypes.Capacity = lineIndex;
			for (int i = 0; i < m_cFileLinesTypes.Capacity; i++)
			{
				m_cFileLinesTypes.Add(0);
			}

			m_cFileStream.Close();
			EngineGovernor.log("\tClosed file stream.");
		}

		//get all blocks of '//' or '/**/' (eventually change to allow different delimiters)
		private void getCommentBlocks()
		{
			EngineGovernor.log("Searching for comment blocks...");

			int currentLineIndex = 0;
			int commentBlockIndex = 0; //index within the List<List<>>>
			bool commentFound = false;

			while (currentLineIndex < m_cFileLines.Count)
			{
				//potentially beginning of comment
				int commentBeginningLineIndex = 0; //these indicies should always be INCLUSIVE. If it's a one line comment, they'll both be the same
				int commentEndingLineIndex = 0;
				int lineResult = isCommentLine(false, currentLineIndex);

				if (lineResult == 1) //found the start of a normal comment
				{
					commentFound = true;
					commentBeginningLineIndex = currentLineIndex;
					currentLineIndex++; //so that it doesn't review the first line twice
					//EngineGovernor.log("DEBUG - SUPER - Found comment starting on line " + commentBeginningLineIndex, 1);
					while (isCommentLine(false, currentLineIndex) == 1) { currentLineIndex++; }
					currentLineIndex--; //based on the logic, it will always extend one past the end of the comment
					commentEndingLineIndex = currentLineIndex;
					//EngineGovernor.log("DEBUG - SUPER - That comment ends on line " + commentEndingLineIndex, 1);
				}
				else if (lineResult == 2) //found the start of a block comment
				{
					commentFound = true;
					commentBeginningLineIndex = currentLineIndex;
					currentLineIndex++;
					//EngineGovernor.log("DEBUG - SUPER - Found block comment starting on line " + commentBeginningLineIndex, 1);
					while (isCommentLine(true, currentLineIndex) == 3) { currentLineIndex++; }
					commentEndingLineIndex = currentLineIndex;
					//EngineGovernor.log("DEBUG - SUPER - That block comment ends on line " + commentEndingLineIndex, 1);
				}
				else if (lineResult == 5) //found a single line block comment
				{
					commentFound = true;
					commentBeginningLineIndex = currentLineIndex;
					commentEndingLineIndex = currentLineIndex;
					//EngineGovernor.log("DEBUG - SUPER - Found single line block comment on line " + currentLineIndex, 1);
				}

				//save the comment lines
				if (commentFound)
				{
					EngineGovernor.log("\tFound a comment on lines " + (int)(commentBeginningLineIndex + 1) + " - " + (int)(commentEndingLineIndex + 1)); //adding 1 to account for real line numbers, not zero-based
					List<string> commentedLines = new List<string>();
					List<int> commentLineIndex = new List<int>();
					for (int i = commentBeginningLineIndex; i <= commentEndingLineIndex; i++)
					{
						commentedLines.Add(m_cFileLines[i]);
						commentLineIndex.Add(i);
						//EngineGovernor.log("DEBUG - Added line " + i + " to comment block " + commentBlockIndex, 1);
					}
					m_cCommentBlocks.Add(commentedLines);
					m_cCommentBlocksLineIndex.Add(commentLineIndex);
					//EngineGovernor.log("DEBUG - Comment block " + commentBlockIndex + " was added to the list of comment blocks", 1);
					commentFound = false;
					commentBlockIndex++;
				}

				currentLineIndex++;
			}
		}

		//returns 1 if the line has an instance of the normal comment delimiter (pass in true for first parameter if you've already found a '/*')
		//returns 2 if the line has a block comment start (/*)
		//returns 3 if the line is INSIDE OF A BLOCK COMMENT
		//returns 4 if the line ends a block comment
		//returns 5 if the single line IS a block comment
		//(returns 0 if no comment)
		private int isCommentLine(bool blockComment, int requestedLineIndex)
		{
			if (requestedLineIndex >= m_cFileLines.Count) { EngineGovernor.log("DEBUG - WARNING - Tried analyzing too far and went past total line count. Returning 0.", 1); return 0; }
			string line = m_cFileLines[requestedLineIndex];

			//if not in a block comment, check for obvious '//'
			if (!blockComment)
			{
				//EngineGovernor.log("DEBUG - Analyzing line " + requestedLineIndex + " in NONBLOCK comment mode", 1);
				bool foundNormal = false;
				bool foundBlock = false;

				if (line.Contains("//")) { foundNormal = true; /*EngineGovernor.log("DEBUG - : Found normal comment delimiter", 1);*/ }
				if (line.Contains("/*")) { foundBlock = true; /*EngineGovernor.log("DEBUG - : Found block comment delimiter", 1);*/ }

				//resolve potential conflicts (if perhaps there's a '//' in the comment after a '/*')
				if (foundNormal && foundBlock)
				{
					//EngineGovernor.log("DEBUG - : Found multiple comment types, resolving conflict...", 1);

					int normalIndex = line.IndexOf("//");
					int blockIndex = line.IndexOf("/*");

					//EngineGovernor.log("DEBUG - : : Normal delimiter index = " + normalIndex, 1);
					//EngineGovernor.log("DEBUG - : : Block delimiter index = " + blockIndex, 1);
					if (normalIndex < blockIndex) { foundBlock = false; /*EngineGovernor.log("DEBUG - : : Normal delimiter found sooner, ignoring block delimiter", 1);*/ }
					else { foundNormal = false; /*EngineGovernor.log("DEBUG - : : Block delimiter found sooner, ignoring normal delimiter", 1);*/ }
				}

				if (foundNormal) 
				{ 
					//EngineGovernor.log("DEBUG - : returning 1, for normal comment delimiter", 1); 
					m_cFileLinesTypes[requestedLineIndex] = 1;
					return 1; 
				}
				//if found start of block, need to check and make sure the end isn't on the same line
				else if (foundBlock) 
				{
					//EngineGovernor.log("DEBUG - : Checking to make sure end block comment delimiter isn't on the same line...", 1);
					if (line.IndexOf("*/") > line.IndexOf("/*")) 
					{ 
						//EngineGovernor.log("DEBUG - : : Found the end block comment delimiter, returning 5 for single line block comment.", 1);
						m_cFileLinesTypes[requestedLineIndex] = 5;
						return 5; 
					}
					//else { EngineGovernor.log("DEBUG - : : No end block comment delimiter found, continuing normally.", 1); }
					//EngineGovernor.log("DEBUG - : Returning 2, for block comment delimiter", 1);
					m_cFileLinesTypes[requestedLineIndex] = 2;
					return 2; 
				}
				else 
				{ 
					//EngineGovernor.log("DEBUG - : Returning 0, no comment delimiter found", 1);
					m_cFileLinesTypes[requestedLineIndex] = 0;
					return 0; 
				}
			}
			else //block comment mode
			{
				//EngineGovernor.log("DEBUG - Analyzing line " + requestedLineIndex + " in BLOCK comment mode", 1);
				if (line.Contains("*/"))
				{
					//EngineGovernor.log("DEBUG - : Returning 4, the end block comment delimiter was found", 1);
					m_cFileLinesTypes[requestedLineIndex] = 4;
					return 4;
				}
				else 
				{ 
					//EngineGovernor.log("DEBUG - : Returning 3, currently inside of a block comment", 1);
					m_cFileLinesTypes[requestedLineIndex] = 3;
					return 3; 
				}
			}
		}

		//returns only the text within a comment, based on the type (see documentation of the function isCommentLine())
		private string getCommentedPart(string line, int commentType)
		{
			//EngineGovernor.log("DEBUG - Getting comment content of '" + line + "' Reportedly comment type " + commentType, 1);
			if (commentType == 3) //inside block comment
			{ 
				//EngineGovernor.log("DEBUG - : 3, we're inside of a block comment, return all of it: '" + line + "'", 1); 
				return line; 
			}
			else if (commentType == 1) //line with a '//'
			{
				if (line.IndexOf("//") + 2 > line.Length) { /*EngineGovernor.log("DEBUG - : 1, but the comment is blank after the delimiter, returning ''", 1);*/ return ""; }
				string afterDelimit = line.Substring(line.IndexOf("//") + 2);
				//EngineGovernor.log("DEBUG - : 1, returning everything after the delimiter, '" + afterDelimit + "'", 1);
				return afterDelimit;
			}
			else if (commentType == 2) //line with a '/*' (assumed to be multi-line, exception would have a comment type of 5)
			{
				if (line.IndexOf("/*") + 2 > line.Length) { /*EngineGovernor.log("DEBUG - : 2, but the comment is blank after delimiter, returning ''", 1);*/ return ""; }
				string afterDelimit = line.Substring(line.IndexOf("/*") + 2);
				//EngineGovernor.log("DEBUG - : 2, returning everything after the block delimiter, '" + afterDelimit + "'", 1);
				return afterDelimit;
			}
			else if (commentType == 4) //end of a block comment
			{
				string before = line.Substring(0, line.IndexOf("*/"));
				//EngineGovernor.log("DEBUG - : 4, returning everything before the end block delimiter, '" + before + "'", 1);
				return before;
			}
			else if (commentType == 5) //single line block comment
			{
				int startIndex = line.IndexOf("/*");
				startIndex += 2;
				int endIndex = line.IndexOf("*/");
				int length = endIndex - startIndex;
				string between = line.Substring(startIndex, length);
				//EngineGovernor.log("DEBUG - : 5, returning everything inside of the single line comment block, '" + between + "'", 1);
				return between;
			}
			EngineGovernor.log("WARNING - A comment type was registered incorrectly. Program could potentially crash or have incorrect results.", -1);
			return null;
		}

		//separate the comment blocks out into syntax blocks
		private void getSyntaxBlocks()
		{
			//get all text WITHIN the comments
			List<string> innerComments = new List<string>();

			EngineGovernor.log("Parsing comment content from comment lines...");
			for (int i = 0; i < m_cCommentBlocks.Count; i++)
			{
				for (int j = 0; j < m_cCommentBlocks[i].Count; j++)
				{
					//EngineGovernor.log("DEBUG - SUPER - preparing to check '" + m_cCommentBlocks[i][j] + "' (line " + m_cCommentBlocksLineIndex[i][j] + ")", 1);
					string innerComment = getCommentedPart(m_cCommentBlocks[i][j], m_cFileLinesTypes[m_cCommentBlocksLineIndex[i][j]]);
					//EngineGovernor.log("DEBUG - Obtained: " + innerComment, 1);
					innerComments.Add(innerComment);
				}
			}
			EngineGovernor.log("Successfully gathered all content.");

			//analyze all inner comments and obtain all content between the { }
			EngineGovernor.log("Finding syntax blocks within comment content...");
			
			//first find a starting brace
			int currentLine = 0;
			while (currentLine < innerComments.Count)
			{
				//EngineGovernor.log("DEBUG - Evaluating innerCommentLine " + currentLine, 1);
				 
				if (innerComments[currentLine].Contains("{"))
				{
					//EngineGovernor.log("DEBUG - : Found a starting brace!", 1);
					List<string> syntaxBlock = new List<string>();

					//assuming all on one line
					if (innerComments[currentLine].IndexOf("}") > innerComments[currentLine].IndexOf("{"))
					{
						//EngineGovernor.log("DEBUG - : Found an ending brace on the same line, this is a single line syntax block", 1);
						int start = innerComments[currentLine].IndexOf("{") + 1;
						int end = innerComments[currentLine].IndexOf("}");
						int length = end - start;

						string block = innerComments[currentLine].Substring(start, length);
						//EngineGovernor.log("DEBUG - : Adding all content between braces, '" + block + "'", 1);
						syntaxBlock.Add(block);
					}
					else //not all on one line
					{
						//add the first part of the line after the starting brace 
						string first = innerComments[currentLine].Substring(innerComments[currentLine].IndexOf("{") + 1);
						//EngineGovernor.log("DEBUG - : Adding content after the starting brace, '" + first + "'", 1);
						syntaxBlock.Add(first);
						currentLine++;

						bool flag_reachedEnd = false; //throw this flag if we reach the end during the while loop without having found an ending brace in the comments

						//get all body lines
						while (innerComments[currentLine].IndexOf("}") == -1)
						{
							//EngineGovernor.log("DEBUG - : No ending brace found on this line, add whole line, '" + innerComments[currentLine] + "'", 1);
							syntaxBlock.Add(innerComments[currentLine]);
							currentLine++;
							if (currentLine >= innerComments.Count) { EngineGovernor.log("WARNING - Did not find a closing brace for the syntax block. Check for missing closing braces in your comments.", -1); flag_reachedEnd = true; break; }
						}
						//get final line (at this point, the last while loop will have stopped at exactly the line that contains the ending brace)
						if (!flag_reachedEnd)
						{
							//EngineGovernor.log("DEBUG - : Reached line with ending brace", 1);
							int end = innerComments[currentLine].IndexOf("}");
							string before = innerComments[currentLine].Substring(0, end);
							//EngineGovernor.log("DEBUG - : Adding text before the brace, '" + before + "'", 1);
							syntaxBlock.Add(before);
						}
					}
					m_cSyntaxBlocks.Add(syntaxBlock);
					EngineGovernor.log("\tFound syntax block");
				}
				currentLine++;
			}
			
		}

		private List<CodeObject> analyzeSyntaxBlock(List<string> linesBlock)
		{
			EngineGovernor.log("DEBUG - ================================== Analyzing a syntax block ==============================", 1);
			//first combine all into one string, remove excess spaces
			string syntax = "";
			
			//remove all excess space from each line in linesBlock
			for (int i = 0; i < linesBlock.Count; i++)
			{
				//EngineGovernor.log("DEBUG - : trimming space from '" + linesBlock[i] + "'", 1);
				linesBlock[i] = linesBlock[i].Trim();
				//EngineGovernor.log("DEBUG - : : line now reads: '" + linesBlock[i] + "'", 1);
				syntax += linesBlock[i];
				//EngineGovernor.log("DEBUG - : line added to syntax, syntax is now '" + syntax + "'", 1);
			}

			List<string> tags = findTags(syntax);
			EngineGovernor.log("\tParsing tags...");

			List<CodeObject> foundObjects = new List<CodeObject>();

			//waiting objects are used for things that WILL have children/information (it's found a function, but hasn't finished getting input, output, etc)
			//when checking previous objects, CHECK THEM IN THE REVERSE ORDER THAT THEY ARE LISTED HERE
			CodeObject waitingClass = null;
			CodeObject waitingInterface = null;
			CodeObject waitingConstant = null;
			CodeObject waitingConstructor = null;
			CodeObject waitingProperty = null;
			CodeObject waitingFunction = null;
			CodeObject waitingInput = null;
			CodeObject waitingVariable = null;
			for (int i = 0; i < tags.Count; i += 2)
			{	
				string tagSpecification = tags[i];
				string tagDefinition = tags[i + 1];
				//EngineGovernor.log("DEBUG - Parsing tag '" + tagSpecification + "' : '" + tagDefinition + "'", 1);

				//check tag specification
				if (tagSpecification == "cl" || tagSpecification == "class")
				{
					//EngineGovernor.log("DEBUG - : Found a class!", 1);
					string className = tagDefinition;
					CodeObject codeClass = new CodeObject();
					codeClass.Name = className;
					codeClass.CodeType = "class";
					waitingClass = codeClass;
					//EngineGovernor.log("DEBUG - WAITING CLASS ASSIGNED: " + className, 1);
				}
				else if (tagSpecification == "intf" || tagSpecification == "interface")
				{
					//EngineGovernor.log("DEBUG - : Found an interface!", 1);
					string interfaceName = tagDefinition;
					CodeObject codeInterface = new CodeObject();
					codeInterface.Name = interfaceName;
					codeInterface.CodeType = "interface";
					waitingInterface = codeInterface;
					//EngineGovernor.log("DEBUG - WAITING INTERFACE ASSIGNED: " + interfaceName, 1);
				}
				else if (tagSpecification == "cnst" || tagSpecification == "constant")
				{
					//EngineGovernor.log("DEBUG - : Found a constant!", 1);

					string[] words = tagDefinition.Split(' ');
					string constantName = words[words.Length - 1]; //name of the constant should be the last word
					//EngineGovernor.log("DEBUG - : : the name of this constant should be '" + constantName + "'", 1);
					string constantType = "";
					for (int j = 0; j < words.Length - 1; j++) //everything except the last word (name) should be the type/modifier
					{
						constantType += words[j] + " ";
					}
					constantType = constantType.Trim(); //take out the space that will be at the end (from for loop)
					//EngineGovernor.log("DEBUG - : : the type/modifier of this constant should be '" + constantType + "'", 1);

					CodeObject codeConstant = new CodeObject();
					codeConstant.Name = constantName;
					codeConstant.Type = constantType;
					codeConstant.CodeType = "constant";
					waitingConstant = codeConstant;
					//EngineGovernor.log("DEBUG - WAITING CONSTANT ASSIGNED: " + constantName, 1);
				}
				else if (tagSpecification == "c" || tagSpecification == "constructor")
				{
					//EngineGovernor.log("DEBUG - : Found a constructor!", 1);
					string constructorName = tagDefinition;
					CodeObject codeConstructor = new CodeObject();
					codeConstructor.Name = constructorName;
					codeConstructor.CodeType = "constructor";
					waitingConstructor = codeConstructor;
					//EngineGovernor.log("DEBUG - WAITING CONSTRUCTOR ASSIGNED: " + constructorName, 1);
				}
				else if (tagSpecification == "p" || tagSpecification == "property")
				{
					//EngineGovernor.log("DEBUG - : Found a property!", 1);

					string[] words = tagDefinition.Split(' ');
					string propertyName = words[words.Length - 1]; //name of the property should be the last word
					//EngineGovernor.log("DEBUG - : : the name of this property should be '" + propertyName + "'", 1);
					string propertyType = "";
					for (int j = 0; j < words.Length - 1; j++) //everything except the last word (name) should be the type/modifier
					{
						propertyType += words[j] + " ";
					}
					propertyType = propertyType.Trim(); //take out the space that will be at the end (from for loop)
					//EngineGovernor.log("DEBUG - : : the type/modifier of this property should be '" + propertyType + "'", 1);

					CodeObject codeProperty = new CodeObject();
					codeProperty.Name = propertyName;
					codeProperty.Type = propertyType;
					codeProperty.CodeType = "property";
					waitingProperty = codeProperty;
					//EngineGovernor.log("DEBUG - WAITING PROPERTY ASSIGNED: " + propertyName, 1);
				}
				else if (tagSpecification == "f" || tagSpecification == "function")
				{
					//EngineGovernor.log("DEBUG - : Found a function!", 1);

					string[] words = tagDefinition.Split(' ');
					string functionName = words[words.Length - 1];
					//EngineGovernor.log("DEBUG - : : the name of this function should be '" + functionName + "'", 1);

					string functionType = "";
					for (int j = 0; j < words.Length - 1; j++) //everything except the last word (name) should be the type/modifier
					{
						functionType += words[j] + " ";
					}
					functionType = functionType.Trim(); //take out the space that will be at the end (from for loop)
					//EngineGovernor.log("DEBUG - : : the type/modifier of this property should be '" + functionType + "'", 1);

					CodeObject codeFunction = new CodeObject();
					codeFunction.Name = functionName;
					codeFunction.Type = functionType;
					codeFunction.CodeType = "function";
					waitingFunction = codeFunction;
					//EngineGovernor.log("DEBUG - WAITING FUNCTION ASSIGNED: " + functionName, 1);
				}
				else if (tagSpecification == "i" || tagSpecification == "input")
				{
					//EngineGovernor.log("DEBUG - : Found an input start!", 1);

					CodeObject codeInput = new CodeObject();
					codeInput.CodeType = "input";
					waitingInput = codeInput;
					//EngineGovernor.log("DEBUG - : : WAITING INPUT ASSIGNED", 1);
				}
				else if (tagSpecification == "v" || tagSpecification == "variable")
				{
					//EngineGovernor.log("DEBUG - : Found a variable!", 1);

					string[] words = tagDefinition.Split(' ');
					string variableName = words[words.Length - 1];
					//EngineGovernor.log("DEBUG - : : the name of the variable should be '" + variableName + "'", 1);

					string variableType = "";
					for (int j = 0; j < words.Length - 1; j++)
					{
						variableType += words[j] + " ";
					}
					variableType = variableType.Trim();
					//EngineGovernor.log("DEBUG - : : the type/modifier of this property should be '" + variableType + "'", 1);

					CodeObject codeVariable = new CodeObject();
					codeVariable.Name = variableName;
					codeVariable.Type = variableType;
					codeVariable.CodeType = "variable";

					//check to see if already a waiting variable. If so, means previous variable didn't have description. Add before nullifying!!
					if (waitingVariable != null)
					{
						EngineGovernor.log("WARNING - Found variable with no description", -1);
						EngineGovernor.log("Warning text: Variable '" + waitingVariable.Name + "'", -1);
						CodeObject finishedVariable = waitingVariable;
						if (waitingInput != null)
						{
							//EngineGovernor.log("DEBUG - : : : Found waiting input to add the previous waiting variable to.", 1);
							waitingInput.Children.Add(finishedVariable);
						}
						else //free variable
						{
							//EngineGovernor.log("DEBUG - : : : No waiting input found for previous waiting variable. Adding as free variable.", 1);
							foundObjects.Add(finishedVariable);
						}
					}
					
					waitingVariable = codeVariable;
					//EngineGovernor.log("DEBUG - : : WAITING VARIABLE ASSIGNED: " + variableName, 1);
				}
				else if (tagSpecification == "d" || tagSpecification == "description")
				{
					//EngineGovernor.log("DEBUG - : Found a description", 1);

					string descriptionContent = tagDefinition;
					//EngineGovernor.log("DEBUG - : : Description content is '" + descriptionContent + "'", 1);

					//check the list of waiting variables in reverse initialized order from above
					if (waitingVariable != null)
					{
						waitingVariable.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting variable was the first in the chain. Adding description to the variable", 1);

						//at this point, we know that the variable is finished, description should be the last part of it
						CodeObject finishedVariable = waitingVariable;
						if (waitingInput != null) //check if it belongs to an input
						{
							//EngineGovernor.log("DEBUG - : : : Found a waiting input, variable being assigned to it.", 1);
							waitingInput.Children.Add(finishedVariable);
						}
						else
						{
							//EngineGovernor.log("DEBUG - : : : No waiting input was found, saving as a free variable.", 1);
							foundObjects.Add(finishedVariable);
						}
						//EngineGovernor.log("DEBUG - : : Variable finished, nullifying waitingVariable.", 1);
						waitingVariable = null;
					}
					else if (waitingFunction != null)
					{
						waitingFunction.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting function was the next in the chain. Adding description to the function", 1);
					}
					else if (waitingProperty != null)
					{
						waitingProperty.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting property was the next in the chain. Adding description to the property", 1);
						CodeObject finishedProperty = waitingProperty;
						foundObjects.Add(finishedProperty);
						//EngineGovernor.log("DEBUG - : : Property finished, nullifying waitingProperty.", 1);
						waitingProperty = null;
					}
					else if (waitingConstructor != null)
					{
						waitingConstructor.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting constructor was the next in the chain. Adding description to the constructor", 1);

						//taken out because then if there's an input, it doesn't get added to the constructor
						/*CodeObject finishedConstructor = waitingConstructor;
						foundObjects.Add(finishedConstructor);
						EngineGovernor.log("DEBUG - : : Constructor finished, nullifying waitingConstructor.", 1);
						waitingConstructor = null;*/
					}
					else if (waitingConstant != null)
					{
						waitingConstant.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting constant was the next in the chain. Adding description to the constant", 1);
						CodeObject finishedConstant = waitingConstant;
						foundObjects.Add(finishedConstant);
						//EngineGovernor.log("DEBUG - : : Constant finished, nullifying waitingConstant.", 1);
						waitingConstant = null;
					}
					else if (waitingInterface != null)
					{
						waitingInterface.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting interface was the next in the chain. Adding description to the interface", 1);
						CodeObject finishedInterface = waitingInterface;
						foundObjects.Add(finishedInterface);
						//EngineGovernor.log("DEBUG - : : Interface finished, nullifying waitingInterface.", 1);
						waitingInterface = null;
					}
					else if (waitingClass != null)
					{
						waitingClass.Description = descriptionContent;
						//EngineGovernor.log("DEBUG - : : A waiting class was the next in the chain. Adding description to the class", 1);
						CodeObject finishedClass = waitingClass;
						foundObjects.Add(finishedClass);
						//EngineGovernor.log("DEBUG - : : Class finished, nullifying waitingClass.", 1);
						waitingClass = null;
					}
					else
					{
						EngineGovernor.log("WARNING - Found a description but couldn't figure out what it was describing. Check the order of your tags.", -1);
						EngineGovernor.log("Warning text: description was '" + descriptionContent + "'", -1);
					}
				}
				else if (tagSpecification == "o" || tagSpecification == "output")
				{
					//EngineGovernor.log("DEBUG - : Found an output tag", 1);

					string outputDefinition = tagDefinition;
					CodeObject codeOutput = new CodeObject();
					codeOutput.Description = outputDefinition;
					codeOutput.CodeType = "output";


					//don't forget, after output, we can assume that both a function and any waiting input is officially finished
					//(output will ONLY ever be from a function, not a constructor or a property, ONLY function)
					if (waitingFunction == null)
					{
						EngineGovernor.log("WARNING - Found an output, but no function. Output tags can only be used with a function.", -1);
						EngineGovernor.log("Warning text: output was '" + outputDefinition + "'", -1);
					}
					else
					{
						//if waiting input, end it, we've reached the end by this point!
						CodeObject codeInput = null;
						if (waitingInput != null)
						{
							//EngineGovernor.log("DEBUG - : : Since we've reached an output, the input is now complete.", 1);
							codeInput = waitingInput;
							//EngineGovernor.log("DEBUG - : : Input finished, nullifying waitingInput.", 1);
							waitingInput = null;
						}
						//now end the function
						if (codeInput != null) //if there was an input, go ahead and add it to the function
						{ 
							waitingFunction.Children.Add(codeInput);
							//EngineGovernor.log("DEBUG - : : Added input to waitingFunction.", 1);
						} 

						CodeObject finishedFunction = waitingFunction;
						finishedFunction.Children.Add(codeOutput);
						//EngineGovernor.log("DEBUG - : : Added output to finishedFunction.", 1);
						foundObjects.Add(finishedFunction);
						//EngineGovernor.log("DEBUG - : : Function finished, nullifying waitingFunction.", 1);
						waitingFunction = null;
					}
				}
			}

			//EngineGovernor.log("DEBUG - : Cleaning up and finishing all waiting objects...", 1);
			//now take care of any waiting objects that haven't been finished yet
			if (waitingClass != null)
			{
				EngineGovernor.log("WARNING - Found class with no description.", -1);
				CodeObject finishedClass = waitingClass;
				foundObjects.Add(finishedClass);
				//EngineGovernor.log("DEBUG - : : Adding and nullifying waitingClass anyway.", 1);
				waitingClass = null;
			}
			if (waitingInterface != null)
			{
				EngineGovernor.log("WARNING - Found interface with no description.", -1);
				CodeObject finishedInterface = waitingInterface;
				foundObjects.Add(finishedInterface);
				//EngineGovernor.log("DEBUG - : : Adding and nullifying waitingInterface anyway.", 1);
				waitingInterface = null;
			}
			if (waitingVariable != null)
			{
				EngineGovernor.log("WARNING - Found variable with no description.", -1);
				EngineGovernor.log("Warning text: Variable '" + waitingVariable.Name + "'", -1);
				CodeObject finishedVariable = waitingVariable;
				if (waitingInput != null)
				{
					waitingInput.Children.Add(finishedVariable);
				}
				else //free variable
				{
					foundObjects.Add(finishedVariable);
				}
				waitingVariable = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingVariable.", 1);
			}
			if (waitingInput != null && waitingConstructor != null) //if we got to this point, it means that a constructor had an input
			{
				//EngineGovernor.log("DEBUG - : : Found both an unfinished input and constructor. Putting them together.", 1);
				CodeObject finishedInput = waitingInput;
				CodeObject finishedConstructor = waitingConstructor;
				finishedConstructor.Children.Add(finishedInput);
				foundObjects.Add(finishedConstructor);
				waitingInput = null;
				waitingConstructor = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingInput and waitingConstructor", 1);
			}
			if (waitingInput != null && waitingFunction != null) //this is a function with input but not any output
			{
				//EngineGovernor.log("DEBUG - : : Found an unfinished input and function. Putting them together.", 1);
				CodeObject finishedInput = waitingInput;
				CodeObject finishedFunction = waitingFunction;
				finishedFunction.Children.Add(finishedInput);
				foundObjects.Add(finishedFunction);
				waitingInput = null;
				waitingFunction = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingInput and waitingFunction", 1);
			}
			if (waitingProperty != null)
			{
				//EngineGovernor.log("DEBUG - : : Found an unfinished property.", 1);
				CodeObject finishedProperty = waitingProperty;
				foundObjects.Add(finishedProperty);
				waitingProperty = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingProperty.", 1);
			}
			if (waitingFunction != null)
			{
				//EngineGovernor.log("DEBUG - : : Found an unfinished function.", 1);
				CodeObject finishedFunction = waitingFunction;
				foundObjects.Add(finishedFunction);
				waitingFunction = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingFunction.", 1);
			}
			if (waitingConstructor != null)
			{
				//EngineGovernor.log("DEBUG - : : Found an unfinished constructor.", 1);
				CodeObject finishedConstructor = waitingConstructor;
				foundObjects.Add(finishedConstructor);
				waitingConstructor = null;
				//EngineGovernor.log("DEBUG - : : Nullifying waitingConstructor.", 1);
			}

			//last checks
			if (waitingClass != null) { EngineGovernor.log("WARNING - waiting class object leftover after cleaning.", -1); }
			if (waitingConstant != null) { EngineGovernor.log("WARNING - waiting constant object leftover after cleaning.", -1); }
			if (waitingConstructor != null) { EngineGovernor.log("WARNING - waiting constructor object leftover after cleaning.", -1); }
			if (waitingFunction != null) { EngineGovernor.log("WARNING - waiting function object leftover after cleaning.", -1); }
			if (waitingInput != null) { EngineGovernor.log("WARNING - waiting input object leftover after cleaning.", -1); }
			if (waitingInterface != null) { EngineGovernor.log("WARNING - waiting interface object leftover after cleaning.", -1); }
			if (waitingVariable != null) { EngineGovernor.log("WARNING - waiting variable object leftover after cleaning.", -1); }
			if (waitingProperty != null) { EngineGovernor.log("WARNING - waiting property object leftover after cleaning.", -1); }


			//EngineGovernor.log("DEBUG - : Finished cleaning up waiting objects.", 1);

			//unitTestDumpCodeObjects(foundObjects);
			return foundObjects;
		}

		//this function gets all the tags (@something) in the source string
		//list format is as follows:
		// index 'i': tag type (ex: 'function' or 'f')
		// index 'i+1': tag content (ex: 'static BuildTrainingFacility getTaskBySectorID')
		// NOTE: IGNORES LINK TAGS. LINKING IS HANDLED IN A DIFFERENT PASS
		private List<string> findTags(string source)
		{
			EngineGovernor.log("\tBuilding tag list...");
			//EngineGovernor.log("DEBUG - Searching for all tags in '" + source + "'", 1);
			List<string> pass = new List<string>();
			string updatedSource = source;
			
			//------------------------------------------------------
			// ---- PASS ONE ---- 1111111111111111111111111111111111
			//------------------------------------------------------
			//first pass, find EVERY tag

			//find the first tag
			while (updatedSource.Contains("@"))
			{
				int tagIndex = updatedSource.IndexOf("@");
				//EngineGovernor.log("DEBUG - : Found a tag delimiter at " + tagIndex, 1);

				//check to make sure this tag delimiter isn't at the end
				if (tagIndex == updatedSource.Length - 1) { EngineGovernor.log("WARNING - Found a tag delimiter at the end of a syntax block. Check for incorrectly specified tags.", -1); break; }
				updatedSource = updatedSource.Substring(tagIndex + 1);

				//now find the end of the tag specification (the ':') also find end of the entire tag definition, (next time it hits a new tag delimiter, '@')
				//to make sure that the end tag specification is BEFORE the next tag
				int nextTag = updatedSource.IndexOf("@");
				int tagDefEnd = nextTag;
				if (nextTag == -1) { EngineGovernor.log("DEBUG - : Determined that this is the last tag in the syntax block.", 1); tagDefEnd = updatedSource.Length; }
				//EngineGovernor.log("DEBUG - : length of this tag is " + tagDefEnd, 1);

				//assign current working tag
				string thisTag = updatedSource.Substring(0, tagDefEnd);
				//EngineGovernor.log("DEBUG - : FOUND TAG - '" + thisTag + "'", 1);

				if (!thisTag.Contains(":"))
				{
					EngineGovernor.log("WARNING - Tag does not have colon ':', can't separate tag specification from definition.", -1);
					continue;
				}

				string tagSpecification = "";
				string tagDefinition = "";

				tagDefinition = thisTag.Substring(0, thisTag.IndexOf(":"));
				if (thisTag.IndexOf(":") >= thisTag.Length - 1)
				{
					//EngineGovernor.log("DEBUG - : : Tag definition blank. (Colon at the end of the line)", 1);
				}
				else
				{
					tagSpecification = thisTag.Substring(thisTag.IndexOf(":") + 1);
				}
				//EngineGovernor.log("DEBUG - : : Tag definition is '" + tagDefinition + "' and specification is '" + tagSpecification + "'", 1);
				//EngineGovernor.log("DEBUG - : : Trimming tag specification...", 1);
				tagSpecification = tagSpecification.Trim();
				pass.Add(tagDefinition);
				pass.Add(tagSpecification);
			}

			//unitTestDumpTags(pass, 1);

			//------------------------------------------------------
			// ---- PASS TWO ---- 2222222222222222222222222222222222
			//------------------------------------------------------
			//second pass, add links back into the previous tag definition

			//EngineGovernor.log("DEBUG - : Sending tags through second pass to untag links...", 1);

			//take current entries of @link and move them back to a non-null line, then make the lines that previously contained just the @link to @NULL
			for (int i = 2; i < pass.Count; i += 2)
			{
				//EngineGovernor.log("DEBUG - : : index " + i, 1);
				//EngineGovernor.log("DEBUG - : : line '" + pass[i] + "'", 1);
				if (pass[i] == "l" || pass[i] == "link")
				{
					//EngineGovernor.log("DEBUG - : : : Found a link tag", 1);
					int backIndex = i - 1;
					while (pass[backIndex] == "@NULL") 
					{
						backIndex -= 2; //in case of two link tags in a row (keep on going back until a non-null row is found)
						//EngineGovernor.log("DEBUG - : : : last tag was already a link, and has already been nullified, moving back a little further...", 1);
					}
					pass[backIndex] += " @l:" + pass[i + 1];
					pass[i] = "@NULL";
					pass[i + 1] = "@NULL";
				}
			}

			//unitTestDumpTags(pass, 2);

			//------------------------------------------------------
			// ---- PASS THREE ---- 33333333333333333333333333333333
			//------------------------------------------------------
			//third pass, remove all instances of @NULL from the list

			//EngineGovernor.log("DEBUG - : Removing instances of @NULL from tag list", 1);

			while (pass.IndexOf("@NULL") != -1)
			{
				int removalIndex = pass.IndexOf("@NULL");
				//EngineGovernor.log("DEBUG - : : Found an @NULL at " + removalIndex + ", removing...", 1);
				pass.Remove("@NULL");
			}

			//EngineGovernor.log("DEBUG - : All instances of @NULL removed.", 1);

			//unitTestDumpTags(pass, 3);

			//EngineGovernor.log("DEBUG - : Tag list built! Returning it now!", 1);
			return pass;
		}



		//unit tests
		private void unitTestPrintCommentBlockLists()
		{
			//print out comment blocks line index
			foreach (List<int> lineIndexList in m_cCommentBlocksLineIndex)
			{
				foreach (int i in lineIndexList)
				{
					EngineGovernor.log("CommentBlocksLineIndex: " + i, 2);
				}
			}
			EngineGovernor.log("---------------------------------------------------------", 2);
			for (int i = 0; i < m_cCommentBlocksLineIndex.Count; i++)
			{
				for (int j = 0; j < m_cCommentBlocksLineIndex[i].Count; j++)
				{
					EngineGovernor.log("CommnetBlocksLineIndex (alternate for loops): " + m_cCommentBlocksLineIndex[i][j], 2);
				}
			}
			EngineGovernor.log("---------------------------------------------------------", 2);
			for (int i = 0; i < m_cCommentBlocksLineIndex.Count; i++)
			{
				for (int j = 0; j < m_cCommentBlocksLineIndex[i].Count; j++)
				{
					EngineGovernor.log("commentblocksLineIndex cross referenced with fileLinesTypes: " + m_cCommentBlocksLineIndex[i][j] + " type: " + m_cFileLinesTypes[m_cCommentBlocksLineIndex[i][j]], 2);
				}
			}
			EngineGovernor.log("---------------------------------------------------------", 2);

			for (int j = 0; j < m_cFileLinesTypes.Count; j++)
			{
				EngineGovernor.log("fileLinesTypes: j:" + j + " - " + m_cFileLinesTypes[j], 2);
			}

		}

		private void unitTestSyntaxBlocks()
		{
			foreach (List<string> syntaxBlock in m_cSyntaxBlocks)
			{
				EngineGovernor.log("SYNTAXBLOCKBELOW", 2);
				foreach (string line in syntaxBlock)
				{
					EngineGovernor.log("syntaxPart: " + line, 2);
				}
			}
		}

		private void unitTestDumpTags(List<string> dump, int pass)
		{
			EngineGovernor.log("------------------------------------------------", 2);
			foreach (string s in dump)
			{
				EngineGovernor.log("tag dump " + pass + " - '" + s + "'", 2);
			}
		}
	
		private void unitTestDumpCodeObjects(List<CodeObject> objs)
		{
			EngineGovernor.log("------------------------code objects-----------------------------------", 2);
			foreach (CodeObject obj in objs)
			{
				EngineGovernor.log(obj.ToString(), 2);
				foreach (CodeObject obj2 in obj.Children)
				{
					EngineGovernor.log(obj2.ToString(), 2);
					foreach (CodeObject obj3 in obj2.Children)
					{
						EngineGovernor.log(obj3.ToString(), 2);
						foreach (CodeObject obj4 in obj3.Children)
						{
							EngineGovernor.log(obj4.ToString(), 2);
						}
					}
				}
			}
		}
	}
}
