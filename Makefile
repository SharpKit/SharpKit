compile:
	cd external && $(MAKE)
	cd Compiler/skc5 && $(MAKE)
	cd Compiler/MSBuild && $(MAKE)
