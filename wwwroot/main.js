
const wasm = await eval(`import("/_framework/dotnet.js")`);
const dotnet = wasm.dotnet;

console.debug("initializing dotnet");
const runtime = await dotnet.withConfig({
	jsThreadBlockingMode: "DangerousAllowBlockingWait",
}).create();

const config = runtime.getConfig();
const exports = await runtime.getAssemblyExports(config.mainAssemblyName);
const canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;

self.wasm = {
	Module: dotnet.instance.Module,
	dotnet,
	runtime,
	config,
	exports,
	canvas,
};

console.debug("PreInit...");
await runtime.runMain();
await exports.Program.PreInit();
console.debug("dotnet initialized");

console.debug("Init...");
exports.Program.Init();

console.debug("MainLoop...");
const main = () => {
	const ret = exports.Program.MainLoop();

	if (!ret) {
		console.debug("Cleanup...");
		exports.Program.Cleanup();
		return;
	}

	requestAnimationFrame(main);
}
requestAnimationFrame(main);
