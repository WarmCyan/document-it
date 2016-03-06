
window.onload = main;


var classList = [];
var interfaceList = [];
var fileList = [];

var sectionList = [];

var selectedSectionIndex = 0;

function main()
{
	console.log("Hello there!");
	
	
	parseLists();
	alphabtizeLists();

	checkSectionExistence();

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

function refreshSidebarSectionContents()
{
	clearSidebarSectionContents();
	if (classList.length > 0) { printOutClasses(); }
	if (interfaceList.length > 0) { printOutInterfaces(); }
	if (fileList.length > 0) { printOutFiles(); }
}

function clearSidebarSectionContents() { document.getElementById("side_section_contents").innerHTML = ""; }

function printOutClasses()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Classes</h4>";
	for (var i = 0; i < classList.length; i++)
	{
		if (classList[i][1] == selectedSectionIndex) { obj.innerHTML += "<p><a href='thing.html'>" + classList[i][0] + "</a></p>"; }
	}
	obj.innerHTML += "&nbsp;";
}

function printOutInterfaces()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Interfaces</h4>";
	for (var i = 0; i < interfaceList.length; i++)
	{
		if (interfaceList[i][1] == selectedSectionIndex) { obj.innerHTML += "<p><a href='thing.html'>" + interfaceList[i][0] + "</a></p>"; }
	}
	obj.innerHTML += "&nbsp;";
}

function printOutFiles()
{
	var obj = document.getElementById("side_section_contents");

	obj.innerHTML += "<h4 class='sectionLabel'>Files</h4>";
	for (var i = 0; i < fileList.length; i++)
	{
		if (fileList[i][1] == selectedSectionIndex) { obj.innerHTML += "<p><a href='thing.html'>" + fileList[i][0] + "</a></p>"; }
	}
	obj.innerHTML += "&nbsp;";
}
