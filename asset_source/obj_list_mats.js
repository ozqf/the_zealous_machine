
function listMaterials(lines) {
	let count = 0;
	lines.forEach((line, i)=> {
		if (line[0] === 'u') {
			let name = line.split(' ')[1];
			console.log(`${count}: ${name}`);
			count += 1;
		}
	});
}


if (process.argv.length < 3) {
	console.log(`Need an obj file to open!`);
	return;
}

const g_inputPath = process.argv[2];
console.log(`Scanning obj file ${g_inputPath}`);

const g_lines = require("fs").readFileSync(g_inputPath, "utf-8").split("\n");
console.log(`Read ${g_lines.length} lines from ${g_inputPath}`);
listMaterials(g_lines);
