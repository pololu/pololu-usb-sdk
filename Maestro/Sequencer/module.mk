Sequencer_lib := $(Sequencer)/Sequencer.dll
Targets += $(Sequencer)/Sequencer.dll

Sequencer_files := $(Sequencer)/Sequence.cs $(Sequencer)/Frame.cs

$(Sequencer)/Sequencer.dll: $(Sequencer_files)
	$(CS) -target:library -out:$@ $(Sequencer_files) -r:/usr/lib/mono/2.0/System.Windows.Forms.dll