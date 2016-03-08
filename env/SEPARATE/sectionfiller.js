
window.onload = main;

var doubleClickReady = false;

var sidebarCollapsed = false;


var classList = [];
var interfaceList = [];
var fileList = [];

var sectionList = [];

var selectedSectionIndex = 0;
//var selectedEntityIndex = 0; 

function main()
{
	console.log("Hello there!");
	
	
	parseLists();
	alphabtizeLists();

	checkSectionExistence();

	refreshSidebarSections();
	refreshSidebarSectionContents();
}


function parseLists()
{
	console.log("parsing lists");

	// section list (though technically just copying the array, leaving this here in case further action is ever necessary) 
	for (var i = 0; i < SECTION_LIST.length; i++) 
	{ 
		var current = SECTION_LIST[i].split(',');
		sectionList.push(current);
	}
	
	// class list
	for (var i = 0; i < CLASS_LIST.length; i++)
	{
		var current = CLASS_LIST[i].split(',');
		classList.push(current);
	}

	// interface list
	for (var i = 0; i < INTERFACE_LIST.length; i++)
	{
		var current = INTERFACE_LIST[i].split(',');
		classList.push(current);
	}

	// file list
	for (var i = 0; i < FILE_LIST.length; i++)
	{
		var current = FILE_LIST[i].split(',');
		classList.push(current);
	}

	// DEBUG!!!!
	//sectionList = [["All", 1]];
}

function alphabtizeLists()
{
	classList.sort();
	interfaceList.sort();
	fileList.sort();
}

function checkSectionExistence()
{
	if (sectionList[0][0] == "All" && sectionList.length == 1)
	{
		// remove the sections section in the sidebar 
		document.getElementById("side_sections").style.height = 0;
		document.getElementById("side_section_contents").style.height = "100%";
	}
}

function setSection(sectionID)
{
	selectedSectionIndex = sectionID;
	refreshSidebarSections();
	refreshSidebarSectionContents();
}

function refreshSidebarSectionContents()
{
	clearSidebarSectionContents();
	if (classList.length > 0) { printOutClasses(); }
	if (interfaceList.length > 0) { printOutInterfaces(); }
	if (fileList.length > 0) { printOutFiles(); }
}

function refreshSidebarSections()
{
	clearSidebarSections();
	printOutSections();
}

function clearSidebarSectionContents() { document.getElementById("side_section_contents").innerHTML = ""; }
function clearSidebarSections() { document.getElementById("side_sections").innerHTML = ""; }

function printOutSections()
{
	var obj = document.getElementById("side_sections");

	obj.innerHTML += "<h4 class='sectionLabel'>Sections</h4>";
	for (var i = 0; i < sectionList.length; i++)
	{
		var innerString = "<div onclick='setSection(" + i + ");'";
		if (i == selectedSectionIndex) { innerString += " class='sectionSelected'"; }
		innerString += "><p>" + sectionList[i][0] + "</p></div>";
		obj.innerHTML += innerString;
		
		/*obj.innerHTML += "<div onclick='setSection(" + i + ");'";
		if (i == selectedSectionIndex) { obj.innerHTML += " class='sectionSelected'"; }
		obj.innerHTML +="><p>" + sectionList[i][0] + "</p></div>";*/
		
		
		
		
		/*if (i == selectedSectionIndex) { obj.innerHTML += "<div class='sectionSelected'><p>" + sectionList[i][0] + "</p></div>"; }
		else { obj.innerHTML += "<div><p>" + sectionList[i][0] + "</p></div>"; }*/
	}
}

function printOutClasses()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Classes</h4>";
	for (var i = 0; i < classList.length; i++)
	{
		if (classList[i][1] == selectedSectionIndex) { obj.innerHTML += "<a href='other.html'><div><p>" + classList[i][0] + "</p></div></a>"; }
	}
	obj.innerHTML += "&nbsp;";
}

function printOutInterfaces()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Interfaces</h4>";
	for (var i = 0; i < interfaceList.length; i++)
	{
		if (interfaceList[i][1] == selectedSectionIndex) { obj.innerHTML += "<a href='other.html'><div><p>" + interfaceList[i][0] + "</p></div></a>"; }
	}
	obj.innerHTML += "&nbsp;";
}

function printOutFiles()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Files</h4>";
	for (var i = 0; i < fileList.length; i++)
	{
		if (fileList[i][1] == selectedSectionIndex) { obj.innerHTML += "<a href='other.html'><div><p>" + fileList[i][0] + "</p></div></a>"; }
	}
	obj.innerHTML += "&nbsp;";
}

function toggleCollapseSidebar()
{
	if (sidebarCollapsed)
	{
		document.getElementById("side_bar").style.width = "300px";
		document.getElementById("content").style.marginLeft = "300px";
		document.getElementById("bottom_band").style.marginLeft = "300px";
		sidebarCollapsed = false;
	}
	else
	{
		document.getElementById("side_bar").style.width = "10px";
		document.getElementById("content").style.marginLeft = "10px";
		document.getElementById("bottom_band").style.marginLeft = "10px";
		sidebarCollapsed = true;
	}
}


// -- EVENTS --

function sidebarClick()
{
	if (doubleClickReady) { toggleCollapseSidebar(); }
	doubleClickReady = true;
	setTimeout(function() { doubleClickReady = false; }, 225);
}
