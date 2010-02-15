# Generate a unique list of files that need to be in the same
# directory as the executable at runtime (runtime dependencies).
MaestroExample_runtime := $(sort $(Bytecode_lib) $(UsbWrapper_lib) $(Usc_lib))

# Compile-time dependencies.
MaestroExample_dlls := $(Bytecode)/Bytecode.dll $(UsbWrapper)/UsbWrapper.dll $(Usc)/Usc.dll
MaestroExample_csfiles := $(MaestroExample)/MainWindow.cs $(MaestroExample)/MainWindow.Designer.cs $(MaestroExample)/Program.cs $(wildcard $(MaestroExample)/Properties/*.cs)

# These two resources files don't seem to be needed for compilation, but
# we include them here in case you need them in the future.  This variable
# defines which .resources files are needed.  The rule for making .resources
# files is defined in the Makefile.
MaestroExample_resources := \
	$(MaestroExample)/MainWindow.resources \
	$(MaestroExample)/Properties/Resources.resources

MaestroExample_resourceids := \
	Pololu.Usc.MaestroExample.MainWindow.resources \
	Pololu.Usc.MaestroExample.Properties.Resources.resources

MaestroExample_resource_args = $(join \
	$(foreach res, $(MaestroExample_resources), -resource:$(res)), \
	$(foreach id, $(MaestroExample_resourceids), ,$(id)))

# Required module variables
Targets += $(MaestroExample)/MaestroExample $(MaestroExample_resources)
Byproducts += $(foreach dll, $(MaestroExample_runtime), $(MaestroExample)/$(notdir $(dll)))

MaestroExample_StandardLibs := \
	-r:/usr/lib/mono/2.0/System.dll \
	-r:/usr/lib/mono/2.0/System.Core.dll \
	-r:/usr/lib/mono/2.0/System.Data.dll \
	-r:/usr/lib/mono/2.0/System.Drawing.dll \
	-r:/usr/lib/mono/2.0/System.Windows.Forms.dll \

$(MaestroExample)/MaestroExample: $(MaestroExample_csfiles) $(MaestroExample_runtime) $(MaestroExample_resources)
	cp $(MaestroExample_runtime) $(MaestroExample)
	$(CS) -target:exe -out:$@.exe $(MaestroExample_csfiles) $(foreach dll, $(MaestroExample_dlls),-r:$(MaestroExample)/$(notdir $(dll))) $(MaestroExample_StandardLibs) $(MaestroExample_resource_args)
	mv $@.exe $@

# Alias so you can type "make maestroexample"
maestroexample: $(MaestroExample)/MaestroExample