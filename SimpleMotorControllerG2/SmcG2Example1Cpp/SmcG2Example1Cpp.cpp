// SmcG2Example1Cpp.cpp : main project file.

#include "stdafx.h"
#include "MainWindow.h"

using namespace Pololu::SimpleMotorControllerG2::SmcG2Example1Cpp;

[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
	// Enabling Windows XP visual effects before any controls are created
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false); 

	// Create the main window and run it
	Application::Run(gcnew MainWindow());
	return 0;
}
