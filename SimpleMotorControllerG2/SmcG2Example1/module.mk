# Generate a unique list of files that need to be in the same
# directory as the executable at runtime (runtime dependencies).
SmcG2Example1_runtime := $(sort $(UsbWrapper_lib) $(SmcG2_lib))

# Compile-time dependencies.
SmcG2Example1_dlls := $(UsbWrapper)/UsbWrapper.dll $(SmcG2)/SmcG2.dll
SmcG2Example1_csfiles := $(SmcG2Example1)/MainWindow.cs \
  $(SmcG2Example1)/MainWindow.Designer.cs \
	$(SmcG2Example1)/Program.cs \
	$(wildcard $(SmcG2Example1)/Properties/*.cs)

# This variable defines which .resources files are needed.  The
# rule for making .resources files is defined in the Makefile.
SmcG2Example1_resources := \
	$(SmcG2Example1)/MainWindow.resources \
	$(SmcG2Example1)/Properties/Resources.resources
SmcG2Example1_resourceids := \
	Pololu.SimpleMotorControllerG2.SmcG2Example1.MainWindow.resources \
	Pololu.SimpleMotorControllerG2.SmcG2Example1.Properties.Resources.resources
SmcG2Example1_resource_args = $(join \
	$(foreach res, $(SmcG2Example1_resources), -resource:$(res)), \
	$(foreach id, $(SmcG2Example1_resourceids), ,$(id)))

# Required module variables
Targets += $(SmcG2Example1)/SmcG2Example1 $(SmcG2Example1_resources)
Byproducts += $(foreach dll, $(SmcG2Example1_runtime), $(SmcG2Example1)/$(notdir $(dll)))

$(SmcG2Example1)/SmcG2Example1: $(SmcG2Example1_csfiles) $(SmcG2Example1_runtime) $(SmcG2Example1_resources)
	cp $(SmcG2Example1_runtime) $(SmcG2Example1)
	$(CS) -target:exe -out:$@.exe $(SmcG2Example1_csfiles) $(foreach dll, $(SmcG2Example1_dlls),-r:$(SmcG2Example1)/$(notdir $(dll))) $(Mono_StandardLibs) $(SmcG2Example1_resource_args)
	mv $@.exe $@

smcg2example1: $(SmcG2Example1)/SmcG2Example1
