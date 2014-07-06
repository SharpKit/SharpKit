debug:
	cd external && $(MAKE)
	cd Compiler && $(MAKE)
	cd SDK && $(MAKE)
	cd Integration/MonoDevelop && $(MAKE)

release:
	cd external && $(MAKE) release
	cd Compiler && $(MAKE) release
	cd SDK && $(MAKE) relase
	cd Integration/MonoDevelop && $(MAKE) release
