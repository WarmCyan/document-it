//********************************************************
// File: CodeDocument.cs
// Author: Nathan Martindale
// Date Created: 1/15/2015
// Date Edited: 1/15/2015
// Copyright © 2015 Digital Warrior Labs
// Description: CodeObject 'container'
//********************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
	public class CodeDocument
	{
		//member variables
		private string m_sFileName = "";
		private List<CodeObject> m_cCodeObjects = new List<CodeObject>();

		//construction
		public CodeDocument() { }

		public List<CodeObject> CodeObjects { get { return m_cCodeObjects; } set { m_cCodeObjects = value; } }

	}
}
