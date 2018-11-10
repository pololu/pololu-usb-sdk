Targets += $(SmcG2)/SmcG2.dll

# Run-time dependencies.
SmcG2_lib := $(UsbWrapper_lib) $(SmcG2)/SmcG2.dll

# Compile-time dependencies.
SmcG2_csfiles := $(wildcard $(SmcG2)/*.cs) $(SmcG2)/Properties/AssemblyInfo.cs
SmcG2_dlls := $(UsbWrapper)/UsbWrapper.dll

$(SmcG2)/SmcG2.dll: $(SmcG2_dlls) $(Smc_csfiles)
	$(CS) -target:library -out:$@ $(foreach dll, $(SmcG2_dlls),-r:$(dll)) $(SmcG2_csfiles)
