# Generate a unique list of files that need to be in the same
# directory as UscCmd at runtime (runtime dependencies).
UscCmd_runtime := $(sort $(Bytecode_lib) $(UsbWrapper_lib) $(Usc_lib))

# Compile-time dependencies.
UscCmd_dlls := $(Bytecode)/Bytecode.dll $(UsbWrapper)/UsbWrapper.dll $(Usc)/Usc.dll
UscCmd_csfiles := $(UscCmd)/CommandOptions.cs $(UscCmd)/Program.cs $(UscCmd)/Properties/AssemblyInfo.cs

# Required module variables
Targets += $(UscCmd)/UscCmd.exe
Byproducts += $(foreach dll, $(UscCmd_runtime), $(UscCmd)/$(notdir $(dll)))

$(UscCmd)/UscCmd.exe: $(UscCmd_csfiles) $(UscCmd_runtime)
	cp $(UscCmd_runtime) $(UscCmd)
	$(CS) -target:exe -out:$@ $(UscCmd_csfiles) $(foreach dll, $(UscCmd_dlls),-r:$(UscCmd)/$(notdir $(dll)))