# Generate a unique list of files that need to be in the same
# directory as the executable at runtime (runtime dependencies).
SmcG2Example2_runtime := $(sort $(UsbWrapper_lib) $(SmcG2_lib))

# Compile-time dependencies.
SmcG2Example2_dlls := $(UsbWrapper)/UsbWrapper.dll $(SmcG2)/SmcG2.dll
SmcG2Example2_csfiles := $(SmcG2Example2)/MainWindow.cs $(SmcG2Example2)/MainWindow.Designer.cs $(SmcG2Example2)/Program.cs $(wildcard $(SmcG2Example2)/Properties/*.cs)

# This variable defines which .resources files are needed.  The
# rule for making .resources files is defined in the Makefile.
SmcG2Example2_resources := \
	$(SmcG2Example2)/MainWindow.resources \
	$(SmcG2Example2)/Properties/Resources.resources
SmcG2Example2_resourceids := \
	Pololu.SimpleMotorControllerG2.SmcG2Example2.MainWindow.resources \
	Pololu.SimpleMotorControllerG2.SmcG2Example2.Properties.Resources.resources
SmcG2Example2_resource_args = $(join \
	$(foreach res, $(SmcG2Example2_resources), -resource:$(res)), \
	$(foreach id, $(SmcG2Example2_resourceids), ,$(id)))

# Required module variables
Targets += $(SmcG2Example2)/SmcG2Example2 $(SmcG2Example2_resources)
Byproducts += $(foreach dll, $(SmcG2Example2_runtime), $(SmcG2Example2)/$(notdir $(dll)))

$(SmcG2Example2)/SmcG2Example2: $(SmcG2Example2_csfiles) $(SmcG2Example2_runtime) $(SmcG2Example2_resources)
	cp $(SmcG2Example2_runtime) $(SmcG2Example2)
	$(CS) -target:exe -out:$@.exe $(SmcG2Example2_csfiles) $(foreach dll, $(SmcG2Example2_dlls),-r:$(SmcG2Example2)/$(notdir $(dll))) $(Mono_StandardLibs) $(SmcG2Example2_resource_args)
	mv $@.exe $@

smcg2example2: $(SmcG2Example2)/SmcG2Example2
