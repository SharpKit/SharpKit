debug:
	cd external && $(MAKE)
	cd Compiler && $(MAKE)
	cd SDK && $(MAKE)
	cd Integration/MonoDevelop && $(MAKE)

release:
	cd external && $(MAKE) release
	cd Compiler && $(MAKE) release
	cd SDK && $(MAKE) release
	cd Integration/MonoDevelop && $(MAKE) release

release-minimal:
	cd external && $(MAKE) release
	cd Compiler && $(MAKE) release
	cd SDK/Defs/JavaScript && xbuild
	cd SDK/Defs/Html && xbuild
	cd SDK/Defs/jQuery && xbuild

clean:
	rm -rf \
		*/bin */*/bin */*/*/bin */*/*/*/bin */*/*/*/*/bin \
		*/obj */*/obj */*/*/obj */*/*/*/obj */*/*/*/*/obj

