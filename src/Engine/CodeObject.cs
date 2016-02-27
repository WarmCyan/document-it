//********************************************************
// File: CodeObject.cs
// Author: Nathan Martindale
// Date Created: 1/15/2015
// Date Edited: 1/19/2015
// Copyright © 2015 Digital Warrior Labs
// Description: Object that represents a code THING (function, class, variable, property, etc.)
//********************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
	public class CodeObject
	{
		//member variables
		private String m_sName = "";
		private string m_sDescription = "";
		private string m_sType = ""; //the virtual type (int, string, etc)
		private string m_sCodeType = ""; //represents what type of code object this is, possibilities: "class", "interface", "constructor", "variable", "constant", "property", "output", "function
		private List<CodeObject> m_cChildren = new List<CodeObject>();

		//construction
		public CodeObject() { }
		public CodeObject(string name, string description, string type, string codeType, List<CodeObject> children)
		{
			m_sName = name;
			m_sDescription = description;
			m_sType = type;
			m_sCodeType = codeType;
			m_cChildren = children;
		}

		//properties
		public string Name { get { return m_sName; } set { m_sName = value; } }
		public string Description { get { return m_sDescription; } set { m_sDescription = value; } }
		public string Type { get { return m_sType; } set { m_sType = value; } }
		public string CodeType { get { return m_sCodeType; } set { m_sCodeType = value; } }
		public List<CodeObject> Children { get { return m_cChildren; } set { m_cChildren = value; } }


		//specific codeObject properties
		public List<CodeObject> Constants
		{
			get
			{
				List<CodeObject> constantCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "constant") { constantCollection.Add(obj); }
				}
				return constantCollection;
			}
		}
		public List<CodeObject> Constructors
		{
			get
			{
				List<CodeObject> constructorCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "constructor") { constructorCollection.Add(obj); }
				}
				return constructorCollection;
			}
		}
		public List<CodeObject> Functions
		{
			get
			{
				List<CodeObject> functionCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "function") { functionCollection.Add(obj); }
				}
				return functionCollection;
			}
		}
		public List<CodeObject> Properties
		{
			get
			{
				List<CodeObject> propertyCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "property") { propertyCollection.Add(obj); }
				}
				return propertyCollection;
			}
		}
		public List<CodeObject> Inputs
		{
			get
			{
				List<CodeObject> inputCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "input") { inputCollection.Add(obj); }
				}
				return inputCollection;
			}
		}
		public List<CodeObject> Variables
		{
			get
			{
				List<CodeObject> variableCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "variable") { variableCollection.Add(obj); }
				}
				return variableCollection;
			}
		}
		public List<CodeObject> Outputs
		{
			get
			{
				List<CodeObject> outputCollection = new List<CodeObject>();
				foreach (CodeObject obj in Children)
				{
					if (obj.CodeType == "output") { outputCollection.Add(obj); }
				}
				return outputCollection;
			}
		}



		public string ToString()
		{
			string top = "Code Object - Name: '" + m_sName + "' Type: '" + m_sType + "' Description: '" + m_sDescription + "' Code Type: '" + m_sCodeType + "'";
			/*foreach (CodeObject obj in m_cChildren)
			{
				top += "\n" + obj.ToString();
			}*/
			return top;
		}
	}
}
