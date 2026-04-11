STATICS_RELEASE=eb111fb8-7474-4f75-a1b7-848fc6293aa5

statics:
	mkdir statics
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FAudio.a -O statics/FAudio.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/FNA3D.a -O statics/FNA3D.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/libmojoshader.a -O statics/libmojoshader.a
	wget https://github.com/r58Playz/FNA-WASM-Build/releases/download/$(STATICS_RELEASE)/SDL3.a -O statics/SDL3.a

FNA:
	git clone https://github.com/FNA-XNA/FNA --recursive -b 26.04
	cd FNA && git apply ../FNA.patch

clean:
	rm -rvf statics obj bin FNA || true

build: statics FNA
	dotnet publish -c Release -v d
	# fix mono init with -sWASMFS enabled
	sed -i 's/FS_createPath("\/","usr\/share",!0,!0)/FS_createPath("\/usr","share",!0,!0)/' bin/Release/net10.0/publish/wwwroot/_framework/dotnet.runtime.*.js
	# automatically force transfer of canvas matching selector `.canvas` (class canvas) to deputy thread (c# managed main thread)
	sed -i 's/var offscreenCanvases={};/var offscreenCanvases={};if(globalThis.window\&\&!window.TRANSFERRED_CANVAS){transferredCanvasNames=[".canvas"];window.TRANSFERRED_CANVAS=true;}/' bin/Release/net10.0/publish/wwwroot/_framework/dotnet.native.*.js

serve: build
	python3 tools/serve.py
