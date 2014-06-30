debug:
	cd external && $(MAKE)
	cd Compiler/skc5 && $(MAKE)
	cd Compiler/MSBuild && $(MAKE)
	cd SDK && $(MAKE)
	cd Integration/MonoDevelop && $(MAKE)

release:
	cd external && $(MAKE) release
	cd Compiler/skc5 && $(MAKE) release
	cd Compiler/MSBuild && $(MAKE) release
	cd SDK && $(MAKE) relase
	cd Integration/MonoDevelop && $(MAKE) release
